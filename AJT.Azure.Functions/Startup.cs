using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace AJT.Azure.Functions
{
    public class Startup : IWebJobsStartup
    {
        public Startup()
        {
            var storageAccountName = Environment.GetEnvironmentVariable("CLOUD_STORAGE_ACCOUNT_NAME", EnvironmentVariableTarget.Process);
            var storageAccountKey = Environment.GetEnvironmentVariable("CLOUD_STORAGE_ACCOUNT_KEY", EnvironmentVariableTarget.Process);

            StorageCredentials storageCredentials = new StorageCredentials(storageAccountName, storageAccountKey);
            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("AJT.Azure.Functions", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Events)
                .WriteTo.AzureTableStorage(cloudStorageAccount,storageTableName:"Logs",restrictedToMinimumLevel:LogEventLevel.Information)
                .CreateLogger();
        }

        public void Configure(IWebJobsBuilder builder)
        {
            ConfigureServices(builder.Services).BuildServiceProvider(true);
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            return services;
        }
    }
}
