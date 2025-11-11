using System.Threading.Tasks;

namespace InvisibleRDP.Core.Interfaces
{
    /// <summary>
    /// Interface for Windows Registry operations.
    /// </summary>
    public interface IRegistryService
    {
        /// <summary>
        /// Reads a string value from the registry.
        /// </summary>
        /// <param name="keyPath">Registry key path.</param>
        /// <param name="valueName">Name of the value to read.</param>
        /// <returns>The string value, or null if not found.</returns>
        Task<string?> ReadStringAsync(string keyPath, string valueName);

        /// <summary>
        /// Writes a string value to the registry.
        /// </summary>
        /// <param name="keyPath">Registry key path.</param>
        /// <param name="valueName">Name of the value to write.</param>
        /// <param name="value">Value to write.</param>
        /// <returns>True if successful.</returns>
        Task<bool> WriteStringAsync(string keyPath, string valueName, string value);

        /// <summary>
        /// Reads an integer value from the registry.
        /// </summary>
        /// <param name="keyPath">Registry key path.</param>
        /// <param name="valueName">Name of the value to read.</param>
        /// <returns>The integer value, or null if not found.</returns>
        Task<int?> ReadIntAsync(string keyPath, string valueName);

        /// <summary>
        /// Writes an integer value to the registry.
        /// </summary>
        /// <param name="keyPath">Registry key path.</param>
        /// <param name="valueName">Name of the value to write.</param>
        /// <param name="value">Value to write.</param>
        /// <returns>True if successful.</returns>
        Task<bool> WriteIntAsync(string keyPath, string valueName, int value);

        /// <summary>
        /// Deletes a registry value.
        /// </summary>
        /// <param name="keyPath">Registry key path.</param>
        /// <param name="valueName">Name of the value to delete.</param>
        /// <returns>True if successful.</returns>
        Task<bool> DeleteValueAsync(string keyPath, string valueName);

        /// <summary>
        /// Deletes a registry key and all its subkeys.
        /// </summary>
        /// <param name="keyPath">Registry key path to delete.</param>
        /// <returns>True if successful.</returns>
        Task<bool> DeleteKeyAsync(string keyPath);

        /// <summary>
        /// Checks if a registry key exists.
        /// </summary>
        /// <param name="keyPath">Registry key path to check.</param>
        /// <returns>True if the key exists.</returns>
        Task<bool> KeyExistsAsync(string keyPath);

        /// <summary>
        /// Configures the service to start automatically with Windows.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>True if successful.</returns>
        Task<bool> ConfigureAutoStartAsync(string serviceName);

        /// <summary>
        /// Removes auto-start configuration for the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>True if successful.</returns>
        Task<bool> RemoveAutoStartAsync(string serviceName);
    }
}
