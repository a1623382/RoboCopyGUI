using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Controls;

public sealed partial class RoboCopyOptionsPanel : UserControl
{
    public static readonly DependencyProperty TaskProperty =
        DependencyProperty.Register(nameof(Task), typeof(CopyTask), typeof(RoboCopyOptionsPanel),
            new PropertyMetadata(null, OnTaskChanged));

    public CopyTask? Task
    {
        get => (CopyTask?)GetValue(TaskProperty);
        set => SetValue(TaskProperty, value);
    }

    public RoboCopyOptionsPanel()
    {
        InitializeComponent();
    }

    private static void OnTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RoboCopyOptionsPanel panel && e.NewValue is CopyTask task)
            panel.LoadFromOptions(task.Options);
    }

    private void LoadFromOptions(RoboCopyOptions options)
    {
        ChkMirror.IsChecked = options.Mirror;
        ChkCopySubdirs.IsChecked = options.CopySubdirectories;
        ChkCopyEmpty.IsChecked = options.CopyEmptySubdirectories;
        ChkMoveFiles.IsChecked = options.MoveFiles;
        ChkPurge.IsChecked = options.PurgeDestination;
        ChkRestart.IsChecked = options.RestartMode;
        ChkBackup.IsChecked = options.BackupMode;
        ChkUnbuffered.IsChecked = options.UnbufferedIO;
        ChkEfsRaw.IsChecked = options.EfsRawMode;
        ChkFixSec.IsChecked = options.FixFileSecurity;
        ChkFixTimes.IsChecked = options.FixFileTimes;
        ChkFatTimes.IsChecked = options.FatFileTimes;
        ChkDisableDst.IsChecked = options.DisableDirectoryTimestamps;
        ChkExcludeJunctions.IsChecked = options.ExcludeJunctions;
        ChkVerbose.IsChecked = options.VerboseOutput;
        ChkTimestamps.IsChecked = options.IncludeSourceTimestamps;
        ChkNoProgress.IsChecked = options.NoProgress;
        ChkETA.IsChecked = options.ShowEstimatedTimeOfArrival;
        ChkUnicodeLog.IsChecked = options.ProduceUnicodeLog;
        ChkFullPaths.IsChecked = options.PrintFullPaths;
        NumRetryCount.Value = options.RetryCount;
        NumRetryWait.Value = options.RetryWaitSeconds;
        NumMultiThread.Value = options.MultiThreadCount;
        TxtIncludePatterns.Text = string.Join(",", options.IncludePatterns);
        TxtExcludePatterns.Text = string.Join(",", options.ExcludePatterns);
        TxtExcludeDirs.Text = string.Join(",", options.ExcludeDirectories);
        NumMaxDepth.Value = options.MaxDepth;
        NumMinSize.Value = options.MinFileSize;
        NumMaxSize.Value = options.MaxFileSize;
        ChkLogFile.IsChecked = options.LogFile;
        TxtLogPath.Text = options.LogPath;
        ChkDryRun.IsChecked = options.DryRun;
    }

    private void ApplyOptions_Click(object sender, RoutedEventArgs e)
    {
        if (Task is null) return;
        ApplyToOptions(Task.Options);
    }

    private void ApplyToOptions(RoboCopyOptions options)
    {
        options.Mirror = ChkMirror.IsChecked is true;
        options.CopySubdirectories = ChkCopySubdirs.IsChecked is true;
        options.CopyEmptySubdirectories = ChkCopyEmpty.IsChecked is true;
        options.MoveFiles = ChkMoveFiles.IsChecked is true;
        options.PurgeDestination = ChkPurge.IsChecked is true;
        options.RestartMode = ChkRestart.IsChecked is true;
        options.BackupMode = ChkBackup.IsChecked is true;
        options.UnbufferedIO = ChkUnbuffered.IsChecked is true;
        options.EfsRawMode = ChkEfsRaw.IsChecked is true;
        options.FixFileSecurity = ChkFixSec.IsChecked is true;
        options.FixFileTimes = ChkFixTimes.IsChecked is true;
        options.FatFileTimes = ChkFatTimes.IsChecked is true;
        options.DisableDirectoryTimestamps = ChkDisableDst.IsChecked is true;
        options.ExcludeJunctions = ChkExcludeJunctions.IsChecked is true;
        options.VerboseOutput = ChkVerbose.IsChecked is true;
        options.IncludeSourceTimestamps = ChkTimestamps.IsChecked is true;
        options.NoProgress = ChkNoProgress.IsChecked is true;
        options.ShowEstimatedTimeOfArrival = ChkETA.IsChecked is true;
        options.ProduceUnicodeLog = ChkUnicodeLog.IsChecked is true;
        options.PrintFullPaths = ChkFullPaths.IsChecked is true;
        options.RetryCount = (int)NumRetryCount.Value;
        options.RetryWaitSeconds = (int)NumRetryWait.Value;
        options.MultiThreadCount = (int)NumMultiThread.Value;
        options.IncludePatterns = SplitTrim(TxtIncludePatterns.Text);
        options.ExcludePatterns = SplitTrim(TxtExcludePatterns.Text);
        options.ExcludeDirectories = SplitTrim(TxtExcludeDirs.Text);
        options.MaxDepth = (int)NumMaxDepth.Value;
        options.MinFileSize = (long)NumMinSize.Value;
        options.MaxFileSize = (long)NumMaxSize.Value;
        options.LogFile = ChkLogFile.IsChecked is true;
        options.LogPath = TxtLogPath.Text.Trim();
        options.DryRun = ChkDryRun.IsChecked is true;
    }

    private static string[] SplitTrim(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
