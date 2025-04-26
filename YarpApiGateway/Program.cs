using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YARP API Gateway", Version = "v1" });
});

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5261, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("cert.pfx", "password");
        });
    });
});

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "YARP API Gateway v1");
        c.RoutePrefix = string.Empty; // Swagger'ı root URL'de göster
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// API Gateway endpoints
var api = app.MapGroup("/api/gateway");

api.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("GetHealth")
    .WithDescription("Health check endpoint for the API Gateway");

api.MapGet("/routes", () =>
{
    var routes = new[]
    {
        new { Path = "/auth/*", Target = "Auth Service", Description = "Authentication and Authorization" },
        new { Path = "/product/*", Target = "Product Service", Description = "Product Management" },
        new { Path = "/basket/*", Target = "Basket Service", Description = "Shopping Basket" }
    };
    return Results.Ok(routes);
})
.WithName("GetRoutes")
.WithDescription("List all available routes in the API Gateway");

api.MapGet("/status", () =>
{
    var status = new
    {
        Service = "YARP API Gateway",
        Version = "1.0",
        Status = "Running",
        Port = 5261,
        Protocol = "HTTPS",
        Features = new[] { "Reverse Proxy", "Load Balancing", "Path Rewriting" }
    };
    return Results.Ok(status);
})
.WithName("GetStatus")
.WithDescription("Get the current status of the API Gateway");

// Use YARP for reverse proxy
app.MapReverseProxy();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
