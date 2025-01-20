using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web.Resource;
using Serilog;
using Serilog.Sinks.Seq;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using ZoneRV.Api;
using ZoneRV.Api.Controllers;
using ZoneRV.Services;
using ZoneRV.Services.DB;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Configuration.AddJsonFile("appsettings.json");

    Log.Logger =
        new LoggerConfiguration()
           .ReadFrom.Configuration(builder.Configuration)
           .CreateLogger();

    builder.Services.AddSerilog();
    Log.Logger.Information("test {env}", Environment.UserName);

    // Add services to the container.
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddAuthorization();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddTransient<SqlDataAccess, MsSqlDataAccess>();
    builder.Services.AddTransient<ProductionDataService>();
    builder.Services.AddZoneService();

    var app = builder.Build();
    
    IProductionService? productionService = app.Services.GetService<IProductionService>();
            
    ArgumentNullException.ThrowIfNull(productionService);

    Task.Run(async () => await productionService.InitialiseService());

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "";

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


