using System.Text;
using AccountService.gRPC.Services;
using AccountService.Library;
using AccountService.Library.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

IConfiguration configuration = new ConfigurationBuilder()
   .AddJsonFile("appsettings.json", true, true)
   .Build();

var dbConnectionString = Environment.GetEnvironmentVariable("DefaultConnection");

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
{
    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
        Server = Environment.GetEnvironmentVariable("Server"),
        Database = Environment.GetEnvironmentVariable("Database"),
        UserID = Environment.GetEnvironmentVariable("UserID"),
        Password = Environment.GetEnvironmentVariable("Password")
    };

    if (Environment.GetEnvironmentVariable("Platform") == "Cloud Run")
    {
        connectionStringBuilder.ConnectionProtocol = MySqlConnectionProtocol.UnixSocket;
    }

    var port = Environment.GetEnvironmentVariable("Port");
    if (!string.IsNullOrEmpty(port))
    {
        connectionStringBuilder.Port = uint.Parse(port);
    }

    dbConnectionString = connectionStringBuilder.ToString();
}

logger.LogInformation($"DB Connection String: {dbConnectionString}");
logger.LogInformation($"JWT:Secret: {configuration["JWT:Secret"]}");

// Add the database context for Identity
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(dbConnectionString, new MySqlServerVersion(ServerVersion.AutoDetect(dbConnectionString))));

// Add Identity services
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

//source begin: https://www.c-sharpcorner.com/article/jwt-authentication-and-authorization-in-net-6-0-with-identity-framework/
// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
        ValidateIssuerSigningKey = true
    };
});
//source end

builder.Services.AddAuthorization();

builder.Services.AddScoped<AccountService.Library.Services.AccountService>();
var app = builder.Build();

// Automatic Database Migration
using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetRequiredService<UserDbContext>().Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        logger.LogError($"DB Migration Error: {ex}");
    }
}

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<AccountManagementService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

