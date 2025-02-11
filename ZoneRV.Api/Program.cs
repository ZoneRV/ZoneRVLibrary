using Figgle;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ZoneRV.Api;
using ZoneRV.DBContexts;
using ZoneRV.Services.Production;
using Scalar.AspNetCore;
using ZoneRV.Api.SignalR;
using ZoneRV.Serialization;
using ZoneRV.Services.Test;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Configuration.AddJsonFile("appsettings.json");
    
    var startupText = FiggleFonts.Standard.Render("Zone RV API");
    var startupSplits = startupText.Split("\r\n");
    
    string[] camp =
    [
        @"       ______                    ",
        @"      /     /\",
        @"     /     /  \",
        @"    /_____/----\_",
        @"",
        @""
    ];

    string[] fire =
    [
                                          "",
                       "             (     ",
                        "            ).    ",
                          "       o (:') o ",
        @"                       o ~/~~\~ o",
         "                        o  o  o  "
    ];

    if (Debugger.IsAttached)
    {
        fire[4] = @"    Debugger Attached  o ~/~~\~ o";
    }
    
    if (bool.TryParse(builder.Configuration["enableWebhooks"], out var result) && !result)
    {
        fire[5] =  "    Webhooks Disabled   o  o  o  ";
    }
    
    var startupTextBorder = new string(Enumerable.Repeat('#', startupSplits[0].Length + 4 + camp[0].Length).ToArray());
  
    Console.WriteLine(startupTextBorder);
    for (var i = 0; i < startupSplits.Length - 1; i++)
    {
        Console.Write("| ");
        Console.Write(startupSplits[i]);
        Console.Write(camp[i]);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(fire[i]);
        Console.ResetColor();
        Console.WriteLine(" |");
    }
    Console.WriteLine(startupTextBorder);
    Console.WriteLine();

    Log.Logger =
        new LoggerConfiguration()
           .ReadFrom.Configuration(builder.Configuration)
           .CreateLogger();

    builder.Services.AddSerilog();
    Log.Logger.Information("Starting Api on {machine}", Environment.MachineName);

    builder.Services.AddControllers()
           .AddNewtonsoftJson((options) =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                options.SerializerSettings.Converters.Add(new LocationInfoJsonConverter());
            });
    
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = Debugger.IsAttached;
    });

    builder.Services.AddResponseCompression(options =>
    {
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    });
    
    builder.Services.AddOpenApi();

    builder.Services.AddOpenApiExtensions((options) =>
    {
        //options.AddXmlComments<Program>();
    });
    
    if(!bool.TryParse(builder.Configuration["useTestProductionService"], out var useTest) || !useTest)
    {
        builder.Services.AddZoneService(builder.Configuration);
    }

    else
    {
        var seedString = builder.Configuration["testProductionServiceSeed"];
        
        builder.Services.AddTestProductionService(false, string.IsNullOrEmpty(seedString) ? null : int.Parse(seedString));
    }

    var app = builder.Build();

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
    app.MapHub<ProductionServiceHub>("/production-hub");
    app.UseHttpsRedirection();
    
    // Continue without waiting
#pragma warning disable CS4014
    app.RunAsync();
#pragma warning restore CS4014

    IProductionService productionService = app.Services.GetRequiredService<IProductionService>();
    
    await productionService.InitialiseService();
    await productionService.LoadRequiredSalesOrdersAsync();
    
    await app.WaitForShutdownAsync();
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "Exception during startup.");
    throw;
}
finally
{
    Log.CloseAndFlush();
    Console.WriteLine("Api has shut down.");
}


