using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InvisibleRDP.Core.Interfaces;
using InvisibleRDP.Core.Models;

namespace InvisibleRDP.Core.Services
{
    /// <summary>
    /// Service for audit logging with rotation support.
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private readonly string _logDirectory;
        private readonly string _currentLogFile;
        private readonly long _maxLogSizeBytes;
        private readonly int _maxLogFiles;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public AuditLogger(string? logDirectory = null, long maxLogSizeMB = 50, int maxLogFiles = 10)
        {
            _logDirectory = logDirectory ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                    "InvisibleRDP", "Logs");
            
            _currentLogFile = Path.Combine(_logDirectory, "audit.log");
            _maxLogSizeBytes = maxLogSizeMB * 1024 * 1024;
            _maxLogFiles = maxLogFiles;

            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task LogEventAsync(AuditLogEntry entry)
        {
            await _writeLock.WaitAsync();
            try
            {
                // Check if rotation is needed
                if (File.Exists(_currentLogFile))
                {
                    var fileInfo = new FileInfo(_currentLogFile);
                    if (fileInfo.Length >= _maxLogSizeBytes)
                    {
                        await RotateLogsAsync();
                    }
                }

                var logLine = FormatLogEntry(entry);
                await File.AppendAllTextAsync(_currentLogFile, logLine + Environment.NewLine);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task LogConnectionAttemptAsync(string ipAddress, string username, bool consentVerified, string result)
        {
            var entry = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = "ConnectionAttempt",
                RemoteIpAddress = ipAddress,
                Username = username,
                ConsentVerified = consentVerified,
                Result = result,
                Details = $"Connection attempt from {ipAddress} by {username}. Consent: {consentVerified}"
            };

            await LogEventAsync(entry);
        }

        public async Task LogSessionStartAsync(string sessionId, string ipAddress, string username)
        {
            var entry = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = "SessionStart",
                RemoteIpAddress = ipAddress,
                Username = username,
                SessionId = sessionId,
                ConsentVerified = true,
                Result = "Success",
                Details = $"Session {sessionId} started for user {username} from {ipAddress}"
            };

            await LogEventAsync(entry);
        }

        public async Task LogSessionEndAsync(string sessionId, long durationSeconds)
        {
            var entry = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = "SessionEnd",
                SessionId = sessionId,
                SessionDurationSeconds = durationSeconds,
                ConsentVerified = true,
                Result = "Success",
                Details = $"Session {sessionId} ended. Duration: {durationSeconds}s"
            };

            await LogEventAsync(entry);
        }

        public async Task<List<AuditLogEntry>> GetLogsAsync(DateTime startDate, DateTime endDate)
        {
            var entries = new List<AuditLogEntry>();

            await _writeLock.WaitAsync();
            try
            {
                if (!File.Exists(_currentLogFile))
                    return entries;

                var lines = await File.ReadAllLinesAsync(_currentLogFile);
                
                foreach (var line in lines)
                {
                    try
                    {
                        var entry = ParseLogEntry(line);
                        if (entry != null && entry.Timestamp >= startDate && entry.Timestamp <= endDate)
                        {
                            entries.Add(entry);
                        }
                    }
                    catch
                    {
                        // Skip malformed entries
                    }
                }
            }
            finally
            {
                _writeLock.Release();
            }

            return entries;
        }

        public async Task RotateLogsAsync()
        {
            await _writeLock.WaitAsync();
            try
            {
                if (!File.Exists(_currentLogFile))
                    return;

                // Rotate existing log files
                for (int i = _maxLogFiles - 1; i >= 1; i--)
                {
                    var oldFile = Path.Combine(_logDirectory, $"audit.{i}.log");
                    var newFile = Path.Combine(_logDirectory, $"audit.{i + 1}.log");
                    
                    if (File.Exists(oldFile))
                    {
                        if (i + 1 > _maxLogFiles)
                        {
                            File.Delete(oldFile);
                        }
                        else
                        {
                            File.Move(oldFile, newFile, true);
                        }
                    }
                }

                // Move current log to .1
                var rotatedFile = Path.Combine(_logDirectory, "audit.1.log");
                File.Move(_currentLogFile, rotatedFile, true);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private string FormatLogEntry(AuditLogEntry entry)
        {
            return JsonSerializer.Serialize(entry);
        }

        private AuditLogEntry? ParseLogEntry(string line)
        {
            try
            {
                return JsonSerializer.Deserialize<AuditLogEntry>(line);
            }
            catch
            {
                return null;
            }
        }
    }
}
