using System.Text.Json;
using System.Text.Json.Serialization;
using ChromeCDPSharp.Models;

namespace ChromeCDPSharp.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    UseStringEnumConverter = false,
    WriteIndented = false)]
[JsonSerializable(typeof(CdpIncomingMessage))]
[JsonSerializable(typeof(CdpEventEnvelope))]
[JsonSerializable(typeof(CdpErrorObject))]
[JsonSerializable(typeof(CdpCommandResponse<JsonElement>))]
[JsonSerializable(typeof(CdpCommandResponse<EmptyResult>))]
[JsonSerializable(typeof(CdpCommandResponse<GetResponseBodyResult>))]
[JsonSerializable(typeof(CdpCommandEnvelope<NetworkEnableParams>))]
[JsonSerializable(typeof(CdpCommandEnvelope<GetResponseBodyParams>))]
[JsonSerializable(typeof(NetworkEnableParams))]
[JsonSerializable(typeof(GetResponseBodyParams))]
[JsonSerializable(typeof(GetResponseBodyResult))]
[JsonSerializable(typeof(ResponseReceivedEvent))]
[JsonSerializable(typeof(LoadingFinishedEvent))]
[JsonSerializable(typeof(NetworkResponsePayload))]
[JsonSerializable(typeof(DevToolsTargetInfo))]
[JsonSerializable(typeof(List<DevToolsTargetInfo>))]
[JsonSerializable(typeof(EmptyResult))]
public partial class CdpJsonContext : JsonSerializerContext
{
}
