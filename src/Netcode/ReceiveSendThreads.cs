using SupercellProxy.JSON;
using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Net.Sockets;
using System.Threading;

namespace SupercellProxy.Netcode;

internal class ReceiveSendThreads
{
    // Constants
    private const int HeaderSizeInBytes = 7;

    // Threads
    private Thread ClientThread, ServerThread;

    // Sockets
    private readonly Socket ClientSocket;

    private readonly Socket ServerSocket;

    // PacketHeaders
    private readonly byte[] ClientHeader = new byte[HeaderSizeInBytes];

    private readonly byte[] ServerHeader = new byte[HeaderSizeInBytes];

    // PacketBufs
    private byte[] ClientBuf, ServerBuf;

    public static bool Running = false;

    /// <summary>
    /// ReceiveSendThread constructor
    /// </summary>
    public ReceiveSendThreads(Socket ClientSocket, Socket ServerSocket)
    {
        this.ClientSocket = ClientSocket;
        this.ServerSocket = ServerSocket;
    }

    /// <summary>
    /// Starts both threads
    /// </summary>
    public void Run()
    {
        ClientThread = new Thread(() =>
        {
            while (ClientSocket.Receive(ClientHeader, 0, ClientHeader.Length, SocketFlags.None) != 0)
            {
                // Parse packet length
                var encodedLen = ClientHeader.Skip(2).Take(3).ToArray();
                var length = ((0x00 << 24) | (encodedLen[0] << 16) | (encodedLen[1] << 8) | encodedLen[2]);

                // Initialize Client buffer
                ClientBuf = new byte[length + HeaderSizeInBytes];

                // Apply header
                for (int i = 0; i < HeaderSizeInBytes; i++)
                    ClientBuf[i] = ClientHeader[i];

                // Fill client buffer
                ClientSocket.Receive(ClientBuf, HeaderSizeInBytes, length, SocketFlags.None);

                // Parse and export packet
                Packet ClientPacket = new(ClientBuf, PacketDestination.FROM_CLIENT);
                ClientPacket.Export();

                // Log Packet
                Logger.Log($"[C->S] {ClientPacket.ID}", LogType.PACKET);
                Logger.Log(ClientPacket.DecryptedPayload.ToHexString());

                JSONPacketManager.HandlePacket(ClientPacket);

                // Resend
                ServerSocket.Send(ClientPacket.Rebuilt);
            }
        });

        ServerThread = new Thread(() =>
        {
            while (ServerSocket.Receive(ServerHeader, 0, ServerHeader.Length, SocketFlags.None) != 0)
            {
                // Parse Packet Length
                var tmp = ServerHeader.Skip(2).Take(3).ToArray();
                var PacketLength = ((0x00 << 24) | (tmp[0] << 16) | (tmp[1] << 8) | tmp[2]);

                // Initialize Server Buffer
                ServerBuf = new byte[PacketLength + HeaderSizeInBytes];

                // Apply header
                for (int i = 0; i < HeaderSizeInBytes; i++)
                    ServerBuf[i] = ServerHeader[i];

                // Fill Server Buffer
                ServerSocket.Receive(ServerBuf, HeaderSizeInBytes, PacketLength, SocketFlags.None);

                // Parse & Export Packet
                Packet ServerPacket = new(ServerBuf, PacketDestination.FROM_SERVER);
                ServerPacket.Export();

                // Log Packet
                Logger.Log($"[S->C] {ServerPacket.ID}", LogType.PACKET);

                JSONPacketManager.HandlePacket(ServerPacket);

                // Resend
                ClientSocket.Send(ServerPacket.Rebuilt);
            }
        });

        ClientThread.Start();
        ServerThread.Start();
        Running = true;
    }
}