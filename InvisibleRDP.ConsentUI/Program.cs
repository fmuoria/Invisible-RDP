using System;
using System.Threading.Tasks;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Services;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.ConsentUI
{
    /// <summary>
    /// Consent UI application for obtaining user consent.
    /// This application presents a GUI to the user on first run to obtain consent.
    /// Note: This is a console-based stub. In production, this would be WPF or WinForms.
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  InvisibleRDP - User Consent Required");
            Console.WriteLine("========================================");
            Console.WriteLine();

            try
            {
                var consentService = new ConsentService();
                var registryService = new RegistryService();

                // Check if consent already exists
                var username = Environment.UserName;
                var hasConsent = await consentService.HasValidConsentAsync(username);

                if (hasConsent)
                {
                    Console.WriteLine($"Valid consent already exists for user: {username}");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return 0;
                }

                // Display consent text
                DisplayConsentText();

                Console.WriteLine();
                Console.Write("Do you consent to remote desktop access? (yes/no): ");
                var response = Console.ReadLine()?.Trim().ToLower();

                if (response != "yes" && response != "y")
                {
                    Console.WriteLine();
                    Console.WriteLine("Consent denied. The service will not accept remote connections.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return 1;
                }

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

                var success = await consentService.RecordConsentAsync(consentRecord);

                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("✓ Consent has been recorded successfully.");
                    Console.WriteLine($"Consent ID: {consentRecord.Id}");
                    Console.WriteLine($"Timestamp: {consentRecord.ConsentTimestamp:yyyy-MM-dd HH:mm:ss UTC}");
                    
                    // Mark first run as completed
                    await registryService.WriteIntAsync(
                        RegistryService.GetAppRegistryPath(), "FirstRunCompleted", 1);
                    
                    Console.WriteLine();
                    Console.WriteLine("The SystemHostSvc service can now accept remote connections.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("✗ Failed to record consent. Please try again or contact support.");
                    return 1;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return 1;
            }
        }

        private static void DisplayConsentText()
        {
            Console.WriteLine(GetConsentText());
        }

        private static string GetConsentText()
        {
            return @"
CONSENT FOR REMOTE DESKTOP ACCESS
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
and the service will not accept any connections.
";
        }

        private static string GetLocalIpAddress()
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
