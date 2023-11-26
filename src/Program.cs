using SupercellProxy.Console;
using SupercellProxy.Netcode;
using SupercellProxy.Utils;

namespace SupercellProxy;

internal class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    private static void Main(string[] args)
    {
        // Parse console args
        new ConsoleArgs(args).Parse(); // Todo: Use library instead!

        // Console title
        Title = $"SupercellProxy v{Helper.AssemblyVersion}";

        // Start Proxy
        Proxy.Start();

        // Start command listener
        _ = new ConsoleCmdListener();
    }

    /// <summary>
    /// Closes the program with an exit code of 0.
    /// </summary>
    public static void Close()
        => Environment.Exit(0);

    /// <summary>
    /// Restarts the program.
    /// </summary>
    public static void Restart()
    {
        // Start another process
        // Todo: Make it working for .NET 6
        //var fileName = Assembly.GetExecutingAssembly().Location;
        //System.Diagnostics.Process.Start("fileName");

        // Close the current process
        Environment.Exit(0);
    }
}