using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Services;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Service
{
    /// <summary>
    /// Background worker service for the SystemHostSvc Windows Service.
    /// This service runs headless and manages remote desktop connections.
    /// </summary>
    public class SystemHostSvcWorker : BackgroundService
    {
        private readonly ILogger<SystemHostSvcWorker> _logger;
        private readonly IConsentService _consentService;
        private readonly IAuditLogger _auditLogger;
        private readonly ISessionHandler _sessionHandler;
        private readonly IRegistryService _registryService;

        public SystemHostSvcWorker(
            ILogger<SystemHostSvcWorker> logger,
            IConsentService consentService,
            IAuditLogger auditLogger,
            ISessionHandler sessionHandler,
            IRegistryService registryService)
        {
            _logger = logger;
            _consentService = consentService;
            _auditLogger = auditLogger;
            _sessionHandler = sessionHandler;
            _registryService = registryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SystemHostSvc starting at: {time}", DateTimeOffset.Now);

            // Check if this is first run - if so, consent must be obtained
            await CheckFirstRunConsentAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Monitor for connection requests
                    await MonitorConnectionRequestsAsync(stoppingToken);

                    // Perform periodic maintenance
                    await PerformMaintenanceAsync();

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SystemHostSvc main loop");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("SystemHostSvc stopping at: {time}", DateTimeOffset.Now);
        }

        private async Task CheckFirstRunConsentAsync()
        {
            var firstRunKey = await _registryService.ReadIntAsync(
                RegistryService.GetAppRegistryPath(), "FirstRunCompleted");

            if (firstRunKey != 1)
            {
                _logger.LogWarning("First run detected - user consent required before accepting connections");
                
                // TODO: In a real implementation, this would trigger the consent UI
                // For now, just log and mark as needing consent
                await _auditLogger.LogEventAsync(new AuditLogEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = "FirstRun",
                    Result = "ConsentRequired",
                    Details = "Service started but user consent has not been obtained"
                });
            }
        }

        private async Task MonitorConnectionRequestsAsync(CancellationToken cancellationToken)
        {
            // TODO: Implement actual connection monitoring
            // This is a stub that would:
            // 1. Listen for incoming RDP connection requests
            // 2. Verify user consent before accepting
            // 3. Establish encrypted session if consent is valid
            // 4. Log all connection attempts

            // For now, just check if any sessions need cleanup
            var activeSessions = await _sessionHandler.GetActiveSessionsAsync();
            
            foreach (var session in activeSessions)
            {
                // Check for inactive sessions
                var inactiveTime = DateTime.UtcNow - session.LastActivity;
                if (inactiveTime > TimeSpan.FromHours(1))
                {
                    _logger.LogInformation("Terminating inactive session: {sessionId}", session.SessionId);
                    await _sessionHandler.TerminateSessionAsync(session.SessionId);
                }
            }
        }

        private async Task PerformMaintenanceAsync()
        {
            // Rotate logs if needed
            try
            {
                await _auditLogger.RotateLogsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rotating audit logs");
            }

            // TODO: Cleanup expired consent records
            // TODO: Cleanup old session data
            // TODO: Perform security checks
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SystemHostSvc is stopping - terminating all active sessions");

            // Terminate all active sessions gracefully
            var activeSessions = await _sessionHandler.GetActiveSessionsAsync();
            foreach (var session in activeSessions)
            {
                await _sessionHandler.TerminateSessionAsync(session.SessionId);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
