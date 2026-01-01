using System.Text;
using Spectre.Console;

namespace Cyival.Build.Cli.Utils;

public class AnsiConsoleRedirector : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        if (value is null) return;
        AnsiConsole.Write(value);
    }

    public override void WriteLine(string? value)
    {
        if (value is null) return;
        AnsiConsole.WriteLine(value);
    }
}
