using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Interfaces
{
    /// <summary>
    /// Interface for audit logging functionality.
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Logs an audit event.
        /// </summary>
        /// <param name="entry">The audit log entry to record.</param>
        /// <returns>Task representing the async operation.</returns>
        Task LogEventAsync(AuditLogEntry entry);

        /// <summary>
        /// Logs a connection attempt.
        /// </summary>
        /// <param name="ipAddress">Remote IP address.</param>
        /// <param name="username">Username attempting connection.</param>
        /// <param name="consentVerified">Whether consent was verified.</param>
        /// <param name="result">Result of the attempt.</param>
        /// <returns>Task representing the async operation.</returns>
        Task LogConnectionAttemptAsync(string ipAddress, string username, bool consentVerified, string result);

        /// <summary>
        /// Logs a session start event.
        /// </summary>
        /// <param name="sessionId">Unique session identifier.</param>
        /// <param name="ipAddress">Remote IP address.</param>
        /// <param name="username">Username for the session.</param>
        /// <returns>Task representing the async operation.</returns>
        Task LogSessionStartAsync(string sessionId, string ipAddress, string username);

        /// <summary>
        /// Logs a session end event.
        /// </summary>
        /// <param name="sessionId">Unique session identifier.</param>
        /// <param name="durationSeconds">Session duration in seconds.</param>
        /// <returns>Task representing the async operation.</returns>
        Task LogSessionEndAsync(string sessionId, long durationSeconds);

        /// <summary>
        /// Retrieves audit log entries within a date range.
        /// </summary>
        /// <param name="startDate">Start date for the query.</param>
        /// <param name="endDate">End date for the query.</param>
        /// <returns>List of audit log entries.</returns>
        Task<List<AuditLogEntry>> GetLogsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Performs log rotation to prevent excessive file growth.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        Task RotateLogsAsync();
    }
}
