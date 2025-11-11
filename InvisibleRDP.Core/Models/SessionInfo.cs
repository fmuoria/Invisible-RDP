using System;

namespace InvisibleRDP.Core.Models
{
    /// <summary>
    /// Represents information about an active remote desktop session.
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Username of the remote user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Remote IP address.
        /// </summary>
        public string RemoteIpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Last activity timestamp.
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Encryption protocol used (e.g., TLS 1.2, TLS 1.3).
        /// </summary>
        public string EncryptionProtocol { get; set; } = string.Empty;

        /// <summary>
        /// Authentication method used.
        /// </summary>
        public string AuthenticationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the consent record that authorized this session.
        /// </summary>
        public Guid? ConsentRecordId { get; set; }
    }
}
