using Microsoft.Extensions.Configuration;
using SupercellProxy.Logging;

namespace SupercellProxy.Configuration;

internal static class Config
{
    private static readonly IConfiguration configuration;

    static Config()
    {
        configuration = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public static bool JsonLogging
    {
        get
        {
            return bool.Parse(configuration["JsonLogging"]);
        }
    }

    /// <summary>
    /// Returns the host name in the following format:
    /// game(a).<supercell_game>.com
    /// </summary>
    public static string Host
    {
        get
        {
            return configuration["Host"].ToLower();
        }
    }

    /// <summary>
    /// Returns the Supercell game you want to intercept data from.
    /// </summary>
    public static SupercellGame SupercellGame
    {
        get
        {
            switch (Host.Replace("gamea", "game"))
            {
                case "game.clashofclans.com":
                    return SupercellGame.CLASH_OF_CLANS;

                case "game.clashroyaleapp.com":
                    return SupercellGame.CLASH_ROYALE;

                case "game.boombeachgame.com":
                    return SupercellGame.BOOM_BEACH;

                case "brawlstars-hostname": // Todo: Find out host
                    return SupercellGame.BRAWL_STARS;

                case "hayday-hostname": // Todo: Find out host
                    return SupercellGame.HAY_DAY;

                default:
                    Logger.Log("You configured an unknown host (" + Host + ")!", LogType.WARNING);
                    Logger.Log("Please check your config file.", LogType.WARNING);
                    ReadKey();
                    return SupercellGame.CLASH_OF_CLANS; // Todo: Throw exception instead
            }
        }
    }
}