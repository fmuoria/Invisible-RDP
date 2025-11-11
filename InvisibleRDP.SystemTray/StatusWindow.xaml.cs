using System;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Services;

namespace InvisibleRDP.SystemTray
{
    public partial class StatusWindow : Window
    {
        private readonly IConsentService _consentService;
        private readonly ISessionHandler _sessionHandler;
        private readonly IAuditLogger _auditLogger;

        public StatusWindow()
        {
            InitializeComponent();
            _consentService = new ConsentService();
            _auditLogger = new AuditLogger();
            _sessionHandler = new SessionHandler(_auditLogger);
            
            LoadStatus();
        }

        private async void LoadStatus()
        {
            try
            {
                // Check service status
                try
                {
                    using var service = new ServiceController("SystemHostSvc");
                    ServiceStatusText.Text = $"Service Status: {service.Status}";
                }
                catch
                {
                    ServiceStatusText.Text = "Service Status: Not Installed";
                }

                // Check consent status
                var username = Environment.UserName;
                var hasConsent = await _consentService.HasValidConsentAsync(username);
                ConsentStatusText.Text = $"Consent Status: {(hasConsent ? "Granted" : "Not Granted")}";

                // Get IP address
                IpAddressText.Text = $"IP Address: {GetLocalIpAddress()}";

                // Get machine name
                MachineNameText.Text = $"Machine Name: {Environment.MachineName}";

                // Get active sessions
                var sessions = await _sessionHandler.GetActiveSessionsAsync();
                SessionsListBox.Items.Clear();
                
                if (sessions.Any())
                {
                    foreach (var session in sessions)
                    {
                        SessionsListBox.Items.Add(
                            $"Session: {session.SessionId}\n" +
                            $"  User: {session.Username}\n" +
                            $"  Remote IP: {session.RemoteIpAddress}\n" +
                            $"  Started: {session.StartTime:yyyy-MM-dd HH:mm:ss}\n" +
                            $"  Duration: {(DateTime.UtcNow - session.StartTime).TotalMinutes:F1} minutes\n");
                    }
                }
                else
                {
                    SessionsListBox.Items.Add("No active sessions");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading status: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var addresses = System.Net.Dns.GetHostAddresses(hostName);
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStatus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
