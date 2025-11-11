using System;

namespace InvisibleRDP.Core.Models
{
    /// <summary>
    /// Represents an audit log entry for connection attempts and sessions.
    /// </summary>
    public class AuditLogEntry
    {
        /// <summary>
        /// Unique identifier for the audit entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Type of event (e.g., ConnectionAttempt, SessionStart, SessionEnd).
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Remote IP address of the connection.
        /// </summary>
        public string RemoteIpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Username attempting or maintaining the connection.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Whether user consent was verified for this connection.
        /// </summary>
        public bool ConsentVerified { get; set; }

        /// <summary>
        /// Result of the connection attempt (Success, Failure, Rejected).
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the event.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Session ID if applicable.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Duration of the session in seconds (for session end events).
        /// </summary>
        public long? SessionDurationSeconds { get; set; }
    }
}
