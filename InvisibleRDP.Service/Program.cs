using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Services;
using InvisibleRDP.Service;

var builder = Host.CreateApplicationBuilder(args);

// Configure Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SystemHostSvc";
});

// Register core services
builder.Services.AddSingleton<IConsentService, ConsentService>();
builder.Services.AddSingleton<IAuditLogger, AuditLogger>();
builder.Services.AddSingleton<IRegistryService, RegistryService>();
builder.Services.AddSingleton<ISessionHandler>(provider =>
{
    var auditLogger = provider.GetRequiredService<IAuditLogger>();
    return new SessionHandler(auditLogger);
});

// Register the background service
builder.Services.AddHostedService<SystemHostSvcWorker>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "SystemHostSvc";
});

var host = builder.Build();
host.Run();
