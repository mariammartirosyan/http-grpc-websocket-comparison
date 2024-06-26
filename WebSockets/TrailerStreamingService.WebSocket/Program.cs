using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using TrailerStreamingService.Library.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
   .AddJsonFile("appsettings.json", true, true)
   .Build();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM")),
        ValidateIssuerSigningKey = true
    };
});
//source end

builder.Services.AddAuthorization();

builder.Services.AddScoped<StreamingService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);


app.Use(async (context, next) =>
{
    if (context.Request.Path == "/fetchTrailer" && context.WebSockets.IsWebSocketRequest)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next();
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            Metrics.CreateHistogram("websocket_response_time_seconds", "WebSocket response time in seconds")
                .Observe(elapsedMilliseconds / 1000);
        }
    }
    else
    {
        await next();
    }
});

app.UseMetricServer("/metrics");

app.MapControllers();
app.Run();

