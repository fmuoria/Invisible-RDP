using System;
using System.Threading.Tasks;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Interfaces
{
    /// <summary>
    /// Interface for managing user consent for remote desktop access.
    /// </summary>
    public interface IConsentService
    {
        /// <summary>
        /// Records a new consent from the user.
        /// </summary>
        /// <param name="record">The consent record to save.</param>
        /// <returns>True if consent was successfully recorded.</returns>
        Task<bool> RecordConsentAsync(ConsentRecord record);

        /// <summary>
        /// Checks if valid consent exists for the current user.
        /// </summary>
        /// <param name="username">Username to check consent for.</param>
        /// <returns>True if valid consent exists.</returns>
        Task<bool> HasValidConsentAsync(string username);

        /// <summary>
        /// Retrieves the active consent record for a user.
        /// </summary>
        /// <param name="username">Username to retrieve consent for.</param>
        /// <returns>The active consent record, or null if none exists.</returns>
        Task<ConsentRecord?> GetActiveConsentAsync(string username);

        /// <summary>
        /// Revokes consent for a user.
        /// </summary>
        /// <param name="username">Username to revoke consent for.</param>
        /// <returns>True if consent was successfully revoked.</returns>
        Task<bool> RevokeConsentAsync(string username);

        /// <summary>
        /// Validates the integrity of a consent signature.
        /// </summary>
        /// <param name="record">The consent record to validate.</param>
        /// <returns>True if the signature is valid.</returns>
        bool ValidateConsentSignature(ConsentRecord record);
    }
}
