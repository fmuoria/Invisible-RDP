<#
PowerShell script: tools\build-msi.ps1
Purpose: Build Invisible-RDP solution and produce Host and Viewer MSI installers locally.
Save this file at the repository root (.\tools\build-msi.ps1) and run from the repo root.
#>

[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$OutputDir = ".\artifacts",
    [switch]$SkipWiXInstall,
    [switch]$Sign,
    [string]$CertThumbprint = '',
    [string]$SigntoolPath = ''
)

function Write-Info { param($m) Write-Host "[INFO] $m" -ForegroundColor Cyan }
function Write-Success { param($m) Write-Host "[OK] $m" -ForegroundColor Green }
function Write-Warn { param($m) Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Write-Err { param($m) Write-Host "[ERROR] $m" -ForegroundColor Red }

try {
    Write-Info "Starting build-msi.ps1 script"
    $scriptRoot = (Resolve-Path .).Path
    Write-Info "Repo root: $scriptRoot"

    # Check running OS
    if ($env:OS -notlike "*Windows*") {
        throw "This script must be run on Windows."
    }

    # Check dotnet SDK
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "dotnet CLI not found. Install .NET 8.0 SDK: https://dotnet.microsoft.com/download"
    }

    $sdks = & dotnet --list-sdks 2>$null
    if (-not $sdks) {
        Write-Warn "Could not enumerate installed SDKs; proceeding but ensure .NET 8.0 SDK is installed."
    } else {
        if ($sdks -notmatch '^8\.') {
            Write-Warn "No .NET 8.x SDK detected in 'dotnet --list-sdks' output. Ensure .NET 8.0 SDK is installed."
        } else {
            Write-Info "Found .NET 8 SDK."
        }
    }

    # Ensure WiX Toolset or dotnet-wix tool available
    $wixFound = $false
    $wixBin = "C:\Program Files\WiX Toolset v4\bin"
    if (Test-Path (Join-Path $wixBin "wix.exe") -PathType Leaf -ErrorAction SilentlyContinue -or
        Test-Path (Join-Path $wixBin "candle.exe") -PathType Leaf -ErrorAction SilentlyContinue) {
        $wixFound = $true
        Write-Info "Found WiX Toolset in $wixBin"
    } else {
        if (Get-Command wix -ErrorAction SilentlyContinue) {
            $wixFound = $true
            Write-Info "Found 'wix' command on PATH."
        }
    }

    if (-not $wixFound) {
        if ($SkipWiXInstall) {
            Write-Warn "WiX not detected and SkipWiXInstall specified. Continuing; builds may fail if WiX is required."
        } else {
            if (Get-Command choco -ErrorAction SilentlyContinue) {
                Write-Info "Attempting to install WiX Toolset via Chocolatey..."
                & choco install wixtoolset -y --no-progress
                $wixFound = Test-Path (Join-Path $wixBin "wix.exe")
                if ($wixFound) {
                    Write-Info "WiX installed to $wixBin"
                } else {
                    Write-Warn "WiX installation via choco completed but tool path not detected. You may need to restart shell or add WiX bin to PATH."
                }
            } else {
                Write-Warn "Chocolatey not found. Install WiX Toolset v4 manually: https://wixtoolset.org/"
                throw "WiX Toolset not found. Set SkipWiXInstall to bypass or install WiX and re-run."
            }
        }
    }

    # Restore and build solution
    Write-Info "Restoring NuGet packages..."
    & dotnet restore InvisibleRDP.sln
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

    Write-Info "Building solution in $Configuration..."
    & dotnet build InvisibleRDP.sln --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

    # Build Host installer
    $hostInstallerProj = "Installers/Host/HostInstaller.wixproj"
    if (Test-Path $hostInstallerProj) {
        Write-Info "Building Host installer project..."
        Push-Location (Split-Path $hostInstallerProj)
        & dotnet build (Split-Path $hostInstallerProj -Leaf) --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Host installer build failed." }
        Pop-Location
    } else {
        Write-Warn "Host installer project not found at $hostInstallerProj, skipping Host installer build."
    }

    # Build Client/Viewer installer
    $clientInstallerProj = "Installers/Client/ClientInstaller.wixproj"
    if (Test-Path $clientInstallerProj) {
        Write-Info "Building Viewer (Client) installer project..."
        Push-Location (Split-Path $clientInstallerProj)
        & dotnet build (Split-Path $clientInstallerProj -Leaf) --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Client installer build failed." }
        Pop-Location
    } else {
        Write-Warn "Client installer project not found at $clientInstallerProj, skipping Client installer build."
    }

    # Collect produced MSIs
    Write-Info "Collecting produced MSI files..."
    $msiFiles = @()
    $hostMsiPattern = Join-Path $scriptRoot "Installers\Host\bin\$Configuration\*.msi"
    $clientMsiPattern = Join-Path $scriptRoot "Installers\Client\bin\$Configuration\*.msi"
    $msiFiles += Get-ChildItem -Path $hostMsiPattern -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
    $msiFiles += Get-ChildItem -Path $clientMsiPattern -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }

    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }

    if ($msiFiles.Count -eq 0) {
        Write-Warn "No MSI files found. Check build logs and confirm installer projects produced MSIs at expected paths."
    } else {
        foreach ($f in $msiFiles) {
            $dest = Join-Path (Resolve-Path $OutputDir).Path (Split-Path $f -Leaf)
            Copy-Item -Path $f -Destination $dest -Force
            Write-Success "Copied MSI: $dest"
        }
    }

    # Optional signing
    if ($Sign.IsPresent) {
        if ([string]::IsNullOrEmpty($CertThumbprint)) {
            Write-Err "Sign specified but CertThumbprint not provided. Skipping signing."
        } else {
            if (-not $SigntoolPath) {
                $signtoolCmd = Get-Command signtool -ErrorAction SilentlyContinue
                if ($signtoolCmd) {
                    $SigntoolPath = $signtoolCmd.Source
                } else {
                    Write-Warn "signtool.exe not found in PATH. Provide -SigntoolPath or install Windows SDK that provides signtool.exe."
                }
            }
            if (-not (Test-Path $SigntoolPath) -and -not (Get-Command signtool -ErrorAction SilentlyContinue)) {
                Write-Err "signtool.exe not available. Cannot sign MSIs."
            } else {
                $signtoolExe = if ($SigntoolPath) { $SigntoolPath } else { "signtool" }
                Write-Info "Signing MSIs with certificate thumbprint $CertThumbprint..."
                $artifacts = Get-ChildItem -Path $OutputDir -Filter *.msi -File -ErrorAction SilentlyContinue
                if ($artifacts.Count -eq 0) {
                    Write-Warn "No MSIs found in $OutputDir to sign."
                } else {
                    foreach ($a in $artifacts) {
                        Write-Info "Signing $($a.FullName)..."
                        & $signtoolExe sign /fd SHA256 /sha1 $CertThumbprint /tr "http://timestamp.digicert.com" /td SHA256 /v $a.FullName
                        if ($LASTEXITCODE -eq 0) {
                            Write-Success "Signed $($a.Name)"
                        } else {
                            Write-Err "Failed to sign $($a.Name). signtool exit code: $LASTEXITCODE"
                        }
                    }
                }
            }
        }
    }

    Write-Success "Script finished. Check $OutputDir for produced MSI files."
    if ($msiFiles.Count -gt 0) {
        Write-Info "Produced MSIs:"
        Get-ChildItem -Path $OutputDir -Filter *.msi -File | ForEach-Object { Write-Host " - $($_.FullName)" }
    }

} catch {
    Write-Err $_.Exception.Message
    if ($_.Exception.InnerException) { Write-Err $_.Exception.InnerException.Message }
    exit 1
}
