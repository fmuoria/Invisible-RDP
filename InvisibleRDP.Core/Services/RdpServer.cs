using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Services
{
    /// <summary>
    /// RDP server that listens for incoming connections and handles screen sharing.
    /// </summary>
    public class RdpServer
    {
        private readonly IConsentService _consentService;
        private readonly IAuditLogger _auditLogger;
        private readonly ISessionHandler _sessionHandler;
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly int _port;
        private readonly string _password;

        public RdpServer(
            IConsentService consentService,
            IAuditLogger auditLogger,
            ISessionHandler sessionHandler,
            int port = 9876,
            string password = "default")
        {
            _consentService = consentService;
            _auditLogger = auditLogger;
            _sessionHandler = sessionHandler;
            _port = port;
            _password = password;
        }

        public async Task StartAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            await _auditLogger.LogEventAsync(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = "ServerStart",
                Result = "Success",
                Details = $"RDP Server started on port {_port}"
            });

            _ = Task.Run(() => AcceptClientsAsync(_cancellationTokenSource.Token));
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();

            await _auditLogger.LogEventAsync(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = "ServerStop",
                Result = "Success",
                Details = "RDP Server stopped"
            });
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await _auditLogger.LogEventAsync(new AuditLogEntry
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        EventType = "ConnectionError",
                        Result = "Failure",
                        Details = $"Error accepting client: {ex.Message}"
                    });
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            var remoteIp = remoteEndPoint?.Address.ToString() ?? "unknown";

            try
            {
                await _auditLogger.LogConnectionAttemptAsync(remoteIp, "unknown", false, "Pending");

                using var stream = client.GetStream();
                
                // Simple authentication protocol
                var buffer = new byte[4096];
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                var authMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                var authData = JsonSerializer.Deserialize<AuthenticationRequest>(authMessage);
                
                if (authData == null || authData.Password != _password)
                {
                    await SendMessageAsync(stream, new { success = false, error = "Invalid password" });
                    await _auditLogger.LogEventAsync(new AuditLogEntry
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        EventType = "ConnectionAttempt",
                        RemoteIpAddress = remoteIp,
                        Result = "Rejected",
                        Details = "Invalid password"
                    });
                    return;
                }

                // Check consent
                var username = Environment.UserName;
                var hasConsent = await _consentService.HasValidConsentAsync(username);
                
                if (!hasConsent)
                {
                    await SendMessageAsync(stream, new { success = false, error = "No consent" });
                    await _auditLogger.LogEventAsync(new AuditLogEntry
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        EventType = "ConnectionAttempt",
                        RemoteIpAddress = remoteIp,
                        Result = "Rejected",
                        Details = "User consent not granted"
                    });
                    return;
                }

                // Get consent record ID
                var consent = await _consentService.GetActiveConsentAsync(username);
                if (consent == null)
                {
                    await SendMessageAsync(stream, new { success = false, error = "Consent record not found" });
                    return;
                }

                // Initiate session
                var session = await _sessionHandler.InitiateSessionAsync(username, remoteIp, consent.Id);
                
                await SendMessageAsync(stream, new 
                { 
                    success = true, 
                    sessionId = session.SessionId,
                    screenWidth = 1920,
                    screenHeight = 1080
                });

                // Handle session (screen capture and input)
                await HandleSessionAsync(stream, session.SessionId, cancellationToken);
            }
            catch (Exception ex)
            {
                await _auditLogger.LogEventAsync(new AuditLogEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = "SessionError",
                    RemoteIpAddress = remoteIp,
                    Result = "Failure",
                    Details = $"Error handling client: {ex.Message}"
                });
            }
            finally
            {
                client.Close();
            }
        }

        private async Task HandleSessionAsync(NetworkStream stream, string sessionId, CancellationToken cancellationToken)
        {
            // This is a simplified protocol - in production would use proper RDP or VNC protocol
            var buffer = new byte[4096];
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Update activity
                    await _sessionHandler.UpdateSessionActivityAsync(sessionId);

                    // Read commands from client (stub)
                    if (stream.DataAvailable)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                        var command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        
                        if (command == "DISCONNECT")
                        {
                            break;
                        }
                        
                        // Handle other commands (mouse, keyboard, etc.)
                    }

                    // Send screen updates (stub - would capture and send actual screen data)
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception)
                {
                    break;
                }
            }

            await _sessionHandler.TerminateSessionAsync(sessionId);
        }

        private async Task SendMessageAsync(NetworkStream stream, object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytes);
            await stream.FlushAsync();
        }

        private class AuthenticationRequest
        {
            public string? Password { get; set; }
            public string? Username { get; set; }
        }
    }
}
