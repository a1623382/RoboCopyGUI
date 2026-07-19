using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoboCopyGUI.Models;
using RoboCopyGUI.Services;

namespace RoboCopyGUI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly RoboCopyService _roboCopyService = new();
    private readonly PresetManager _presetManager = new();
    private readonly AppSettingsService _settingsService = new();
    private readonly TaskQueuePersistence _queuePersistence = new();
    private readonly BatchScriptExporter _batchExporter = new();
    private CancellationTokenSource? _cts;
    private bool _isProcessingQueue;

    public MainViewModel()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _settingsService.LoadAsync();
        await LoadPresetsAsync();
        await LoadQueueAsync();

        if (_settingsService.Current.AutoStartQueueOnLaunch && Tasks.Count > 0)
            await StartQueueCommand.ExecuteAsync(null);
    }

    public AppSettings Settings => _settingsService.Current;

    public ObservableCollection<CopyTaskViewModel> Tasks { get; } = [];
    public ObservableCollection<LogEntry> LogEntries { get; } = [];
    public ObservableCollection<RoboCopyPreset> Presets { get; } = [];

    [ObservableProperty]
    private CopyTaskViewModel? _selectedTask;

    [ObservableProperty]
    private RoboCopyPreset? _selectedPreset;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private bool _isQueueRunning;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private string _overallProgressText = string.Empty;

    [ObservableProperty]
    private string _newPresetName = string.Empty;

    [ObservableProperty]
    private string _newPresetDescription = string.Empty;

    [ObservableProperty]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    private string _destinationPath = string.Empty;

    [ObservableProperty]
    private CopyTaskStatus _currentTaskStatus = CopyTaskStatus.Pending;

    [RelayCommand]
    private void AddTask()
    {
        var task = new CopyTask
        {
            Source = SourcePath,
            Destination = DestinationPath,
            Options = new RoboCopyOptions
            {
                Source = SourcePath,
                Destination = DestinationPath,
                CopySubdirectories = true,
                CopyEmptySubdirectories = true,
                RetryCount = 3,
                RetryWaitSeconds = 5
            }
        };

        var vm = new CopyTaskViewModel(task);
        Tasks.Add(vm);
        Log($"Task added: {task.Source} → {task.Destination}", LogLevel.Info, task.Id);
        SourcePath = string.Empty;
        DestinationPath = string.Empty;

        if (Settings.AutoSaveQueue)
            _ = SaveQueueSilentAsync();
    }

    [RelayCommand]
    private void RemoveTask(CopyTaskViewModel? task)
    {
        if (task is null) return;
        if (task.IsRunning)
        {
            Log("Cannot remove a running task. Cancel it first.", LogLevel.Warning);
            return;
        }
        Tasks.Remove(task);
        Log($"Task removed: {task.Source}", LogLevel.Info);

        if (Settings.AutoSaveQueue)
            _ = SaveQueueSilentAsync();
    }

    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Tasks.Where(t =>
            t.Status is CopyTaskStatus.Completed or
            CopyTaskStatus.Failed or
            CopyTaskStatus.Cancelled).ToList();

        foreach (var task in completed)
            Tasks.Remove(task);

        Log($"Cleared {completed.Count} completed task(s).", LogLevel.Info);

        if (Settings.AutoSaveQueue)
            _ = SaveQueueSilentAsync();
    }

    [RelayCommand]
    private void ClearAllTasks()
    {
        if (IsQueueRunning)
        {
            Log("Cannot clear tasks while queue is running.", LogLevel.Warning);
            return;
        }
        Tasks.Clear();
        Log("All tasks cleared.", LogLevel.Info);

        if (Settings.AutoSaveQueue)
            _ = SaveQueueSilentAsync();
    }

    [RelayCommand]
    private async Task StartQueueAsync()
    {
        if (_isProcessingQueue || Tasks.Count == 0) return;

        _isProcessingQueue = true;
        IsQueueRunning = true;
        _cts = new CancellationTokenSource();

        Log("Queue processing started.", LogLevel.Info);
        StatusText = "Processing queue...";

        try
        {
            var pendingTasks = Tasks.Where(t => t.IsPending).ToList();
            int completed = 0;

            foreach (var taskVm in pendingTasks)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    taskVm.Status = CopyTaskStatus.Cancelled;
                    taskVm.SyncToModel();
                    continue;
                }

                await ExecuteTaskAsync(taskVm, _cts.Token);
                completed++;
                OverallProgress = (double)completed / pendingTasks.Count * 100;
                OverallProgressText = $"{completed} / {pendingTasks.Count}";
            }

            StatusText = $"Queue completed. {completed} task(s) processed.";
            Log("Queue processing completed.", LogLevel.Success);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Queue cancelled.";
            Log("Queue processing was cancelled.", LogLevel.Warning);
        }
        finally
        {
            _isProcessingQueue = false;
            IsQueueRunning = false;
            _cts?.Dispose();
            _cts = null;

            if (Settings.AutoSaveQueue)
                _ = SaveQueueSilentAsync();
        }
    }

    [RelayCommand]
    private void CancelQueue()
    {
        _cts?.Cancel();
        StatusText = "Cancelling...";
        Log("Cancellation requested.", LogLevel.Warning);
    }

    [RelayCommand]
    private async Task ExecuteSingleTaskAsync(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null || taskVm.IsRunning) return;
        _cts = new CancellationTokenSource();
        await ExecuteTaskAsync(taskVm, _cts.Token);
        _cts?.Dispose();
        _cts = null;
    }

    [RelayCommand]
    private void CancelSingleTask(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null || !taskVm.CanCancel) return;
        _roboCopyService.Cancel();
        Log($"Cancellation sent for task: {taskVm.Source}", LogLevel.Warning);
    }

    private async Task ExecuteTaskAsync(CopyTaskViewModel taskVm, CancellationToken ct)
    {
        taskVm.Status = CopyTaskStatus.Running;
        taskVm.ProgressPercent = 0;
        taskVm.ErrorMessage = string.Empty;
        taskVm.SyncToModel();
        IsExecuting = true;
        CurrentTaskStatus = CopyTaskStatus.Running;
        SelectedTask = taskVm;

        var task = taskVm.Model;
        task.StartTime = DateTime.Now;
        task.Options.Source = task.Source;
        task.Options.Destination = task.Destination;

        Log($"Starting: {task.Source} → {task.Destination}", LogLevel.Info, task.Id);
        if (!string.IsNullOrEmpty(taskVm.PresetName))
            Log($"Using preset: {taskVm.PresetName}", LogLevel.Info, task.Id);

        if (task.Options.DryRun)
            Log("DRY RUN mode - no files will be copied.", LogLevel.Warning, task.Id);

        var progress = new Progress<RoboCopyProgress>(p =>
        {
            taskVm.ProgressPercent = p.Percent;
            taskVm.CurrentFile = p.CurrentFile;
            taskVm.FilesCopied = p.FilesCopied;
            taskVm.SyncToModel();

            if (!string.IsNullOrWhiteSpace(p.CurrentFile))
                StatusText = $"[{p.Percent:F1}%] {p.CurrentFile}";
        });

        try
        {
            var result = await _roboCopyService.ExecuteAsync(task.Options, progress, ct);

            task.Result = result;
            task.EndTime = DateTime.Now;

            if (result.Success)
            {
                taskVm.Status = CopyTaskStatus.Completed;
                CurrentTaskStatus = CopyTaskStatus.Completed;
                taskVm.ProgressPercent = 100;
                Log($"Completed: {task.Source} ({result.Message})", LogLevel.Success, task.Id);
                Log($"  Duration: {result.Duration:hh\\:mm\\:ss}, Files: {result.TotalFiles}", LogLevel.Info, task.Id);
            }
            else
            {
                taskVm.Status = CopyTaskStatus.Failed;
                CurrentTaskStatus = CopyTaskStatus.Failed;
                taskVm.ErrorMessage = result.Message;
                Log($"Failed: {task.Source} - {result.Message}", LogLevel.Error, task.Id);
            }
        }
        catch (OperationCanceledException)
        {
            taskVm.Status = CopyTaskStatus.Cancelled;
            CurrentTaskStatus = CopyTaskStatus.Cancelled;
            task.EndTime = DateTime.Now;
            Log($"Cancelled: {task.Source}", LogLevel.Warning, task.Id);
        }
        catch (Exception ex)
        {
            taskVm.Status = CopyTaskStatus.Failed;
            CurrentTaskStatus = CopyTaskStatus.Failed;
            taskVm.ErrorMessage = ex.Message;
            task.EndTime = DateTime.Now;
            Log($"Error: {task.Source} - {ex.Message}", LogLevel.Error, task.Id);
        }
        finally
        {
            taskVm.SyncToModel();
            IsExecuting = false;
        }
    }

    [RelayCommand]
    private async Task SavePresetAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPresetName))
        {
            Log("Preset name cannot be empty.", LogLevel.Warning);
            return;
        }

        var options = SelectedTask?.Model.Options ?? new RoboCopyOptions
        {
            CopySubdirectories = true,
            CopyEmptySubdirectories = true,
            RetryCount = 3,
            RetryWaitSeconds = 5
        };

        var preset = new RoboCopyPreset
        {
            Name = NewPresetName.Trim(),
            Description = NewPresetDescription.Trim(),
            Options = options
        };

        await _presetManager.SaveAsync(preset);
        await LoadPresetsAsync();

        NewPresetName = string.Empty;
        NewPresetDescription = string.Empty;

        Log($"Preset saved: {preset.Name}", LogLevel.Success);
    }

    [RelayCommand]
    private async Task DeletePresetAsync(RoboCopyPreset? preset)
    {
        if (preset is null) return;
        await _presetManager.DeleteAsync(preset.Name);
        await LoadPresetsAsync();
        Log($"Preset deleted: {preset.Name}", LogLevel.Info);
    }

    [RelayCommand]
    private void ApplyPreset(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null || SelectedPreset is null) return;
        taskVm.ApplyPreset(SelectedPreset);
        Log($"Applied preset '{SelectedPreset.Name}' to task.", LogLevel.Info);
    }

    [RelayCommand]
    private async Task DryRunTaskAsync(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null || taskVm.IsRunning) return;

        taskVm.Model.Options.DryRun = true;
        _cts = new CancellationTokenSource();
        await ExecuteTaskAsync(taskVm, _cts.Token);
        taskVm.Model.Options.DryRun = false;
        _cts?.Dispose();
        _cts = null;
    }

    [RelayCommand]
    private void MoveTaskUp(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null) return;
        var idx = Tasks.IndexOf(taskVm);
        if (idx > 0) Tasks.Move(idx, idx - 1);
    }

    [RelayCommand]
    private void MoveTaskDown(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null) return;
        var idx = Tasks.IndexOf(taskVm);
        if (idx < Tasks.Count - 1) Tasks.Move(idx, idx + 1);
    }

    [RelayCommand]
    private void DuplicateTask(CopyTaskViewModel? taskVm)
    {
        if (taskVm is null) return;
        taskVm.SyncToModel();
        var clone = taskVm.Model.Clone();
        var vm = new CopyTaskViewModel(clone);
        Tasks.Add(vm);
        Log($"Duplicated task: {taskVm.Source}", LogLevel.Info);
    }

    [RelayCommand]
    private async Task SaveQueueAsync()
    {
        var tasks = Tasks.Select(t => { t.SyncToModel(); return t.Model; });
        await _queuePersistence.SaveAsync(tasks);
        Log("Queue saved.", LogLevel.Success);
    }

    [RelayCommand]
    private async Task LoadQueueAsync()
    {
        var tasks = await _queuePersistence.LoadAsync();
        Tasks.Clear();
        foreach (var task in tasks)
            Tasks.Add(new CopyTaskViewModel(task));

        if (tasks.Count > 0)
            Log($"Loaded {tasks.Count} task(s) from queue.", LogLevel.Info);
    }

    [RelayCommand]
    private async Task ExportQueueAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log("Export path is required.", LogLevel.Warning);
            return;
        }

        var tasks = Tasks.Select(t => { t.SyncToModel(); return t.Model; });
        await _queuePersistence.ExportAsync(tasks, filePath);
        Log($"Queue exported to: {filePath}", LogLevel.Success);
    }

    [RelayCommand]
    private async Task ImportQueueAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Log("Import file not found.", LogLevel.Warning);
            return;
        }

        var tasks = await _queuePersistence.ImportAsync(filePath);
        foreach (var task in tasks)
            Tasks.Add(new CopyTaskViewModel(task));

        Log($"Imported {tasks.Count} task(s).", LogLevel.Success);

        if (Settings.AutoSaveQueue)
            _ = SaveQueueSilentAsync();
    }

    [RelayCommand]
    private async Task ExportBatAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log("Export path is required.", LogLevel.Warning);
            return;
        }

        var tasks = Tasks.Select(t => { t.SyncToModel(); return t.Model; });
        await _batchExporter.ExportBatAsync(tasks, filePath);
        Log($"Batch script exported to: {filePath}", LogLevel.Success);
    }

    [RelayCommand]
    private async Task ExportPowerShellAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log("Export path is required.", LogLevel.Warning);
            return;
        }

        var tasks = Tasks.Select(t => { t.SyncToModel(); return t.Model; });
        await _batchExporter.ExportPowerShellAsync(tasks, filePath);
        Log($"PowerShell script exported to: {filePath}", LogLevel.Success);
    }

    [RelayCommand]
    private async Task ExportPresetsAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log("Export path is required.", LogLevel.Warning);
            return;
        }

        await _presetManager.ExportAsync(Presets, filePath);
        Log($"Presets exported to: {filePath}", LogLevel.Success);
    }

    [RelayCommand]
    private async Task ImportPresetsAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Log("Import file not found.", LogLevel.Warning);
            return;
        }

        var imported = await _presetManager.ImportAsync(filePath);
        await LoadPresetsAsync();
        Log($"Imported {imported.Count} preset(s).", LogLevel.Success);
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await _settingsService.SaveAsync();
        Log("Settings saved.", LogLevel.Success);
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        _settingsService.Reset();
        await _settingsService.SaveAsync();
        Log("Settings reset to defaults.", LogLevel.Info);
    }

    [RelayCommand]
    private void TrimLog()
    {
        var max = Settings.MaxLogEntries;
        while (LogEntries.Count > max)
            LogEntries.RemoveAt(0);
        Log($"Log trimmed to {max} entries.", LogLevel.Info);
    }

    private async Task SaveQueueSilentAsync()
    {
        try
        {
            var tasks = Tasks.Select(t => { t.SyncToModel(); return t.Model; });
            await _queuePersistence.SaveAsync(tasks);
        }
        catch
        {
            // Silently fail for auto-save
        }
    }

    private async Task LoadPresetsAsync()
    {
        var presets = await _presetManager.LoadAllAsync();
        Presets.Clear();
        foreach (var p in presets) Presets.Add(p);
    }

    private void Log(string message, LogLevel level, Guid? taskId = null)
    {
        LogEntries.Add(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            TaskId = taskId
        });

        if (LogEntries.Count > Settings.MaxLogEntries * 1.2)
            TrimLog();
    }
}
