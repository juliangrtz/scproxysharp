using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SupercellProxy.JSON;

/// <summary>
/// Field types
/// see https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm
/// </summary>
internal enum FieldType
{
    String,
    SupercellString,
    BigEndianInt,
    BigEndianInt64,
    BigEndianUInt,
    Int,
    Bytes,
    Int64,
    BigEndianUInt64,
    UInt,
    UInt64,
    VInt,
    VInt64,
    BigEndianShort,
    BigEndianUShort,
    Short,
    UShort
}

internal class JSONPacketField
{
    /// <summary>
    /// Fieldname
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Byte count
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string BytesToRead { get; set; }

    /// <summary>
    /// Field datatype
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public FieldType FieldType { get; set; }
}