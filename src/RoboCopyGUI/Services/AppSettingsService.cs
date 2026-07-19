using System.Text.Json;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Services;

public sealed class AppSettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RoboCopyGUI");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Current { get; private set; } = new();

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(SettingsPath))
        {
            Current = new AppSettings();
            return Current;
        }

        try
        {
            var json = await File.ReadAllTextAsync(SettingsPath);
            Current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (JsonException)
        {
            Current = new AppSettings();
        }

        return Current;
    }

    public async Task SaveAsync()
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(Current, JsonOptions);
        await File.WriteAllTextAsync(SettingsPath, json);
    }

    public async Task UpdateAsync(Action<AppSettings> update)
    {
        update(Current);
        await SaveAsync();
    }

    public void Reset()
    {
        Current = new AppSettings();
    }
}
