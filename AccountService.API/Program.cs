using System;
using System.Text;
using AccountService.Library;
using AccountService.Library.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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

builder.Services.AddScoped<AccountService.Library.Services.AccountService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

