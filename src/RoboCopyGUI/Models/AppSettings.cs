namespace RoboCopyGUI.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Default";
    public bool AutoSaveQueue { get; set; } = true;
    public bool ConfirmBeforeDelete { get; set; } = true;
    public bool AutoStartQueueOnLaunch { get; set; } = false;
    public double MaxLogEntries { get; set; } = 5000;
    public bool ShowNotifications { get; set; } = true;
    public string DefaultSourcePath { get; set; } = string.Empty;
    public string DefaultDestinationPath { get; set; } = string.Empty;
    public RoboCopyOptions DefaultOptions { get; set; } = new()
    {
        CopySubdirectories = true,
        CopyEmptySubdirectories = true,
        RetryCount = 3,
        RetryWaitSeconds = 5
    };
    public WindowPlacement? LastWindowPlacement { get; set; }
}
