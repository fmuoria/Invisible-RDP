# InvisibleRDP Setup Guide

This guide will walk you through setting up InvisibleRDP for remote desktop access with full transparency and user consent.

## Quick Start

### For the Host Computer (Server)

1. **Install the Host application**
   - Download `InvisibleRDP-Host-Setup.msi`
   - Run as Administrator
   - Read and accept the consent agreement
   - Installation completes automatically

2. **Verify Installation**
   - Look for the shield icon in the system tray (bottom-right of Windows taskbar)
   - Right-click the icon to access options
   - Check that the service is running: Open Task Manager → Services tab → Look for "SystemHostSvc"

3. **Configure Access Password**
   - By default, the password is "default"
   - **IMPORTANT**: Change this immediately for security!
   - Right-click system tray icon → "Change Password" (or use Registry method below)

   **Via Registry:**
   ```powershell
   Set-ItemProperty -Path "HKLM:\SOFTWARE\InvisibleRDP" -Name "AccessPassword" -Value "YourSecurePassword"
   Restart-Service SystemHostSvc
   ```

4. **Configure Firewall**
   - Windows Firewall will need to allow incoming connections on port 9876
   - Run as Administrator:
   ```powershell
   New-NetFirewallRule -DisplayName "InvisibleRDP Host" `
                       -Direction Inbound `
                       -Protocol TCP `
                       -LocalPort 9876 `
                       -Action Allow `
                       -Profile Any
   ```

5. **Note Your IP Address**
   - Open Command Prompt and run: `ipconfig`
   - Look for your IPv4 address (e.g., 192.168.1.100)
   - Share this IP and your password with the viewer

### For the Viewer Computer (Client)

1. **Install the Viewer application**
   - Download `InvisibleRDP-Viewer-Setup.msi`
   - Run the installer (Administrator not required)
   - Installation completes automatically

2. **Launch the Viewer**
   - Double-click the desktop shortcut or find in Start Menu
   - The connection dialog will appear

3. **Connect to a Host**
   - Enter the host's IP address (e.g., 192.168.1.100)
   - Enter the port (default: 9876)
   - Enter the access password
   - Click "Connect"

4. **Control the Remote Desktop**
   - Once connected, you can use your mouse and keyboard to control the remote computer
   - The host user will see the system tray icon and can monitor the connection
   - Click "Disconnect" when finished

## Network Configuration

### Local Network (Same Wi-Fi/LAN)

If both computers are on the same network:
- Use the host's local IP address (e.g., 192.168.1.100)
- Port 9876 should work without additional configuration
- No router configuration needed

### Internet Connection (Remote Access)

To connect over the internet:

1. **Configure Port Forwarding on Host's Router**
   - Access your router's admin panel (usually http://192.168.1.1)
   - Find "Port Forwarding" or "NAT" settings
   - Forward external port 9876 to internal IP of host computer
   - Save changes

2. **Find Your Public IP**
   - On the host computer, visit https://whatismyipaddress.com/
   - Note your public IP address (e.g., 203.0.113.10)

3. **Connect Using Public IP**
   - In the viewer, enter the public IP address
   - Use port 9876
   - Enter the password

**Security Note:** Remote internet access increases security risks. Use a strong password and consider using a VPN.

## User Consent and Transparency

### Host User Must Know

The host application is designed to be **fully transparent**:

1. **System Tray Icon**: Always visible when running
   - Shield icon in the system tray
   - Hover to see status
   - Right-click for options

2. **Consent Required**: Must be accepted before any connections are allowed
   - Shown during installation
   - Can be reviewed anytime via system tray

3. **Connection Notifications**: 
   - Toast notification when a viewer connects
   - Status window shows active sessions
   - All connections are logged

4. **User Control**:
   - Can view logs at any time (right-click system tray → View Logs)
   - Can stop the service (right-click system tray → Stop Service)
   - Can exit and disable access (right-click system tray → Exit)

### Viewing Logs

To see who connected and when:
1. Right-click the system tray icon
2. Select "View Logs"
3. Logs show:
   - Connection attempts
   - IP addresses
   - Timestamps
   - Success/failure status
   - Session durations

## Troubleshooting

### Host: Service Won't Start

**Symptoms:** System tray icon shows "Service Not Running" or doesn't start

**Solutions:**
1. Check consent was granted:
   ```powershell
   Test-Path "C:\ProgramData\InvisibleRDP\Consent\consent.json"
   ```
   If false, run the consent UI again

2. Check service status:
   ```powershell
   Get-Service SystemHostSvc
   ```

3. Check Windows Event Viewer:
   - Open Event Viewer → Windows Logs → Application
   - Look for "SystemHostSvc" entries

4. Restart the service:
   ```powershell
   Restart-Service SystemHostSvc
   ```

### Viewer: Cannot Connect

**Symptoms:** "Connection failed" or "Unable to connect to host"

**Checklist:**
- [ ] Host service is running (check system tray icon)
- [ ] IP address is correct (double-check with host user)
- [ ] Port is correct (default: 9876)
- [ ] Password is correct (case-sensitive)
- [ ] Firewall allows port 9876 on host
- [ ] Router port forwarding is configured (if connecting over internet)
- [ ] Both computers can ping each other

**Test Connection:**
```powershell
# On viewer computer, test if host port is open
Test-NetConnection -ComputerName 192.168.1.100 -Port 9876
```

### Host: Firewall Blocking Connections

**Symptoms:** Viewer times out or cannot reach host

**Solution:**
1. Open Windows Defender Firewall
2. Click "Allow an app or feature through Windows Defender Firewall"
3. Click "Change settings"
4. Look for "InvisibleRDP" or add manually:
   - Click "Allow another app"
   - Browse to `C:\Program Files\InvisibleRDP\Host\InvisibleRDP.Service.exe`
   - Check both Private and Public networks

Or use PowerShell (run as Administrator):
```powershell
New-NetFirewallRule -DisplayName "InvisibleRDP Host" `
                    -Direction Inbound `
                    -Program "C:\Program Files\InvisibleRDP\Host\InvisibleRDP.Service.exe" `
                    -Action Allow
```

### Performance Issues

**Symptoms:** Slow or laggy remote desktop

**Solutions:**
1. Check network speed: Run speed test on both computers
2. Close unnecessary applications on host
3. Reduce screen resolution if needed
4. Use local network instead of internet when possible
5. Check for high CPU/RAM usage on either computer

## Security Best Practices

1. **Strong Passwords**
   - Use at least 12 characters
   - Include uppercase, lowercase, numbers, and symbols
   - Don't share with untrusted parties

2. **Limited Exposure**
   - Only allow connections when needed
   - Exit the host app when not in use
   - Don't leave port forwarding enabled permanently

3. **Monitor Logs**
   - Review connection logs regularly
   - Investigate any unexpected connections
   - Check audit log: `C:\ProgramData\InvisibleRDP\Logs\audit.log`

4. **Network Security**
   - Use a VPN when connecting over internet
   - Keep both computers updated with latest Windows updates
   - Use antivirus software

5. **Consent**
   - Never install on a computer without user knowledge
   - Always explain what the software does
   - Provide access to logs and controls

## Uninstallation

### Uninstalling Host

**Option 1: Windows Settings**
1. Open Settings → Apps → Installed Apps
2. Find "InvisibleRDP Host"
3. Click the three dots → Uninstall
4. Follow prompts

**Option 2: Control Panel**
1. Open Control Panel → Programs → Programs and Features
2. Find "InvisibleRDP Host"
3. Right-click → Uninstall

**Complete Cleanup:**
```powershell
# Run as Administrator
sc stop SystemHostSvc
sc delete SystemHostSvc
Remove-Item "C:\Program Files\InvisibleRDP" -Recurse -Force
Remove-Item "HKLM:\SOFTWARE\InvisibleRDP" -Recurse -Force
Remove-Item "$env:ProgramData\InvisibleRDP" -Recurse -Force
```

### Uninstalling Viewer

Same as Host, but look for "InvisibleRDP Viewer" in the app list.

## Advanced Configuration

### Changing the Port

Default port is 9876. To change:

```powershell
# Stop the service
Stop-Service SystemHostSvc

# Change port in registry
Set-ItemProperty -Path "HKLM:\SOFTWARE\InvisibleRDP" -Name "Port" -Value 8080

# Update firewall rule
Remove-NetFirewallRule -DisplayName "InvisibleRDP Host"
New-NetFirewallRule -DisplayName "InvisibleRDP Host" `
                    -Direction Inbound `
                    -Protocol TCP `
                    -LocalPort 8080 `
                    -Action Allow

# Start the service
Start-Service SystemHostSvc
```

### Auto-Start Configuration

Host is configured to start automatically with Windows. To disable:

```powershell
# Disable auto-start of service
Set-Service -Name SystemHostSvc -StartupType Manual

# Remove system tray from startup
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "InvisibleRDP"
```

### Log File Location

Audit logs are stored at:
```
C:\ProgramData\InvisibleRDP\Logs\audit.log
```

Logs rotate automatically when they reach 50 MB. Up to 10 historical logs are kept.

## Support

For help, issues, or feature requests:
- GitHub Issues: https://github.com/fmuoria/Invisible-RDP/issues
- Read the full documentation: README.md
- Check installer documentation: Installers/README.md

## Legal and Ethics

**Remember:**
- Always obtain explicit consent before installing on any computer
- Maintain full transparency with users
- Respect privacy and data protection laws
- Use only for legitimate purposes
- Comply with all applicable laws and regulations

**Unauthorized access to computer systems is illegal and unethical.**
