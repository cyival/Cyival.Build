namespace Cyival.Build;

public class EmptyConsoleRedirector : IConsoleRedirector
{
    public void Write(params string[] content)
    {
    }

    public void WriteLine(params string[] content)
    {
    }
}