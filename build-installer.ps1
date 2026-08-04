# build-installer.ps1
# Build-before-installer script that publishes Release win-x64 and compiles Inno Setup installer

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Retro Launcher - Build & Installer Packaging Pipeline      " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Execute publish-win-x64.ps1
Write-Host "[1/3] Running publish-win-x64.ps1..." -ForegroundColor Yellow
$publishScript = Join-Path $ScriptDir "publish-win-x64.ps1"
if (-not (Test-Path $publishScript)) {
    Write-Error "publish-win-x64.ps1 script not found!"
    exit 1
}

& $publishScript
if ($LASTEXITCODE -ne 0) {
    Write-Error "Publishing step failed!"
    exit 1
}

# 2. Verify published output
Write-Host "[2/3] Verifying published output..." -ForegroundColor Yellow
$pubExe = Join-Path $ScriptDir "bin\Release\net10.0-windows\win-x64\publish\RetroLauncher.exe"
if (-not (Test-Path $pubExe)) {
    Write-Error "Published executable missing at $pubExe"
    exit 1
}

$pubItem = Get-Item $pubExe
Write-Host "   Published EXE verified: $($pubItem.FullName) ($($pubItem.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')))" -ForegroundColor Green

# 3. Locate & Compile Inno Setup Script
Write-Host "[3/3] Locating Inno Setup Compiler (ISCC.exe)..." -ForegroundColor Yellow
$issPath = Join-Path $ScriptDir "RetroLauncher.iss"

if (-not (Test-Path $issPath)) {
    Write-Error "RetroLauncher.iss script not found at $issPath"
    exit 1
}

$isccPaths = @(
    "ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe"
)

$isccExe = $null
foreach ($path in $isccPaths) {
    $cmd = Get-Command $path -ErrorAction SilentlyContinue
    if ($cmd) {
        $isccExe = $cmd.Source
        break
    }
    if (Test-Path $path) {
        $isccExe = $path
        break
    }
}

if ($isccExe) {
    Write-Host "   Found ISCC compiler at: $isccExe" -ForegroundColor Green
    Write-Host "   Compiling $issPath..." -ForegroundColor Yellow
    & $isccExe $issPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Inno Setup compilation failed!"
        exit 1
    }
    Write-Host "   Inno Setup Installer compiled successfully!" -ForegroundColor Green
} else {
    Write-Host "   Notice: Inno Setup (ISCC.exe) is not installed in standard PATH." -ForegroundColor Magenta
    Write-Host "   The published single-file package is 100% built and ready at:" -ForegroundColor White
    Write-Host "   $pubExe" -ForegroundColor Cyan
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Build & Installer Pipeline Completed Successfully!         " -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
