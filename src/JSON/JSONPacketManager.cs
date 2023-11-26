using Newtonsoft.Json;
using SupercellProxy.Configuration;
using SupercellProxy.Logging;
using SupercellProxy.Netcode;
using SupercellProxy.Utils;
using System.Collections.Generic;
using System.IO;

namespace SupercellProxy.JSON;

internal class JSONPacketManager
{
    public static Dictionary<int, JSONPacketWrapper> JsonPackets = new();
    private static readonly List<ParsedField<object>> Fields = new();
    private static ParsedPacket pp;
    private static JSONPacketWrapper wrapper;

    /// <summary>
    /// Handles packet
    /// </summary>
    public static void HandlePacket(Packet packet)
    {
        // Known packet => Parse it
        if (JsonPackets.ContainsKey(packet.ID))
        {
            wrapper = JsonPackets[packet.ID];
            pp = JsonParseHelper.ParsePacket(wrapper, packet);
        }
        // Unknown packet => Save payload
        else
        {
            pp = new ParsedPacket
            {
                PacketID = packet.ID,
                PacketName = "Unknown",
                PayloadLength = packet.DecryptedPayload.Length,
                ParsedFields = Fields
            };

            // Payload
            pp.ParsedFields.Add(new ParsedField<object>
            {
                FieldLength = packet.DecryptedPayload.Length,
                FieldName = "Payload",
                FieldType = FieldType.String,
                FieldValue = packet.DecryptedPayload.ToHexString()
            });
        }

        // Check if the packet is known
        if (Config.JsonLogging && pp.PacketName != "Unknown")
        {
            foreach (var v in pp.ParsedFields)
                Logger.Log(v.FieldName + " : " + v.FieldValue, LogType.JSON);

            var path = @"JsonPackets\\" + Config.SupercellGame + "_" + pp.PacketID + "_" +
                       string.Format("{0:dd-MM_hh-mm-ss}", DateTime.Now) + ".json";

            using var file = File.CreateText(path);

            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Serialize(file, pp);
        }
    }

    /// <summary>
    /// Loads all packet definitions
    /// Todo: Update directory
    /// </summary>
    public static void LoadDefinitions()
    {
        // Loop
        var files = Directory.GetFiles($"JSON\\Definitions\\ClashofClans", "*.json", SearchOption.AllDirectories);
        foreach (var filePath in files)
        {
            // Open
            using var file = File.OpenText(filePath);

            // Deserialize
            var serializer = new JsonSerializer();
            var wrapper = (JSONPacketWrapper)serializer.Deserialize(file, typeof(JSONPacketWrapper));

            // Check existence
            if (!(JsonPackets.ContainsKey(wrapper.PacketID)))
                JsonPackets.Add(wrapper.PacketID, wrapper);
        }

        Logger.Log("Definitions loaded (" + JsonPackets.Count + " packets)", LogType.JSON);
    }
}