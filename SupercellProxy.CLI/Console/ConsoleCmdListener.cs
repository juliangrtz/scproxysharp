using System.Threading;
using SupercellProxy.JSON;
using SupercellProxy.Logging;
using SupercellProxy.Netcode;

namespace SupercellProxy.Console;

internal class ConsoleCmdListener
{
    private Thread ListenerThread;
    public static string Command;

    /// <summary>
    /// Listener for console commands
    /// </summary>
    public ConsoleCmdListener()
    {
        ListenerThread = new Thread(() =>
        {
            while ((Command = ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(Command) && !string.IsNullOrWhiteSpace(Command))
                {
                    switch (Command.ToLower())
                    {
                        case "help":
                            Logger.LogConsole("jsonreload -> Reloads the JSON definitions");
                            Logger.LogConsole("clear -> Clears the entire console window");
                            Logger.LogConsole("restart -> Restarts the proxy");
                            Logger.LogConsole("shutdown -> Safely shuts the proxy down");
                            break;

                        case "jsonreload":
                            JSONPacketManager.LoadDefinitions();
                            break;

                        case "clear":
                            Clear();
                            break;

                        case "restart":
                            Program.Restart();
                            break;

                        case "shutdown":
                            Logger.LogConsole("Shutting down...");
                            Proxy.Stop();
                            Environment.Exit(0);
                            break;

                        default:
                            Logger.LogWarning("Unknown command! Type \"help\" to see a list of available console commands.");
                            break;
                    }
                }
                else
                    CursorTop -= 1;
            }
        });
        ListenerThread.Start();
    }
}