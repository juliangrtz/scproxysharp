using SupercellProxy.Configuration;
using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Net;
using System.Net.Sockets;

namespace SupercellProxy.Netcode;

internal class Client
{
    public Socket ClientSocket, ServerSocket;
    private readonly string Host = Config.Host;

    /// <summary>
    /// Client constructor
    /// </summary>
    public Client(Socket s)
    {
        ClientSocket = s;
    }

    /// <summary>
    /// Enqueues the client
    /// </summary>
    public void Enqueue()
    {
        try
        {
            // Connect to the official supercell server
            Logger.Log("Connecting to " + Host + "...");
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEndPoint = new(ipAddress, 9339);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Connect(remoteEndPoint);

            // Start async recv/send procedure
            Logger.Log("Starting threads..");
            Write(Environment.NewLine);
            new ReceiveSendThreads(ClientSocket, ServerSocket).Run();
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to enqueue client " + ClientSocket.GetIP() + "!");
            Logger.Log(ex.ToString());
        }
    }

    /// <summary>
    /// Dequeues the client
    /// </summary>
    public void Dequeue()
    {
        ClientSocket.Disconnect(false);
        ServerSocket.Disconnect(false);
        ClientSocket = null;
        ServerSocket = null;
    }
}