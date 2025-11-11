using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Interfaces
{
    /// <summary>
    /// Interface for managing remote desktop sessions.
    /// </summary>
    public interface ISessionHandler
    {
        /// <summary>
        /// Initiates a new remote desktop session.
        /// </summary>
        /// <param name="username">Username for the session.</param>
        /// <param name="remoteIpAddress">Remote IP address.</param>
        /// <param name="consentRecordId">Associated consent record ID.</param>
        /// <returns>The created session information.</returns>
        Task<SessionInfo> InitiateSessionAsync(string username, string remoteIpAddress, Guid consentRecordId);

        /// <summary>
        /// Terminates an active session.
        /// </summary>
        /// <param name="sessionId">Session ID to terminate.</param>
        /// <returns>True if session was successfully terminated.</returns>
        Task<bool> TerminateSessionAsync(string sessionId);

        /// <summary>
        /// Gets information about an active session.
        /// </summary>
        /// <param name="sessionId">Session ID to query.</param>
        /// <returns>Session information, or null if not found.</returns>
        Task<SessionInfo?> GetSessionInfoAsync(string sessionId);

        /// <summary>
        /// Lists all active sessions.
        /// </summary>
        /// <returns>List of active sessions.</returns>
        Task<List<SessionInfo>> GetActiveSessionsAsync();

        /// <summary>
        /// Validates session credentials and encryption.
        /// </summary>
        /// <param name="sessionId">Session ID to validate.</param>
        /// <returns>True if session is valid and secure.</returns>
        Task<bool> ValidateSessionSecurityAsync(string sessionId);

        /// <summary>
        /// Updates the last activity timestamp for a session.
        /// </summary>
        /// <param name="sessionId">Session ID to update.</param>
        /// <returns>Task representing the async operation.</returns>
        Task UpdateSessionActivityAsync(string sessionId);
    }
}
