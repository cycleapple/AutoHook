using System.IO;
using System.Text.RegularExpressions;

namespace AutoHook.Utils;

/// <summary>Archived AH3/AH4 presets, independent of the live AH6 wiki.</summary>
internal static class BundledOceanPresets
{
    private static readonly Lazy<Dictionary<string, List<BasePresetConfig>>> bundled = new(Load);

    internal static Dictionary<string, List<BasePresetConfig>> Presets => bundled.Value;

    private static Dictionary<string, List<BasePresetConfig>> Load()
    {
        using var stream = typeof(BundledOceanPresets).Assembly.GetManifestResourceStream(
            "AutoHook.Resources.Presets.Ocean-Fishing.md")
            ?? throw new InvalidOperationException("Bundled ocean fishing presets are missing.");
        using var reader = new StreamReader(stream);
        var groups = new Dictionary<string, List<BasePresetConfig>>();
        var category = "Ocean Fishing";
        foreach (Match match in Regex.Matches(reader.ReadToEnd(), @"(?m)^# ([^\r\n]+)|```\s*(AH[\s\S]*?)\s*```"))
        {
            if (match.Groups[1].Success)
            {
                category = match.Groups[1].Value.Trim();
                continue;
            }

            var code = match.Groups[2].Value.Trim();
            if (code.StartsWith("AHFOLDER_"))
            {
                var folder = Configuration.ImportFolder(code)
                    ?? throw new InvalidOperationException("Invalid bundled ocean fishing folder.");
                groups.Add(folder.Folder.FolderName, [.. folder.Presets]);
            }
            else
            {
                var preset = Configuration.ImportPreset(code) as CustomPresetConfig
                    ?? throw new InvalidOperationException("Invalid bundled ocean fishing preset.");
                if (!groups.TryGetValue(category, out var list))
                    groups.Add(category, list = []);
                list.Add(preset);
            }
        }

        return groups;
    }
}
