using System;

namespace InvisibleRDP.Core.Models
{
    /// <summary>
    /// Represents a user consent record for remote desktop access.
    /// </summary>
    public class ConsentRecord
    {
        /// <summary>
        /// Unique identifier for the consent record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username of the user who provided consent.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when consent was granted.
        /// </summary>
        public DateTime ConsentTimestamp { get; set; }

        /// <summary>
        /// Digital signature or hash of the consent.
        /// </summary>
        public string ConsentSignature { get; set; } = string.Empty;

        /// <summary>
        /// IP address from which consent was provided.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Machine name where consent was granted.
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether consent is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional expiration date for the consent.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Full consent text that was shown to the user.
        /// </summary>
        public string ConsentText { get; set; } = string.Empty;
    }
}
