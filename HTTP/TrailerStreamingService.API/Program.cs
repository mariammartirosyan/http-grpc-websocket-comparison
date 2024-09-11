using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Diagnostics;
using System.Text;
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

builder.Services.AddControllers();

builder.Services.AddScoped<StreamingService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// measure response time and data transfer
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    var originalBodyStream = context.Response.Body;
    await using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    try
    {
        await next.Invoke();
    }
    finally
    {
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

        Metrics.CreateHistogram("http_response_time_seconds", "Response time in seconds")
            .Observe(elapsedMilliseconds / 1000);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);

        Metrics.CreateCounter("http_response_size_bytes", "Response size in bytes")
            .Inc(responseBody.Length);
    }
});

app.UseMetricServer("/metrics");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

