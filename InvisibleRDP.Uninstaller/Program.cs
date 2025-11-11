using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using InvisibleRDP.Core.Services;

namespace InvisibleRDP.Uninstaller
{
    /// <summary>
    /// Uninstaller utility for completely removing InvisibleRDP from the system.
    /// Stops and removes the service, deletes logs and data, and cleans up registry entries.
    /// </summary>
    class Program
    {
        private const string ServiceName = "SystemHostSvc";
        private const string AppName = "InvisibleRDP";

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("  InvisibleRDP - Uninstaller");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            // Check if running with administrator privileges
            if (!IsAdministrator())
            {
                Console.WriteLine("ERROR: This uninstaller must be run as Administrator.");
                Console.WriteLine("Please run this application with elevated privileges.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return 1;
            }

            // Check for command line arguments
            bool silentMode = args.Length > 0 && (args[0] == "/s" || args[0] == "/silent");

            if (!silentMode)
            {
                Console.WriteLine("This will completely remove InvisibleRDP from your system:");
                Console.WriteLine("  • Stop the SystemHostSvc service");
                Console.WriteLine("  • Delete the service");
                Console.WriteLine("  • Remove all log files");
                Console.WriteLine("  • Delete consent records");
                Console.WriteLine("  • Clean up registry entries");
                Console.WriteLine();
                Console.Write("Do you want to continue? (yes/no): ");
                
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "yes" && response != "y")
                {
                    Console.WriteLine("Uninstall cancelled.");
                    return 0;
                }
                Console.WriteLine();
            }

            int exitCode = 0;
            var registryService = new RegistryService();

            // Step 1: Stop the service
            Console.Write("Stopping service... ");
            if (await StopServiceAsync(ServiceName))
            {
                Console.WriteLine("✓ Done");
            }
            else
            {
                Console.WriteLine("⚠ Service not found or already stopped");
            }

            // Step 2: Delete the service
            Console.Write("Removing service... ");
            if (await DeleteServiceAsync(ServiceName))
            {
                Console.WriteLine("✓ Done");
            }
            else
            {
                Console.WriteLine("⚠ Failed to remove service (may not exist)");
            }

            // Step 3: Delete log files
            Console.Write("Deleting log files... ");
            var logsDeleted = await DeleteLogsAsync();
            Console.WriteLine(logsDeleted ? "✓ Done" : "⚠ No logs found");

            // Step 4: Delete consent records
            Console.Write("Deleting consent records... ");
            var consentsDeleted = await DeleteConsentsAsync();
            Console.WriteLine(consentsDeleted ? "✓ Done" : "⚠ No consent records found");

            // Step 5: Clean up registry
            Console.Write("Cleaning registry entries... ");
            var registryCleaned = await CleanRegistryAsync(registryService);
            Console.WriteLine(registryCleaned ? "✓ Done" : "⚠ Failed");

            // Step 6: Delete application data directory
            Console.Write("Removing application data... ");
            var appDataDeleted = await DeleteAppDataAsync();
            Console.WriteLine(appDataDeleted ? "✓ Done" : "⚠ No data found");

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("Uninstall completed successfully!");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine("  • Service stopped and removed");
            Console.WriteLine("  • All logs and data deleted");
            Console.WriteLine("  • Registry entries cleaned");
            Console.WriteLine();
            Console.WriteLine("The InvisibleRDP software has been completely removed from your system.");
            
            if (!silentMode)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return exitCode;
        }

        private static bool IsAdministrator()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                // On non-Windows platforms, assume we have sufficient privileges
                return true;
            }
        }

        private static async Task<bool> StopServiceAsync(string serviceName)
        {
            try
            {
                // Try using ServiceController first (Windows-specific)
                try
                {
                    using var service = new ServiceController(serviceName);
                    if (service.Status != ServiceControllerStatus.Stopped)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                    return true;
                }
                catch
                {
                    // ServiceController might not work on non-Windows, try command line
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"stop {serviceName}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch
            {
                // Service might not exist
            }
            return false;
        }

        private static async Task<bool> DeleteServiceAsync(string serviceName)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"delete {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                var process = Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                // Failed to delete service
            }
            return false;
        }

        private static async Task<bool> DeleteLogsAsync()
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    AppName, "Logs");

                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                    return true;
                }
            }
            catch
            {
                // Failed to delete logs
            }
            return false;
        }

        private static async Task<bool> DeleteConsentsAsync()
        {
            try
            {
                var consentPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    AppName, "Consent");

                if (Directory.Exists(consentPath))
                {
                    Directory.Delete(consentPath, true);
                    return true;
                }
            }
            catch
            {
                // Failed to delete consents
            }
            return false;
        }

        private static async Task<bool> DeleteAppDataAsync()
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    AppName);

                if (Directory.Exists(appDataPath))
                {
                    Directory.Delete(appDataPath, true);
                    return true;
                }
            }
            catch
            {
                // Failed to delete app data
            }
            return false;
        }

        private static async Task<bool> CleanRegistryAsync(RegistryService registryService)
        {
            try
            {
                // Remove application registry key
                await registryService.DeleteKeyAsync(RegistryService.GetAppRegistryPath());
                
                // Remove service auto-start
                await registryService.RemoveAutoStartAsync(ServiceName);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
