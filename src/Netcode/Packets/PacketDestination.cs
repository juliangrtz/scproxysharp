﻿namespace SupercellProxy.Netcode;

/// <summary>
/// Specifies if a packet is client- or serversided
/// </summary>
public enum PacketDestination
{
    FROM_CLIENT,
    FROM_SERVER
}