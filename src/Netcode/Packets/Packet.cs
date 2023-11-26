using SupercellProxy.Configuration;
using SupercellProxy.Crypto.Piranha;
using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SupercellProxy.Netcode;

internal class Packet
{
    private readonly int packetID;
    private readonly int payloadLen;
    private readonly int messageVer;
    private readonly string packetType;

    private readonly byte[] rawPacket;
    private readonly byte[] payload;
    private readonly byte[] encryptedPayload;
    private readonly byte[] decryptedPayload;

    private readonly PacketDestination destination;

    // Constructor
    public Packet(byte[] buf, PacketDestination d)
    {
        using (var PacketReader = new BinaryReader(new MemoryStream(buf)))
        {
            this.rawPacket = buf;
            this.destination = d;
            this.packetID = PacketReader.ReadUShortWithEndian();
            var tmp = PacketReader.ReadBytes(3);
            this.payloadLen = ((0x00 << 24) | (tmp[0] << 16) | (tmp[1] << 8) | tmp[2]);
            this.messageVer = PacketReader.ReadUShortWithEndian();
            this.payload = PacketReader.ReadBytes(this.payloadLen);
            this.packetType = PacketType.GetPacketType(this.packetID);
        }

        // En/Decrypt payload
        this.decryptedPayload = EnDecrypt.DecryptPacket(this);
        this.encryptedPayload = EnDecrypt.EncryptPacket(this);
    }

    /// <summary>
    /// Exports the packet to a file
    /// </summary>
    public void Export()
    {
        try
        {
            File.WriteAllBytes(@"RawPackets\\" + Config.SupercellGame + "_" + ID + "_" +
                               string.Format("{0:dd-MM_hh-mm-ss}", DateTime.Now) + ".dmp", DecryptedPayload);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to export packet " + ID + " (" + ex.GetType() + ")!", LogType.EXCEPTION);
        }
    }

    /// <summary>
    /// Raw, encrypted packet (header included)
    /// 7 byte header + n byte payload
    /// </summary>
    public byte[] Raw
    {
        get
        {
            return this.rawPacket;
        }
    }

    /// <summary>
    /// Raw, re-encrypted packet (header included)
    /// 7 byte header + n byte payload
    /// Reverse() because of little endian byte order
    /// </summary>
    public byte[] Rebuilt
    {
        get
        {
            List<Byte> builtPacket = new();
            builtPacket.AddRange(BitConverter.GetBytes(this.packetID).Reverse().Skip(2));
            builtPacket.AddRange(BitConverter.GetBytes(this.encryptedPayload.Length).Reverse().Skip(1));
            builtPacket.AddRange(BitConverter.GetBytes(this.messageVer).Reverse().Skip(2));
            builtPacket.AddRange(this.encryptedPayload);

            return builtPacket.ToArray();
        }
    }

    /// <summary>
    /// Self-explaining.
    /// 10100, 20100, 10101, 20104 [...]
    /// </summary>
    public int ID
    {
        get
        {
            return this.packetID;
        }
    }

    /// <summary>
    /// 2 bytes nobody has exact info about.
    /// </summary>
    public int MessageVersion
    {
        get
        {
            return this.messageVer;
        }
    }

    /// <summary>
    /// Destination. Either client or server.
    /// Admittedly, the Substring method is pretty nasty.
    /// </summary>
    public PacketDestination Destination
    {
        get
        {
            return this.destination;
        }
    }

    /// <summary>
    /// Normal payload from the received packet.
    /// </summary>
    public byte[] Payload
    {
        get
        {
            return this.payload;
        }
    }

    /// <summary>
    /// Encrypted payload by <seealso cref="EnDecrypt.EncryptPacket(Packet)"/>
    /// </summary>
    public byte[] EncryptedPayload
    {
        get
        {
            return this.encryptedPayload;
        }
    }

    /// <summary>
    /// Decrypted payload by <seealso cref="EnDecrypt.DecryptPacket(Packet)"/>
    /// </summary>
    public byte[] DecryptedPayload
    {
        get
        {
            return this.decryptedPayload;
        }
    }

    /// <summary>
    /// Returns a type as string (i.e. "LoginFailed")
    /// </summary>
    public string Type
    {
        get
        {
            return this.packetType;
        }
    }

    /// <summary>
    /// Returns packet info; Used for debugging
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("Destination: " + Destination);
        sb.AppendLine("ID: " + ID);
        sb.AppendLine("PayloadLen: " + DecryptedPayload.Length);
        sb.AppendLine("Payload: " + Encoding.UTF8.GetString(DecryptedPayload));
        return sb.ToString();
    }
}