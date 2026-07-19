namespace RoboCopyGUI.Models;

public sealed class RoboCopyPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public RoboCopyOptions Options { get; set; } = new();
}
