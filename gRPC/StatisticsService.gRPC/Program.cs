using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using StatisticsService.gRPC.Services;
using StatisticsService.Library;
using StatisticsService.Library.Repositories;
using StatisticsService.Library.Services;

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

logger.LogInformation($"DBConnectionString: {dbConnectionString}");

// Add the database context
builder.Services.AddDbContext<StatisticsDbContext>(options =>
    options.UseMySql(dbConnectionString, new MySqlServerVersion(ServerVersion.AutoDetect(dbConnectionString))));

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
builder.Services.AddAuthorization();
//source end

builder.Services.AddScoped<StatisticsRepository>();
builder.Services.AddScoped<StatisticsService.Library.Services.StatisticsService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<StatisticsManagementService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

