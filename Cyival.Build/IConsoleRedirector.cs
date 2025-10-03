namespace Cyival.Build;

public interface IConsoleRedirector
{
    void Write(params string[] content);
    
    void WriteLine(params string[] content);
}