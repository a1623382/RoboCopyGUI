using System.Text.Json;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Services;

public sealed class PresetManager
{
    private static readonly string PresetsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RoboCopyGUI", "presets");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyList<RoboCopyPreset>> LoadAllAsync()
    {
        if (!Directory.Exists(PresetsDirectory))
            return [];

        var presets = new List<RoboCopyPreset>();
        foreach (var file in Directory.GetFiles(PresetsDirectory, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var preset = JsonSerializer.Deserialize<RoboCopyPreset>(json, JsonOptions);
                if (preset is not null)
                    presets.Add(preset);
            }
            catch (JsonException) { }
        }

        return presets.OrderBy(p => p.Name).ToList().AsReadOnly();
    }

    public async Task SaveAsync(RoboCopyPreset preset)
    {
        Directory.CreateDirectory(PresetsDirectory);
        var safeName = string.Join("_", preset.Name.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(PresetsDirectory, $"{safeName}.json");
        var json = JsonSerializer.Serialize(preset, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public Task DeleteAsync(string presetName)
    {
        var safeName = string.Join("_", presetName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(PresetsDirectory, $"{safeName}.json");
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RoboCopyPreset>> ImportAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var presets = JsonSerializer.Deserialize<List<RoboCopyPreset>>(json, JsonOptions) ?? [];
        foreach (var preset in presets)
            await SaveAsync(preset);
        return presets.AsReadOnly();
    }

    public async Task ExportAsync(IEnumerable<RoboCopyPreset> presets, string filePath)
    {
        var json = JsonSerializer.Serialize(presets.ToList(), JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
