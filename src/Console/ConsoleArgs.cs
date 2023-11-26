using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Collections.Generic;

namespace SupercellProxy.Console;

internal class ConsoleArgs
{
    private readonly List<string> _arguments;

    // Public vars
    public static bool Verbose = false;

    /// <summary>
    /// ConsoleArgs constructor
    /// </summary>
    public ConsoleArgs(string[] args)
    {
        _arguments = new List<string>(args);
    }

    /// <summary>
    /// Removes arg prefix
    /// </summary>
    private string RemovePrefix(string Command) => (Command.ElementAt(0) == '-') ? Command.Replace("-", "") : Command;

    /// <summary>
    /// Parses console arguments
    /// </summary>
    public void Parse()
    {
        try
        {
            if (_arguments.Count == 0)
                return;

            foreach (string argument in _arguments)
            {
                // arg=val format?
                if (argument.Contains('='))
                {
                    var splitArg = argument.Split('=')[0];
                    var splitVal = argument.Split('=')[1];

#if DEBUG
                    WriteLine("Arg = " + splitArg + " | Val = " + splitVal);
#endif
                }
                else
                {
                    switch (RemovePrefix(argument))
                    {
                        case "help":
                            Logger.CenterString("=> Argument usage <=");
                            Write(Environment.NewLine);
                            Logger.CenterString("-help -> Displays this.");
                            Logger.CenterString("-ver  -> Shows detailed version info");
                            break;

                        case "ver":
                            Logger.CenterString("=> Version <=");
                            Write(Environment.NewLine);
                            Logger.CenterString("SupercellProxy v" + Helper.AssemblyVersion);
                            Logger.CenterString("https://opensource.org/licenses/MIT/");
                            break;

                        default:
                            Logger.CenterString("Unknown argument: " + argument);
                            break;
                    }
                    Program.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to parse console arguments (" + ex.GetType() + ")!", LogType.EXCEPTION);
            Logger.Log(ex.Message);
        }
    }
}