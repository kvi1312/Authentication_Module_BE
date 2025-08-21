using Authentication.API;
using Authentication.API.Extensions;
using Authentication.Infrastructure;
using Carter;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
Log.Information("Starting Authentication API up");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Host.UseSerilog(SeriLogger.Configure);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.AddAppConfigurations();
builder.Services.AddCarter();
builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
var app = builder.Build();
try
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapCarter();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }


    Console.WriteLine("EXCEPTION: " + ex.Message);
    if (ex.InnerException != null)
    {
        Console.WriteLine("INNER: " + ex.InnerException.Message);
    }

    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    Log.Information("Shut down Authentication API complete");
    Log.CloseAndFlush();
}

