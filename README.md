# RoboCopy GUI

<div align="center">

![AI Assisted](https://img.shields.io/badge/AI-Assisted-FF6F00?style=for-the-badge&logo=openai&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A modern, feature-rich GUI for Windows RoboCopy**

> **This project was built with AI assistance ([OpenCode](https://github.com/opencode-ai/opencode)).**

</div>

---

## Features

- **Task Queue Management** — Add multiple copy tasks and execute them sequentially
- **Real-time Progress Monitoring** — Live progress bars, file tracking, and speed display
- **Configuration Presets** — Save and load RoboCopy parameter presets as JSON
- **Dry Run Preview** — Preview changes before executing with `/L` flag
- **Batch Export** — Export tasks as `.bat` or `.ps1` scripts
- **Modern UI** — WinUI 3 with Mica backdrop material
- **Auto-save** — Queue and settings persist automatically
- **MVVM Architecture** — Clean, maintainable code with CommunityToolkit.Mvvm

## Requirements

- **OS:** Windows 10 version 1903+ (build 18362) or Windows 11
- **Runtime:** .NET 8 Desktop Runtime (for framework-dependent builds)
- **RoboCopy:** Built into Windows (System32\robocopy.exe)

## Installation

### Download Release

1. Go to [Releases](../../releases)
2. Download `RoboCopyGUI-win-x64.zip` (or `win-arm64` for ARM devices)
3. Extract and run `RoboCopyGUI.exe`

### Build from Source

```bash
# Clone the repository
git clone https://github.com/your-username/RoboCopyGUI.git
cd RoboCopyGUI

# Build
dotnet build src/RoboCopyGUI/RoboCopyGUI.csproj -c Release -r win-x64

# Publish (self-contained)
dotnet publish src/RoboCopyGUI/RoboCopyGUI.csproj -c Release -r win-x64 \
  -p:WindowsAppSDKSelfContained=true -p:SelfContained=true -o publish/
```

## Usage

### Adding Tasks

1. Enter **Source** and **Destination** paths in the input fields
2. Click **Add Task** or press Enter
3. Configure options in the right panel (optional)
4. Repeat for multiple tasks

### Running Tasks

- **Start Queue** — Execute all pending tasks sequentially
- **Play** button on a task — Execute a single task
- **Dry Run** button — Preview what would be copied (no actual changes)

### Presets

1. Configure options for a task
2. Enter a preset name and click **Save**
3. Select a preset from the list and click **Apply to Selected**

### Exporting Scripts

- **Export .bat** — Generate a Windows batch script
- **Export .ps1** — Generate a PowerShell script
- **Export Queue** — Save task list as JSON for later import

## Project Structure

```
RoboCopyGUI/
├── .github/workflows/          # CI/CD pipelines
├── src/RoboCopyGUI/
│   ├── Controls/               # XAML UserControls
│   │   ├── AppTitleBar.xaml    # Custom title bar
│   │   ├── PresetPanel.xaml    # Preset management
│   │   ├── RoboCopyOptionsPanel.xaml  # RoboCopy parameters
│   │   └── SettingsPanel.xaml  # App settings
│   ├── Converters/             # XAML value converters
│   ├── Models/                 # Data models
│   │   ├── AppSettings.cs      # Settings model
│   │   ├── CopyTask.cs         # Task data
│   │   ├── RoboCopyOptions.cs  # RoboCopy parameters
│   │   └── RoboCopyResult.cs   # Execution result
│   ├── Services/               # Business logic
│   │   ├── AppSettingsService.cs
│   │   ├── BatchScriptExporter.cs
│   │   ├── PresetManager.cs
│   │   ├── RoboCopyService.cs  # Core robocopy wrapper
│   │   └── TaskQueuePersistence.cs
│   ├── Styles/                 # Shared XAML styles
│   ├── ViewModels/             # MVVM ViewModels
│   │   ├── CopyTaskViewModel.cs
│   │   └── MainViewModel.cs
│   ├── App.xaml(.cs)
│   ├── MainWindow.xaml(.cs)
│   └── Program.cs
└── RoboCopyGUI.sln
```

## RoboCopy Options Reference

| Option | Description |
|--------|-------------|
| `/MIR` | Mirror directory tree |
| `/S` | Copy subdirectories (non-empty) |
| `/E` | Copy subdirectories (including empty) |
| `/Z` | Restartable mode |
| `/B` | Backup mode |
| `/J` | Unbuffered I/O |
| `/MT:N` | Multi-threaded (N threads) |
| `/L` | List only (dry run) |
| `/V` | Verbose output |
| `/ETA` | Show estimated time |
| `/R:N` | Retry count |
| `/W:N` | Wait between retries (seconds) |

## Data Storage

All user data is stored in `%LocalAppData%\RoboCopyGUI\`:

```
%LocalAppData%\RoboCopyGUI\
├── settings.json      # Application settings
├── queue.json         # Auto-saved task queue
└── presets\           # Saved presets
    ├── preset_1.json
    └── preset_2.json
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Acknowledgments

- Built with [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) (WinUI 3)
- MVVM powered by [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- AI-assisted development with [OpenCode](https://github.com/opencode-ai/opencode)

---

<div align="center">

**Made with AI assistance** — This project's code was generated and reviewed with the help of OpenCode.

</div>
