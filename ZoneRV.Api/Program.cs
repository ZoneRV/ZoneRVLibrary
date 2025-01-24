using System.Reflection;
using System.Text.Json.Serialization;
using MartinCostello.OpenApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using ZoneRV.Api;
using ZoneRV.DBContexts;
using ZoneRV.Services.Production;
using Scalar.AspNetCore;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Configuration.AddJsonFile("appsettings.json");

    Log.Logger =
        new LoggerConfiguration()
           .ReadFrom.Configuration(builder.Configuration)
           .CreateLogger();

    builder.Services.AddSerilog();
    Log.Logger.Information("Starting Api", Environment.UserName);

    builder.Services.AddControllers()
           .AddNewtonsoftJson((options) =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
    
    builder.Services.AddOpenApi();

    builder.Services.AddOpenApiExtensions((options) =>
    {
        options.AddXmlComments<Program>();
    });

    builder.Services.AddDbContext<ProductionContext>
    ((_, options ) =>
        options
           .UseSqlServer(builder.Configuration.GetConnectionString("MySqlConnectionsString"))
           .LogTo(Log.Logger.Debug, LogLevel.Information));

    builder.Services.AddZoneService(builder.Configuration);

    var app = builder.Build();
    
    IProductionService? productionService = app.Services.GetService<IProductionService>();
            
    ArgumentNullException.ThrowIfNull(productionService);
    //productionService.InitialiseService();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(
            options =>
            {
                options.Title = "Zone RV API";
            });
    }

    app.MapControllers();
    app.UseHttpsRedirection();

    app.Run();
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "Fatal exception during startup.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


