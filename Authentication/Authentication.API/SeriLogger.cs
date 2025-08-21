﻿using Serilog;

namespace Authentication.API;

public static class SeriLogger
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
        (context, configuration) =>
        {
            var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
            var envName = context.HostingEnvironment?.EnvironmentName ?? "Development";

            configuration
                .WriteTo.Debug()
                .WriteTo.Console(outputTemplate:
                 "[{Timestamp:HH:mm:ss} {Level}] {SourceContext} {NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", envName)
                .Enrich.WithProperty("Application", applicationName)
                .ReadFrom.Configuration(context.Configuration);
        };
}
