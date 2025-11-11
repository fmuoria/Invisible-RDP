using System;
using System.Windows;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Services;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.SystemTray
{
    public partial class ConsentWindow : Window
    {
        private readonly IConsentService _consentService;
        private readonly IRegistryService _registryService;

        public ConsentWindow()
        {
            InitializeComponent();
            _consentService = new ConsentService();
            _registryService = new RegistryService();
            
            LoadConsentStatus();
        }

        private async void LoadConsentStatus()
        {
            ConsentTextBlock.Text = GetConsentText();

            // Check if consent already exists
            var username = Environment.UserName;
            var hasConsent = await _consentService.HasValidConsentAsync(username);

            if (hasConsent)
            {
                var consent = await _consentService.GetActiveConsentAsync(username);
                if (consent != null)
                {
                    AcceptCheckBox.IsChecked = true;
                    AcceptCheckBox.Content = $"Consent granted on {consent.ConsentTimestamp:yyyy-MM-dd HH:mm:ss UTC}";
                    AcceptCheckBox.IsEnabled = false;
                }
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = Environment.UserName;
                
                // Record consent
                var consentRecord = new ConsentRecord
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    ConsentTimestamp = DateTime.UtcNow,
                    IpAddress = GetLocalIpAddress(),
                    MachineName = Environment.MachineName,
                    IsActive = true,
                    ConsentText = GetConsentText()
                };

                var success = await _consentService.RecordConsentAsync(consentRecord);

                if (success)
                {
                    // Mark first run as completed
                    await _registryService.WriteIntAsync(
                        RegistryService.GetAppRegistryPath(), "FirstRunCompleted", 1);

                    MessageBox.Show(
                        "Consent has been recorded successfully.\n" +
                        $"Consent ID: {consentRecord.Id}\n" +
                        $"Timestamp: {consentRecord.ConsentTimestamp:yyyy-MM-dd HH:mm:ss UTC}",
                        "Consent Recorded",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to record consent. Please try again or contact support.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Consent declined. The service will not accept remote connections.",
                "Consent Declined",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Close();
        }

        private string GetConsentText()
        {
            return @"CONSENT FOR REMOTE DESKTOP ACCESS
==================================

By providing your consent, you acknowledge and agree to the following:

1. REMOTE ACCESS: You authorize the installation and operation of remote 
   desktop software (InvisibleRDP) on this computer.

2. DATA COLLECTION: Connection attempts, session information, and access 
   logs will be recorded for security and audit purposes.

3. MONITORING: All remote desktop sessions are logged, including:
   - Connection timestamp and duration
   - Remote IP address
   - Username and authentication details
   - Session activities

4. SECURITY: Connections use encrypted protocols (TLS/SSL) to protect 
   your data during transmission.

5. YOUR RIGHTS: You have the right to:
   - Review access logs at any time
   - Revoke consent at any time
   - Uninstall the software completely
   - Request deletion of recorded data

6. TRANSPARENCY: This software operates with full transparency. You will 
   be notified of remote access attempts and can monitor active sessions.

7. ETHICAL USE: This software is designed for legitimate remote support 
   and administration purposes only. Unauthorized or malicious use is 
   strictly prohibited.

8. LEGAL COMPLIANCE: Use of this software must comply with all applicable 
   laws, regulations, and organizational policies.

IMPORTANT: If you do not consent, remote desktop access will be denied 
and the service will not accept any connections.";
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
    }
}
