using SupercellProxy.Configuration;
using SupercellProxy.Crypto.Piranha;
using SupercellProxy.JSON;
using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SupercellProxy.Netcode;

internal class Proxy
{
    private static Thread AcceptThread;
    public static List<Client> ClientPool = new();
    public const int Backlog = 100;
    public const int Port = 9339;

    /// <summary>
    /// Starts the proxy.
    /// </summary>
    public static void Start()
    {
        try
        {
            // Check dirs
            Helper.CheckSubDirectories();

            // Splash screen
            Logger.CenterString($"SupercellProxy v{Helper.AssemblyVersion}");
            Logger.CenterString("Made with <3 by expl0itr");
            Logger.CenterString("Shoutout to @iGio90, @Ultrapowa, @nameless, @zzVertigo et al.");
            Logger.CenterString("Anti-shoutout to @BerkanYildiz");

            // Show configuration values
            WriteLine();

            Logger.Log($"Supercell game: {Config.SupercellGame.ReadableName()}");
            Logger.Log($"Host: {Config.Host}");
            Logger.Log($"Local IPv4 address: {Helper.LocalNetworkIPv4}");

            // Set the latest public key
            Keys.SetServerPublicKey();

            // Initialize the JSON definitions for the packets
            JSONPacketManager.LoadDefinitions();

            // Bind a new socket to the local endpoint
            var endPoint = new IPEndPoint(IPAddress.Any, Port);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            socket.Listen(Backlog);

            // Listen for incoming connections
            AcceptThread = new Thread(() =>
            {
                while (true)
                {
                    var clientSocket = socket.Accept();
                    var client = new Client(clientSocket);
                    ClientPool.Add(client);
                    Logger.Log($"A new client connected ({clientSocket.GetIP()}). Enqueuing it...");
                    client.Enqueue();
                }
            });
            AcceptThread.Start();

            Logger.Log("Proxy started. Waiting for incoming connections..");
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to start the proxy (" + ex.GetType() + ")!");
            Logger.Log(ex.Message);
        }
    }

    /// <summary>
    /// Stops the proxy in a healthy way.
    /// </summary>
    public static void Stop()
    {
        for (int i = 0; i < ClientPool.Count; i++)
            ClientPool[i].Dequeue();

        ClientPool.Clear();
    }
}