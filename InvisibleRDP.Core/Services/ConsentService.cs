using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Services
{
    /// <summary>
    /// Service for managing user consent records.
    /// </summary>
    public class ConsentService : IConsentService
    {
        private readonly string _consentStorePath;
        private readonly object _fileLock = new object();

        public ConsentService(string? consentStorePath = null)
        {
            _consentStorePath = consentStorePath ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                    "InvisibleRDP", "Consent", "consent.json");

            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_consentStorePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<bool> RecordConsentAsync(ConsentRecord record)
        {
            try
            {
                // Generate consent signature
                record.ConsentSignature = GenerateConsentSignature(record);
                
                var records = await LoadConsentsAsync();
                
                // Deactivate any existing consent for this user
                foreach (var existing in records.Where(r => r.Username == record.Username))
                {
                    existing.IsActive = false;
                }
                
                records.Add(record);
                
                await SaveConsentsAsync(records);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasValidConsentAsync(string username)
        {
            var consent = await GetActiveConsentAsync(username);
            
            if (consent == null || !consent.IsActive)
                return false;
            
            // Check if consent has expired
            if (consent.ExpirationDate.HasValue && consent.ExpirationDate.Value < DateTime.UtcNow)
                return false;
            
            return true;
        }

        public async Task<ConsentRecord?> GetActiveConsentAsync(string username)
        {
            var records = await LoadConsentsAsync();
            return records
                .Where(r => r.Username == username && r.IsActive)
                .OrderByDescending(r => r.ConsentTimestamp)
                .FirstOrDefault();
        }

        public async Task<bool> RevokeConsentAsync(string username)
        {
            try
            {
                var records = await LoadConsentsAsync();
                var modified = false;
                
                foreach (var record in records.Where(r => r.Username == username && r.IsActive))
                {
                    record.IsActive = false;
                    modified = true;
                }
                
                if (modified)
                {
                    await SaveConsentsAsync(records);
                }
                
                return modified;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateConsentSignature(ConsentRecord record)
        {
            var expectedSignature = GenerateConsentSignature(record);
            return record.ConsentSignature == expectedSignature;
        }

        private string GenerateConsentSignature(ConsentRecord record)
        {
            var dataToSign = $"{record.Username}|{record.ConsentTimestamp:O}|{record.MachineName}|{record.ConsentText}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            return Convert.ToBase64String(hashBytes);
        }

        private Task<List<ConsentRecord>> LoadConsentsAsync()
        {
            if (!File.Exists(_consentStorePath))
                return Task.FromResult(new List<ConsentRecord>());

            try
            {
                string json;
                lock (_fileLock)
                {
                    json = File.ReadAllText(_consentStorePath);
                }
                
                var records = JsonSerializer.Deserialize<List<ConsentRecord>>(json);
                return Task.FromResult(records ?? new List<ConsentRecord>());
            }
            catch
            {
                return Task.FromResult(new List<ConsentRecord>());
            }
        }

        private Task SaveConsentsAsync(List<ConsentRecord> records)
        {
            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            lock (_fileLock)
            {
                File.WriteAllText(_consentStorePath, json);
            }
            
            return Task.CompletedTask;
        }
    }
}
