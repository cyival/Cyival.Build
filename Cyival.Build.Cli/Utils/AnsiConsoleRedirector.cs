using System.Text;
using Spectre.Console;

namespace Cyival.Build.Cli.Utils;

public class AnsiConsoleRedirector(IAnsiConsole ansiConsole) : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        if (value is null) return;
        ansiConsole.Write(value);
    }

    public override void WriteLine(string? value)
    {
        if (value is null) return;
        ansiConsole.WriteLine(value);
    }
}
