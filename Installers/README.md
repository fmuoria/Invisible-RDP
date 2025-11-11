# InvisibleRDP Installers

This directory contains the Windows installer projects for both the Host and Viewer applications.

## Prerequisites

To build the installers, you need:

1. **Windows 10/11** or Windows Server 2019+
2. **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download
3. **WiX Toolset v4** - Download from https://wixtoolset.org/
   ```powershell
   dotnet tool install --global wix
   ```

## Building the Installers

### Host Installer (Server)

The Host installer includes:
- InvisibleRDP.SystemTray.exe (System tray application)
- InvisibleRDP.Service.exe (Windows Service)
- SystemHostSvc Windows Service configuration
- Auto-start on Windows startup

**Build Steps:**

```powershell
# 1. Build the solution in Release mode
cd /path/to/Invisible-RDP
dotnet build InvisibleRDP.sln --configuration Release

# 2. Build the Host installer
cd Installers/Host
dotnet build HostInstaller.wixproj --configuration Release

# 3. The installer will be created at:
# Installers/Host/bin/Release/InvisibleRDP-Host-Setup.msi
```

**Installation:**
1. Run `InvisibleRDP-Host-Setup.msi`
2. Accept the consent dialog (mandatory)
3. The system tray app will start automatically
4. The Windows Service will be installed and started

**Default Settings:**
- Install location: `C:\Program Files\InvisibleRDP\Host`
- Service name: `SystemHostSvc`
- Port: 9876
- Default password: `default` (change after installation)

### Viewer Installer (Client)

The Viewer installer includes:
- InvisibleRDP.Viewer.exe (Viewer application)
- Desktop and Start Menu shortcuts

**Build Steps:**

```powershell
# 1. Build the solution in Release mode (if not already done)
cd /path/to/Invisible-RDP
dotnet build InvisibleRDP.sln --configuration Release

# 2. Build the Viewer installer
cd Installers/Client
dotnet build ClientInstaller.wixproj --configuration Release

# 3. The installer will be created at:
# Installers/Client/bin/Release/InvisibleRDP-Viewer-Setup.msi
```

**Installation:**
1. Run `InvisibleRDP-Viewer-Setup.msi`
2. Launch from Start Menu or Desktop shortcut
3. Enter host IP and password to connect

**Default Settings:**
- Install location: `C:\Program Files\InvisibleRDP\Viewer`

## Manual Installation (Without Installers)

If you cannot build the installers, you can manually install:

### Host (Server) Manual Installation

```powershell
# 1. Build the solution
dotnet build InvisibleRDP.sln --configuration Release

# 2. Copy files to installation directory
$installDir = "C:\Program Files\InvisibleRDP\Host"
New-Item -ItemType Directory -Path $installDir -Force
Copy-Item "InvisibleRDP.SystemTray\bin\Release\net8.0-windows\*" -Destination $installDir -Recurse
Copy-Item "InvisibleRDP.Service\bin\Release\net8.0\*" -Destination $installDir -Recurse

# 3. Install the Windows Service
sc.exe create SystemHostSvc binPath="$installDir\InvisibleRDP.Service.exe" start=auto
sc.exe start SystemHostSvc

# 4. Add system tray to startup
$startupPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $startupPath -Name "InvisibleRDP" -Value "$installDir\InvisibleRDP.SystemTray.exe"

# 5. Start the system tray app
Start-Process "$installDir\InvisibleRDP.SystemTray.exe"
```

### Viewer (Client) Manual Installation

```powershell
# 1. Build the solution
dotnet build InvisibleRDP.sln --configuration Release

# 2. Copy files to installation directory
$installDir = "C:\Program Files\InvisibleRDP\Viewer"
New-Item -ItemType Directory -Path $installDir -Force
Copy-Item "InvisibleRDP.Viewer\bin\Release\net8.0-windows\*" -Destination $installDir -Recurse

# 3. Create Start Menu shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\InvisibleRDP Viewer.lnk")
$Shortcut.TargetPath = "$installDir\InvisibleRDP.Viewer.exe"
$Shortcut.Save()
```

## Configuration

### Setting Access Password (Host)

The default password is "default". To change it:

**Method 1: Via Registry**
```powershell
Set-ItemProperty -Path "HKLM:\SOFTWARE\InvisibleRDP" -Name "AccessPassword" -Value "YourSecurePassword"
Restart-Service SystemHostSvc
```

**Method 2: Via System Tray**
Right-click the system tray icon → Settings → Change Password

### Firewall Configuration

Allow incoming connections on port 9876:

```powershell
New-NetFirewallRule -DisplayName "InvisibleRDP Host" `
                    -Direction Inbound `
                    -Protocol TCP `
                    -LocalPort 9876 `
                    -Action Allow `
                    -Profile Any
```

## Uninstallation

### Using Windows Programs and Features

1. Open Settings → Apps → Installed Apps
2. Find "InvisibleRDP Host" or "InvisibleRDP Viewer"
3. Click Uninstall

### Manual Uninstallation (Host)

```powershell
# 1. Stop and remove the service
sc.exe stop SystemHostSvc
sc.exe delete SystemHostSvc

# 2. Remove from startup
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "InvisibleRDP"

# 3. Remove files
Remove-Item "C:\Program Files\InvisibleRDP" -Recurse -Force

# 4. Remove registry entries
Remove-Item "HKLM:\SOFTWARE\InvisibleRDP" -Recurse -Force

# 5. Remove application data
Remove-Item "$env:ProgramData\InvisibleRDP" -Recurse -Force
```

### Manual Uninstallation (Viewer)

```powershell
# 1. Remove files
Remove-Item "C:\Program Files\InvisibleRDP\Viewer" -Recurse -Force

# 2. Remove shortcuts
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\InvisibleRDP Viewer.lnk" -Force
```

## Troubleshooting

### Installer Fails to Build

**Issue:** WiX errors during build

**Solution:**
1. Ensure WiX Toolset v4 is installed: `wix --version`
2. Update WiX: `dotnet tool update --global wix`
3. Check that all referenced binaries exist in the Release build output

### Service Fails to Start

**Issue:** SystemHostSvc service won't start

**Solution:**
1. Check Windows Event Viewer → Windows Logs → Application
2. Ensure user consent has been granted (run InvisibleRDP.ConsentUI.exe first)
3. Verify .NET 8.0 Runtime is installed
4. Check service account permissions

### Cannot Connect (Viewer)

**Issue:** Viewer cannot connect to host

**Solution:**
1. Verify SystemHostSvc service is running: `Get-Service SystemHostSvc`
2. Check firewall allows port 9876
3. Verify correct IP address and password
4. Check host logs: `C:\ProgramData\InvisibleRDP\Logs\audit.log`

## Security Notes

1. **Change Default Password:** Always change the default password after installation
2. **Firewall:** Only open port 9876 to trusted networks
3. **Consent:** Host requires explicit user consent before accepting connections
4. **Encryption:** Current version uses basic encryption - TLS recommended for production
5. **Updates:** Keep the software updated for security patches

## Advanced Configuration

### Custom Port

Edit registry before starting service:
```powershell
Set-ItemProperty -Path "HKLM:\SOFTWARE\InvisibleRDP" -Name "Port" -Value 8080
Restart-Service SystemHostSvc
```

### Stealth Mode (Not Recommended)

```powershell
Set-ItemProperty -Path "HKLM:\SOFTWARE\InvisibleRDP" -Name "StealthMode" -Value 1
```

**Warning:** Stealth mode reduces transparency and may violate consent requirements.

## Support

For issues, questions, or contributions:
- GitHub Issues: https://github.com/fmuoria/Invisible-RDP/issues
- Documentation: See main README.md

## License

See LICENSE file in the root directory.
