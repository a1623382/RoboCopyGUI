# RoboCopy GUI

<div align="center">

![AI Assisted](https://img.shields.io/badge/AI-Assisted-FF6F00?style=for-the-badge&logo=openai&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A modern GUI for Windows RoboCopy / 一款现代化的 Windows RoboCopy 图形界面工具**

> **本项目由 AI (OpenCode) 辅助生成。**
>
> **This project was built with AI assistance ([OpenCode](https://github.com/opencode-ai/opencode)).**

</div>

---

## Features / 功能特性

| Feature | Description |
|---------|-------------|
| **Task Queue / 任务队列** | Add multiple copy tasks and execute sequentially / 将多个复制任务加入队列按顺序执行 |
| **Real-time Progress / 实时进度** | Live progress bars and log panel / 实时进度条和日志面板 |
| **Presets / 配置预设** | Save/load RoboCopy parameters as JSON / 将常用参数保存为 JSON 预设 |
| **Dry Run / 空运行** | Preview changes before executing / 执行前预览将要发生的变更 |
| **Script Export / 脚本导出** | Export as `.bat` or `.ps1` scripts / 导出为批处理或 PowerShell 脚本 |
| **Modern UI / 现代界面** | WinUI 3 with Mica backdrop / WinUI 3 + Mica 云母材质背景 |
| **Auto-save / 自动保存** | Queue and settings persist automatically / 队列和设置自动持久化 |

---

## Requirements / 系统要求

- **OS:** Windows 10 1903+ (build 18362) 或 Windows 11
- **Runtime:** .NET 8 Desktop Runtime (框架依赖版本)
- **RoboCopy:** 系统自带 (System32\robocopy.exe)

---

## Installation / 安装

### Download Release / 下载发布版

1. Go to [Releases](../../releases) / 前往 [Releases](../../releases)
2. Download `RoboCopyGUI-win-x64.zip` (or `win-arm64` for ARM) / 下载对应架构的压缩包
3. Extract and run `RoboCopyGUI.exe` / 解压后运行

### Build from Source / 从源码构建

```bash
git clone https://github.com/your-username/RoboCopyGUI.git
cd RoboCopyGUI

# Build / 构建
dotnet build src/RoboCopyGUI/RoboCopyGUI.csproj -c Release -r win-x64

# Publish (self-contained) / 发布（自包含）
dotnet publish src/RoboCopyGUI/RoboCopyGUI.csproj -c Release -r win-x64 \
  -p:WindowsAppSDKSelfContained=true -p:SelfContained=true -o publish/
```

---

## Usage / 使用方法

### Adding Tasks / 添加任务

1. Enter **Source** and **Destination** paths / 输入源路径和目标路径
2. Click **Add Task** / 点击"添加任务"
3. Configure options in the right panel / 在右侧面板配置选项（可选）
4. Click **Start Queue** to execute all / 点击"开始队列"执行全部任务

### Presets / 预设管理

1. Configure options for a task / 配置任务选项
2. Enter a preset name and click **Save** / 输入预设名称并保存
3. Select a preset and click **Apply** / 选择预设并应用到任务

### Export Scripts / 导出脚本

- **Export .bat** — Generate a Windows batch script / 生成批处理脚本
- **Export .ps1** — Generate a PowerShell script / 生成 PowerShell 脚本

---

## Project Structure / 项目结构

```
RoboCopyGUI/
├── .github/workflows/          # CI/CD pipelines
├── src/RoboCopyGUI/
│   ├── Controls/               # XAML UserControls
│   ├── Converters/             # XAML value converters
│   ├── Models/                 # Data models / 数据模型
│   ├── Services/               # Business logic / 业务逻辑
│   │   ├── RoboCopyService.cs  # Core robocopy wrapper
│   │   ├── PresetManager.cs    # Preset persistence
│   │   └── BatchScriptExporter.cs
│   ├── Styles/                 # Shared XAML styles
│   ├── ViewModels/             # MVVM ViewModels
│   ├── App.xaml(.cs)
│   └── MainWindow.xaml(.cs)
└── RoboCopyGUI.sln
```

---

## Data Storage / 数据存储

All user data is stored in `%LocalAppData%\RoboCopyGUI\`:

```
%LocalAppData%\RoboCopyGUI\
├── settings.json      # Application settings / 应用设置
├── queue.json         # Auto-saved task queue / 自动保存的任务队列
└── presets\           # Saved presets / 已保存的预设
```

---

## RoboCopy Options Reference / RoboCopy 参数速查

| Option | Description / 说明 |
|--------|-------------------|
| `/MIR` | Mirror directory tree / 镜像目录树 |
| `/S` | Copy subdirectories (non-empty) / 复制子目录（不含空目录） |
| `/E` | Copy subdirectories (including empty) / 复制子目录（含空目录） |
| `/Z` | Restartable mode / 可重启模式 |
| `/MT:N` | Multi-threaded (N threads) / 多线程复制 |
| `/L` | List only (dry run) / 仅列出（空运行） |
| `/V` | Verbose output / 详细输出 |
| `/ETA` | Show estimated time / 显示预估时间 |
| `/R:N` | Retry count / 重试次数 |
| `/W:N` | Wait between retries (seconds) / 重试间隔（秒） |

---

## License / 许可证

MIT License. See [LICENSE](LICENSE).

---

## Acknowledgments / 致谢

- Built with [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) (WinUI 3)
- MVVM powered by [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- AI-assisted development with [OpenCode](https://github.com/opencode-ai/opencode)

---

<div align="center">

**本项目代码由 AI (OpenCode) 辅助生成**

**Made with AI assistance**

</div>
