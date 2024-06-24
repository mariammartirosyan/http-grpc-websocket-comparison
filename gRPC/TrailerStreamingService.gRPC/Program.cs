using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using TrailerStreamingService.gRPC;
using TrailerStreamingService.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(8094, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenAnyIP(8084, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
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

builder.Services.AddAuthorization();
//builder.Services.AddGrpc(options =>
//{
//    options.MaxSendMessageSize = int.MaxValue;
//});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<MetricsInterceptor>(); // Add the MetricsInterceptor
    options.MaxSendMessageSize = int.MaxValue;
});

builder.Services.AddScoped<TrailerStreamingService.Library.Services.StreamingService>();

var app = builder.Build();

//app.Use(async (context, next) =>
//{
//    var stopwatch = Stopwatch.StartNew();
//    var originalBodyStream = context.Response.Body;
//    await using var responseBody = new MemoryStream();
//    context.Response.Body = responseBody;

//    try
//    {
//        await next.Invoke();
//    }
//    finally
//    {
//        stopwatch.Stop();
//        var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

//        // Record the response time metric
//        Metrics.CreateHistogram("grpc_response_time_seconds", "Response time in seconds")
//            .Observe(elapsedMilliseconds / 1000);

//        responseBody.Seek(0, SeekOrigin.Begin);
//        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
//        responseBody.Seek(0, SeekOrigin.Begin);
//        await responseBody.CopyToAsync(originalBodyStream);

//        // Record the response size metric
//        var responseSizeBytes = responseBody.Length;
//        Metrics.CreateCounter("grpc_response_size_bytes", "Response size in bytes")
//            .Inc(responseSizeBytes);
//    }
//});

app.UseMetricServer("/metrics");
//app.UseRouting();
app.UseGrpcMetrics();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<StreamingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
