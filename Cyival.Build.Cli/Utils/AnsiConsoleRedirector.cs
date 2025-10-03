using Spectre.Console;

namespace Cyival.Build.Cli.Utils;

public class AnsiConsoleRedirector : IConsoleRedirector
{
    public void Write(params string[] content)
    {
        AnsiConsole.Write(string.Join(string.Empty, content));
    }

    public void WriteLine(params string[] content)
    {
        Write([.. content, "\n"]);
    }
}