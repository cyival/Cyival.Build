using System.Diagnostics;
using System.Text.Json;
using Cyival.Build.Plugin.Default.Environment;
using Microsoft.Extensions.Logging;
using Cyival.Build.Build;

namespace Cyival.Build.Plugin.Default;

public class GodotConfigConverter
{
    public const string ParsingScript =
        """
        extends MainLoop

        func _initialize():
            var path = OS.get_cmdline_user_args()[0]

            print(_get_cfg_as_json_string(path))

        func _process(delta: float) -> bool:
            return true

        func _get_cfg_as_json_string(path: String) -> String:
            var config = ConfigFile.new()
            var err = config.load(path)

            if err != OK:
                push_error("Failed to load config file: %s" % path)
                return ""

            var dict = {}
            for section in config.get_sections():
                dict[section] = {}
                for key in config.get_section_keys(section):
                    dict[section][key] = config.get_value(section, key)
            return JSON.stringify(dict)
        """;

    public static Dictionary<string, object> ConvertByGodotInstance(BuildSettings buildSettings, GodotInstance instance, string path)
    {
        if (instance.Version.Major != 4)
            throw new NotSupportedException("Only Godot 4 is supported.");

        var tempScriptPath = buildSettings.OutTempPathSolver.GetPathTo("convert_godot_cfg.gd");

        File.WriteAllText(tempScriptPath, ParsingScript);

        var startInfo = new ProcessStartInfo(instance.Path, ["--headless", "--no-header", "-s", tempScriptPath, "--", path])
        {
            RedirectStandardOutput = true,
        };

        var proc = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start godot process for parsing config.");

        var json = "";

        // FIXME: Unexpectedly reads output that not belongs to the process.
        // e.g. `Initialize godot-rust (API v4.5.stable.official, runtime v4.5.1.stable.mono.official, safeguards strict)`
        while (!proc.StandardOutput.EndOfStream)
        {
            var line = proc.StandardOutput.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || !line.TrimStart().StartsWith("{")) continue;

            json += line;
        }

        var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
            ?? throw new NullReferenceException("Failed to parse config json.");
        return parsed;
    }
}
