using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using Path = System.IO.Path;

namespace MakeSTMGreatAgain;

/// <summary>
/// Loads config.json once and shares it between the loaders. A missing file uses the defaults.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ModConfigLoader(ISptLogger<ModConfigLoader> logger, ModHelper modHelper)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private ModConfig? _config;

    public ModConfig Config => _config ??= Load();

    private ModConfig Load()
    {
        var path = Path.Combine(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config.json");
        if (!File.Exists(path))
        {
            return new ModConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<ModConfig>(File.ReadAllText(path), JsonOptions) ?? new ModConfig();
        }
        catch (Exception ex)
        {
            logger.Error($"[MakeSTMGreatAgain] Failed to load {path}, using defaults: {ex.Message}");
            return new ModConfig();
        }
    }
}

public class ModConfig
{
    /// <summary>
    /// Logs what the mod changed at startup. Errors are always logged.
    /// </summary>
    public bool Debug { get; set; }
}
