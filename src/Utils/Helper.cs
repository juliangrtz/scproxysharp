using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace SupercellProxy.Utils;

internal class Helper
{
    /// <summary>
    /// Creates required subdirectories if they don't exist yet.
    /// </summary>
    public static void CheckSubDirectories()
    {
        if (!Directory.Exists("JsonDefinitions"))
            Directory.CreateDirectory("JsonDefinitions");

        if (!Directory.Exists("JsonPackets"))
            Directory.CreateDirectory("JsonPackets");

        if (!Directory.Exists("RawPackets"))
            Directory.CreateDirectory("RawPackets");

        if (!Directory.Exists("Logs"))
            Directory.CreateDirectory("Logs");
    }

    /// <summary>
    /// Returns the local network IP in a proper format
    /// </summary>
    public static IPAddress LocalNetworkIPv4
    {
        get
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socket.Connect("10.0.2.4", 65530);
            return ((IPEndPoint)socket.LocalEndPoint).Address;
        }
    }

    public static string AssemblyVersion
    {
        get
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString()[..5];
        }
    }

    public static string CurrentTime
    {
        get
        {
            return DateTime.Now.ToString("h:mm:ss");
        }
    }
}