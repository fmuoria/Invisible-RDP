# InvisibleRDP - Windows Service-Based Remote Desktop Solution

## ⚠️ ETHICAL USE STATEMENT

**This software is designed for legitimate remote support, system administration, and authorized remote access ONLY.**

By using this software, you agree to:
- Obtain **explicit, informed consent** from all users before enabling remote access
- Use the software **only on systems you own or have explicit authorization** to access
- Comply with **all applicable laws, regulations, and organizational policies**
- Maintain **transparency** about the software's presence and operation
- Respect **user privacy** and data protection rights
- Provide users with the ability to **monitor, control, and terminate** access at any time

**Unauthorized access to computer systems is illegal and unethical. Misuse of this software may result in criminal prosecution and civil liability.**

---

## Project Overview

InvisibleRDP is a Windows Service-based remote desktop solution that emphasizes **transparency, user consent, and security**. Unlike traditional remote desktop solutions, InvisibleRDP is built with ethical principles at its core, requiring mandatory user consent before any remote access is permitted.

### Key Features

✅ **Mandatory User Consent**: No remote access is possible without explicit, documented user consent  
✅ **Comprehensive Audit Logging**: Every connection attempt and session is logged with full details  
✅ **Encrypted Communications**: All remote sessions use TLS/SSL encryption (stub implementation)  
✅ **Windows Service Architecture**: Runs as a background service ("SystemHostSvc") with system boot startup  
✅ **Clean Uninstallation**: Complete removal tool that deletes all data, logs, and registry entries  
✅ **Registry-Based Configuration**: Persistent configuration storage in Windows Registry  
✅ **Log Rotation**: Automatic log file rotation to prevent excessive disk usage  

---

## Architecture

The solution consists of four main projects:

### 1. InvisibleRDP.Core
Core library containing all business logic, models, and services.

**Key Components:**
- **ConsentService**: Manages user consent records with cryptographic signatures
- **AuditLogger**: Logs all connection attempts and sessions with automatic rotation
- **SessionHandler**: Manages active remote desktop sessions (stub implementation)
- **RegistryService**: Handles Windows Registry operations for configuration
- **EncryptionHelper**: Provides cryptographic utilities and TLS/SSL stubs
- **ProcessObfuscator**: Stub for process name obfuscation (ethical considerations)

### 2. InvisibleRDP.Service
Windows Service application that runs as "SystemHostSvc".

**Responsibilities:**
- Runs headless as a background Windows Service
- Checks for user consent on first run
- Monitors for incoming connection requests
- Manages active sessions and performs maintenance
- Automatically rotates logs and cleans up expired data

### 3. InvisibleRDP.ConsentUI
Console application for obtaining user consent (GUI stub).

**Features:**
- Presents comprehensive consent text to users
- Records digitally signed consent with timestamp and machine info
- Validates consent before allowing remote access
- Marks service as "first run completed" in registry

### 4. InvisibleRDP.Uninstaller
Utility for complete system cleanup.

**Cleanup Operations:**
- Stops and removes the SystemHostSvc service
- Deletes all log files and consent records
- Removes all registry entries
- Provides clear feedback on uninstallation progress

---

## Installation

### Prerequisites
- Windows 10/11 or Windows Server 2016+
- .NET 8.0 Runtime or SDK
- Administrator privileges for service installation

### Step 1: Build the Solution

```bash
git clone https://github.com/fmuoria/Invisible-RDP.git
cd Invisible-RDP
dotnet build InvisibleRDP.sln --configuration Release
```

### Step 2: Grant User Consent (MANDATORY)

Before the service can accept connections, user consent must be obtained:

```bash
cd InvisibleRDP.ConsentUI\bin\Release\net8.0
InvisibleRDP.ConsentUI.exe
```

Follow the on-screen prompts to read and accept the consent agreement.

### Step 3: Install the Windows Service

```bash
cd InvisibleRDP.Service\bin\Release\net8.0

# Install the service
sc create SystemHostSvc binPath="<full-path>\InvisibleRDP.Service.exe" start=auto

# Start the service
sc start SystemHostSvc

# Verify service status
sc query SystemHostSvc
```

### Step 4: Verify Installation

Check that the service is running:

```powershell
Get-Service SystemHostSvc
```

Verify consent was recorded:
- Location: `%ProgramData%\InvisibleRDP\Consent\consent.json`

Verify audit logs are being created:
- Location: `%ProgramData%\InvisibleRDP\Logs\audit.log`

---

## Configuration

### Registry Settings

Configuration is stored in the Windows Registry at:
```
HKEY_LOCAL_MACHINE\SOFTWARE\InvisibleRDP
```

**Available Settings:**
- `FirstRunCompleted` (DWORD): 0 = consent required, 1 = consent granted
- `StealthMode` (DWORD): 0 = normal, 1 = stealth (stub feature)

**Service Configuration:**
```
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SystemHostSvc
```

### Log Files

Audit logs are stored at:
```
%ProgramData%\InvisibleRDP\Logs\audit.log
```

**Log Rotation:**
- Maximum log size: 50 MB (configurable)
- Maximum log files: 10 (configurable)
- Rotated files: `audit.1.log`, `audit.2.log`, etc.

**Log Entry Format:**
Each log entry is a JSON object containing:
```json
{
  "Id": "guid",
  "Timestamp": "2024-11-11T09:52:13.086Z",
  "EventType": "ConnectionAttempt|SessionStart|SessionEnd",
  "RemoteIpAddress": "192.168.1.100",
  "Username": "user",
  "ConsentVerified": true,
  "Result": "Success|Failure|Rejected",
  "Details": "descriptive message",
  "SessionId": "session-guid",
  "SessionDurationSeconds": 3600
}
```

---

## Usage

### Monitoring Active Sessions

Currently, sessions are managed in-memory. To view active sessions, check the service logs or implement a monitoring interface (future enhancement).

### Revoking Consent

To revoke consent and disable remote access:

1. **Option 1**: Run the ConsentUI again and decline consent when prompted
2. **Option 2**: Manually edit the consent record:
   - Location: `%ProgramData%\InvisibleRDP\Consent\consent.json`
   - Set `IsActive` to `false`
   - Restart the SystemHostSvc service

### Viewing Audit Logs

Audit logs can be viewed using any text editor or JSON parser:

```powershell
Get-Content "C:\ProgramData\InvisibleRDP\Logs\audit.log" | Select-Object -Last 20
```

Or use a JSON viewer for better formatting.

---

## Uninstallation

### Complete Removal

Run the uninstaller utility with administrator privileges:

```bash
cd InvisibleRDP.Uninstaller\bin\Release\net8.0
InvisibleRDP.Uninstaller.exe
```

**What Gets Removed:**
- SystemHostSvc Windows Service
- All log files
- All consent records
- All registry entries
- Application data directory

### Silent Uninstallation

For automated/scripted uninstallation:

```bash
InvisibleRDP.Uninstaller.exe /silent
```

### Manual Uninstallation

If the uninstaller fails, manually remove:

1. **Stop and delete the service:**
   ```bash
   sc stop SystemHostSvc
   sc delete SystemHostSvc
   ```

2. **Delete application data:**
   ```bash
   rmdir /s /q "%ProgramData%\InvisibleRDP"
   ```

3. **Remove registry entries:**
   ```bash
   reg delete "HKLM\SOFTWARE\InvisibleRDP" /f
   ```

---

## Testing

### Build and Test

```bash
# Build solution
dotnet build InvisibleRDP.sln

# Run tests (when implemented)
dotnet test InvisibleRDP.sln
```

### Manual Testing Steps

1. **Consent Flow:**
   - Run ConsentUI without prior consent → Should require consent
   - Accept consent → Should record to JSON file
   - Run ConsentUI again → Should show existing consent

2. **Service Operation:**
   - Install and start service → Should start successfully
   - Check Windows Event Log → Should see startup messages
   - Check audit logs → Should see initialization events

3. **Uninstallation:**
   - Run uninstaller → Should remove all components
   - Verify files deleted → Application data folder should be gone
   - Verify service removed → `sc query SystemHostSvc` should fail

---

## Security Considerations

### Implemented Security Features

✅ **Consent Signatures**: SHA256-based signatures on consent records  
✅ **Audit Trail**: Comprehensive logging of all access attempts  
✅ **Encryption Stubs**: TLS/SSL placeholders for secure communications  
✅ **File Locking**: Thread-safe file operations for consent and logs  
✅ **Registry Protection**: Administrative privileges required for modifications  

### Stub Features (Not Yet Implemented)

⚠️ **TLS/SSL Encryption**: Currently stubbed - requires actual implementation  
⚠️ **Certificate Validation**: Placeholder only - needs real certificate chain validation  
⚠️ **Process Obfuscation**: Ethical concerns - stub only, not implemented  
⚠️ **Video Capture**: Driver-level capture not implemented  
⚠️ **Remote Input Processing**: Not implemented  

### Security Best Practices

1. **Always obtain explicit consent** before enabling remote access
2. **Review audit logs regularly** for suspicious activity
3. **Keep consent records secure** - protect against tampering
4. **Use strong authentication** (implement in production)
5. **Enable full TLS/SSL encryption** before production use
6. **Regular security audits** of the codebase and deployment
7. **Principle of least privilege** - run service with minimum required permissions

---

## Compliance and Legal

### Data Protection

- **GDPR Compliance**: Users have the right to access, modify, and delete their data
- **Consent Management**: Explicit, informed consent is required and recorded
- **Data Retention**: Logs are rotated and old data is automatically deleted
- **User Rights**: Users can revoke consent and uninstall at any time

### Transparency Requirements

This software is designed to be **fully transparent**:
- Service name ("SystemHostSvc") is visible in Task Manager
- Consent text clearly explains what is being monitored
- Audit logs provide complete visibility into all activities
- Uninstallation is straightforward and removes all traces

### Ethical Guidelines

**DO:**
✅ Use for authorized remote support and system administration  
✅ Obtain explicit, informed consent from all users  
✅ Maintain transparent communication about the software  
✅ Provide users with access to logs and consent records  
✅ Comply with all applicable laws and regulations  

**DO NOT:**
❌ Install without user knowledge or consent  
❌ Use for surveillance, monitoring, or unauthorized access  
❌ Hide the software's presence from users  
❌ Bypass or disable consent requirements  
❌ Use in violation of laws or organizational policies  

---

## Future Enhancements

### Planned Features

- [ ] Full TLS/SSL encryption implementation
- [ ] WPF-based consent GUI with visual signing
- [ ] Web-based monitoring dashboard for users
- [ ] Real-time session monitoring and control
- [ ] Multi-user consent management
- [ ] Certificate-based authentication
- [ ] Hardware-accelerated video capture (DXGI)
- [ ] Configurable consent expiration
- [ ] Email notifications for connection attempts
- [ ] Two-factor authentication support

### Advanced Features (Stubs)

The codebase includes stub methods for:
- Process name obfuscation (ethical concerns - requires careful implementation)
- Video driver-level screen capture (Windows Desktop Duplication API)
- Remote input event processing (with injection attack prevention)

These are **not implemented** and require significant additional work and ethical consideration before use.

---

## Development

### Project Structure

```
InvisibleRDP/
├── InvisibleRDP.Core/           # Core library
│   ├── Models/                  # Data models
│   ├── Services/                # Service implementations
│   ├── Interfaces/              # Service interfaces
│   └── Utils/                   # Utilities and helpers
├── InvisibleRDP.Service/        # Windows Service
│   ├── Program.cs               # Service entry point
│   └── SystemHostSvcWorker.cs   # Background worker
├── InvisibleRDP.ConsentUI/      # Consent application
│   └── Program.cs               # Console-based consent UI
├── InvisibleRDP.Uninstaller/    # Uninstaller utility
│   └── Program.cs               # Uninstallation logic
└── README.md                    # This file
```

### Building from Source

```bash
# Clone repository
git clone https://github.com/fmuoria/Invisible-RDP.git
cd Invisible-RDP

# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests (when available)
dotnet test
```

### Contributing

Contributions are welcome! Please ensure:
- All code follows ethical guidelines
- Security considerations are addressed
- Comprehensive testing is included
- Documentation is updated
- Consent requirements are preserved

---

## Support and Documentation

### Getting Help

- **Issues**: Report bugs or request features via GitHub Issues
- **Documentation**: This README and inline code comments
- **Security Issues**: Report privately to maintainers

### License

[Specify your license here - e.g., MIT, GPL, Apache 2.0]

### Acknowledgments

This project is designed with ethical remote access as a priority. Special thanks to the community for emphasizing responsible software development practices.

---

## Disclaimer

This software is provided "as is" without warranty of any kind. The authors and maintainers are not responsible for any misuse of this software. Users are solely responsible for ensuring their use complies with all applicable laws, regulations, and ethical standards.

**Remember: Always obtain explicit consent, maintain transparency, and respect user privacy.**