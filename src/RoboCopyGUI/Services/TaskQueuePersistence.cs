using System.Text.Json;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Services;

public sealed class TaskQueuePersistence
{
    private static readonly string QueueDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RoboCopyGUI");

    private static readonly string QueuePath = Path.Combine(QueueDirectory, "queue.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveAsync(IEnumerable<CopyTask> tasks)
    {
        Directory.CreateDirectory(QueueDirectory);
        var taskList = tasks.ToList();
        var json = JsonSerializer.Serialize(taskList, JsonOptions);
        await File.WriteAllTextAsync(QueuePath, json);
    }

    public async Task<List<CopyTask>> LoadAsync()
    {
        if (!File.Exists(QueuePath))
            return [];

        try
        {
            var json = await File.ReadAllTextAsync(QueuePath);
            return JsonSerializer.Deserialize<List<CopyTask>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task ExportAsync(IEnumerable<CopyTask> tasks, string filePath)
    {
        var taskList = tasks.ToList();
        var json = JsonSerializer.Serialize(taskList, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<CopyTask>> ImportAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<CopyTask>>(json, JsonOptions) ?? [];
    }
}
