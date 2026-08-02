# Retro Launcher

[![Build Status](https://github.com/TolgaRendaSimsek/Retro-Launcher/actions/workflows/release.yml/badge.svg)](https://github.com/TolgaRendaSimsek/Retro-Launcher/actions/workflows/release.yml)
[![Latest Release](https://img.shields.io/github/v/release/TolgaRendaSimsek/Retro-Launcher)](https://github.com/TolgaRendaSimsek/Retro-Launcher/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20x64-lightgrey.svg)]()
[![.NET Version](https://img.shields.io/badge/.NET-10.0--windows-purple.svg)](https://dotnet.microsoft.com/)

Retro Launcher is a modern Windows desktop application designed to streamline classic game library management, emulator installations, BIOS synchronization, global controller profiling, and application updates within a single, polished user interface.

---

## Overview

Retro Launcher provides a unified desktop dashboard for retro gaming setup and management. Rather than maintaining separate folders, shortcuts, and configuration files across multiple stand-alone emulators, Retro Launcher acts as a central hub to organize game libraries, manage emulator lifecycles, and synchronize controller profiles.

> **Important Legal Disclaimer**:
> Retro Launcher is an open-source library manager and interface tool. It **does not** contain, download, host, or distribute any copyrighted game ROMs, ISOs, BIOS files, or system firmware. Users must provide their own legally acquired game backups and system firmware files. Supported emulator engines remain independent third-party projects under their respective licenses.

---

## Current Status

**Status**: Work in Progress (Alpha / Active Development)

Retro Launcher is actively maintained and functional for Windows 64-bit platforms. All core management features—including game library launching, emulator package installations, BIOS folder syncing, controller profile synchronization, and GitHub Releases application updates—are fully implemented.

---

## Features

### Game Library
- **Organize & Search**: Catalog game titles, search by keyword, filter by platform or emulator.
- **Launch Support**: Launch games directly using registered emulator profiles.
- **Metadata & Covers**: Custom game detail view with cover image and release information support.

### Emulator Management
- **Lifecycle Control**: Perform clean Install, Launch, Repair, Reinstall, Update, and Uninstall actions for supported emulators.
- **Health Verification**: Check whether emulator executables and required installation subdirectories exist before launching.
- **Dynamic Configuration**: Adapt UI controls and installation paths based on the selected engine.

### Supported Emulators

| Console | Emulator Engine | Package Installation | Game Launching | BIOS / Firmware Sync | Controller Profile Sync | Status |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| PlayStation 1 | DuckStation | Supported | Supported | Supported | Supported | Supported |
| PlayStation 2 | PCSX2 | Supported | Supported | Supported | Supported | Supported |
| PlayStation 3 | RPCS3 | Supported | Supported | Supported | Supported | Supported |
| PlayStation Portable | PPSSPP | Supported | Supported | Supported | Supported | Supported |
| GameCube / Wii | Dolphin | Supported | Supported | Supported | Supported | Supported |

### BIOS and Firmware Management
- **Central BIOS Directory**: Maintained under `%LocalAppData%\RetroLauncher\BIOS`.
- **Manual & Batch Sync**: Sync individual emulators or use **Sync All BIOS** to process every installed emulator.
- **Safe Copy Logic**: Scans for compatible firmware files, copies missing or updated files to emulator target directories, and preserves existing user files without overwriting.

### Controller Profiles
- **Master Controller Configuration**: Single configuration screen for Players 1 through 4.
- **Input Methods**: Supports XInput controllers, DirectInput gamepads, and keyboard mappings.
- **Calibration Settings**: Adjust deadzone, sensitivity, trigger threshold, axis inversion, and rumble intensity.
- **Global Hotkeys**: Configure hotkeys for Pause, Save State, Load State, Fast Forward, Screenshot, and Toggle Menu.
- **Auto Sync**: Apply global controller profiles manually or automatically on game launch.

### Package and Download Management
- **Automated Downloads**: Download and unpack emulator archives (ZIP and 7z) via `PackageManagerService`.
- **Manual Local Archives**: Support for installing custom local archives directly into the package registry.
- **Logging & Progress**: Real-time extraction progress indicators and error logging (`%LocalAppData%\RetroLauncher\Logs\package_manager.log`).

### Application Updates
- **GitHub Releases Integration**: Check for official Retro Launcher releases via `https://api.github.com/repos/TolgaRendaSimsek/Retro-Launcher/releases/latest`.
- **Version Resolution**: Uses semantic version parsing (`System.Version`) from assembly metadata (`IApplicationVersionProvider`).
- **Automated Installer**: Downloads `RetroLauncher-win-x64-v{version}.zip`, extracts to staging, executes an external updater mode (`--updater`), backs up existing binaries, preserves user data in `%LocalAppData%\RetroLauncher`, replaces binaries, and restarts the application.
- **CI/CD Pipeline**: GitHub Actions workflow (`.github/workflows/release.yml`) automatically builds, tests, and publishes self-contained release packages on `v*.*.*` version tags.

---

## Screenshots

> **Note**: Screenshot documentation assets will be added here in a future update.
> - `docs/screenshots/home.png` — Home Dashboard & Quick Actions
> - `docs/screenshots/library.png` — Game Library Grid & Search
> - `docs/screenshots/emulators.png` — Emulator Management Panel
> - `docs/screenshots/downloads.png` — Package Downloads & Package Details

---

## Requirements

- **Operating System**: Windows 10 or Windows 11 (64-bit)
- **Architecture**: `win-x64`
- **.NET Runtime**: Included in self-contained single-file release packages (no separate .NET installation required)
- **Development Runtime**: .NET 10.0 SDK (only required when building from source code)
- **Storage & Connectivity**: Internet access for downloading emulator binaries and application updates; sufficient local disk space for game ROMs and emulator dependencies.

---

## Installation

### Recommended: GitHub Release

1. Visit the official [Retro Launcher Releases](https://github.com/TolgaRendaSimsek/Retro-Launcher/releases) page.
2. Download the latest `RetroLauncher-win-x64-v{version}.zip` release package.
3. Extract the ZIP archive to your preferred folder.
4. Run `RetroLauncher.exe`.
5. Complete the first-time setup wizard to configure your default directories.

### Build from Source

Requirements: .NET 10.0 SDK installed on Windows 64-bit.

```powershell
# Clone the repository
git clone https://github.com/TolgaRendaSimsek/Retro-Launcher.git
cd Retro-Launcher

# Restore dependencies
dotnet restore

# Build Debug configuration
dotnet build -c Debug

# Execute unit tests & launch application
dotnet run -c Debug
```

To build a standalone Release package:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

The published single-file executable will be generated at `bin\Release\net10.0-windows\win-x64\publish\RetroLauncher.exe`.

---

## Application Data Paths

All writable application settings, logs, and downloaded packages are stored under `%LocalAppData%\RetroLauncher` to avoid Windows permission restrictions:

- **Config**: `%LocalAppData%\RetroLauncher\Config` (`settings.json`, `emulators.json`, `games.json`, `global_controller_config.json`)
- **Emulators**: `%LocalAppData%\RetroLauncher\Emulators`
- **BIOS**: `%LocalAppData%\RetroLauncher\BIOS`
- **Saves**: `%LocalAppData%\RetroLauncher\Saves`
- **Logs**: `%LocalAppData%\RetroLauncher\Logs`
- **Updates**: `%LocalAppData%\RetroLauncher\Updates`

---

## License

Retro Launcher is licensed under the [Apache License 2.0](LICENSE).
