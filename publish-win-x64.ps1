# publish-win-x64.ps1
# Build script for publishing Retro Launcher win-x64 self-contained single-file executable

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Retro Launcher - Windows x64 Release Publishing Pipeline   " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Stop running RetroLauncher processes
Write-Host "[1/6] Stopping any running RetroLauncher processes..." -ForegroundColor Yellow
$procs = Get-Process -Name "RetroLauncher" -ErrorAction SilentlyContinue
if ($procs) {
    foreach ($p in $procs) {
        Write-Host "   Terminating PID $($p.Id)..." -ForegroundColor Red
        Stop-Process -Id $p.Id -Force
    }
    Start-Sleep -Seconds 1
} else {
    Write-Host "   No running processes found." -ForegroundColor Green
}

# 2. Delete bin and obj folders
Write-Host "[2/6] Cleaning bin and obj directories..." -ForegroundColor Yellow
$binPath = Join-Path $ScriptDir "bin"
$objPath = Join-Path $ScriptDir "obj"

if (Test-Path $binPath) {
    Remove-Item -Path $binPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   Removed $binPath" -ForegroundColor Gray
}
if (Test-Path $objPath) {
    Remove-Item -Path $objPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   Removed $objPath" -ForegroundColor Gray
}

# 3. Restore dependencies
Write-Host "[3/6] Restoring dependencies..." -ForegroundColor Yellow
Set-Location -Path $ScriptDir
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet restore failed!"
    exit 1
}

# 4. Publish Release win-x64 self-contained single-file
Write-Host "[4/6] Publishing Release win-x64 self-contained executable..." -ForegroundColor Yellow
$publishArgs = @(
    "publish",
    "-c", "Release",
    "-r", "win-x64",
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:PublishTrimmed=false",
    "-p:DebugType=none",
    "-p:DebugSymbols=false"
)

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed!"
    exit 1
}

# 5. Verify published output executable
Write-Host "[5/6] Verifying published executable..." -ForegroundColor Yellow
$expectedExe = Join-Path $ScriptDir "bin\Release\net10.0-windows\win-x64\publish\RetroLauncher.exe"

if (-not (Test-Path $expectedExe)) {
    Write-Error "Published executable NOT found at expected location: $expectedExe"
    exit 1
}

# 6. Print output path and timestamp
$exeItem = Get-Item $expectedExe
Write-Host "[6/6] Build & Publication Successful!" -ForegroundColor Green
Write-Host "------------------------------------------------------------" -ForegroundColor Cyan
Write-Host "  Executable Path : $($exeItem.FullName)" -ForegroundColor White
Write-Host "  File Size       : $([math]::Round($exeItem.Length / 1MB, 2)) MB" -ForegroundColor White
Write-Host "  Build Timestamp : $($exeItem.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
Write-Host "------------------------------------------------------------" -ForegroundColor Cyan
