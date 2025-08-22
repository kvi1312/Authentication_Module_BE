using Authentication.API;
using Authentication.API.Extensions;
using Authentication.API.Middleware;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Persistence;
using Carter;
using MediatR;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();

    });
});

builder.Host.AddAppConfigurations();
builder.Services.AddCarter();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Authentication.Application.Handlers.LoginCommandHandler).Assembly));

builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHostedService<DbSeeder>();
builder.Services.ConfigAuthentication(builder.Configuration);

var app = builder.Build();

app.MigrateDataBase<AppDbContext>();

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

    // Add CORS middleware - Allow all origins
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseMiddleware<JwtBlacklistMiddleware>();
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

