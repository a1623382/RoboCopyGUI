using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.ViewModels;

public sealed partial class CopyTaskViewModel : ObservableObject
{
    private readonly CopyTask _task;

    public CopyTaskViewModel(CopyTask task)
    {
        _task = task;
        _source = task.Source;
        _destination = task.Destination;
        _status = task.Status;
        _progressPercent = task.ProgressPercent;
        _currentFile = task.CurrentFile;
        _filesCopied = task.FilesCopied;
        _errorMessage = task.ErrorMessage;
        _presetName = task.PresetName;
    }

    public Guid Id => _task.Id;
    public CopyTask Model => _task;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    private string _source;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    private string _destination;

    [ObservableProperty]
    private CopyTaskStatus _status;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _currentFile;

    [ObservableProperty]
    private long _filesCopied;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private string _presetName;

    [ObservableProperty]
    private bool _isSelected;

    public bool IsRunning => Status == CopyTaskStatus.Running;
    public bool IsPending => Status == CopyTaskStatus.Pending;
    public bool CanCancel => Status == CopyTaskStatus.Running;

    partial void OnStatusChanged(CopyTaskStatus value)
    {
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(CanCancel));
    }

    public void SyncFromModel()
    {
        Source = _task.Source;
        Destination = _task.Destination;
        Status = _task.Status;
        ProgressPercent = _task.ProgressPercent;
        CurrentFile = _task.CurrentFile;
        FilesCopied = _task.FilesCopied;
        ErrorMessage = _task.ErrorMessage;
        PresetName = _task.PresetName;
    }

    public void SyncToModel()
    {
        _task.Source = Source;
        _task.Destination = Destination;
        _task.Status = Status;
        _task.ProgressPercent = ProgressPercent;
        _task.CurrentFile = CurrentFile;
        _task.FilesCopied = FilesCopied;
        _task.ErrorMessage = ErrorMessage;
        _task.PresetName = PresetName;
    }

    public void ApplyPreset(RoboCopyPreset preset)
    {
        PresetName = preset.Name;
        _task.Options = CloneOptionsFromPreset(preset.Options);
    }

    private static RoboCopyOptions CloneOptionsFromPreset(RoboCopyOptions src) => new()
    {
        Mirror = src.Mirror,
        MoveFiles = src.MoveFiles,
        CopySubdirectories = src.CopySubdirectories,
        CopyEmptySubdirectories = src.CopyEmptySubdirectories,
        RestartMode = src.RestartMode,
        BackupMode = src.BackupMode,
        UnbufferedIO = src.UnbufferedIO,
        FixFileSecurity = src.FixFileSecurity,
        FixFileTimes = src.FixFileTimes,
        PurgeDestination = src.PurgeDestination,
        ExcludeJunctions = src.ExcludeJunctions,
        RetryCount = src.RetryCount,
        RetryWaitSeconds = src.RetryWaitSeconds,
        MultiThreadCount = src.MultiThreadCount,
        DryRun = src.DryRun,
        IncludePatterns = (string[])src.IncludePatterns.Clone(),
        ExcludePatterns = (string[])src.ExcludePatterns.Clone(),
        ExcludeDirectories = (string[])src.ExcludeDirectories.Clone(),
        MaxDepth = src.MaxDepth,
        MinFileSize = src.MinFileSize,
        MaxFileSize = src.MaxFileSize,
        VerboseOutput = src.VerboseOutput,
        NoProgress = src.NoProgress,
        ShowEstimatedTimeOfArrival = src.ShowEstimatedTimeOfArrival
    };
}
