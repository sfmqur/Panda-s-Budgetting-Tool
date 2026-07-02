using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.Services;

public class ConfigService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PandaSoft", "PandaBudgetingTool", "config.json");

    public async Task<ConfigDto> LoadAsync()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return new ConfigDto();

            var json = await File.ReadAllTextAsync(ConfigPath);
            return JsonSerializer.Deserialize<ConfigDto>(json) ?? new ConfigDto();
        }
        catch
        {
            return new ConfigDto();
        }
    }

    public async Task SaveAsync(ConfigDto config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigPath, json);
        }
        catch
        {
            // best-effort save; ignore transient IO failures
        }
    }
}