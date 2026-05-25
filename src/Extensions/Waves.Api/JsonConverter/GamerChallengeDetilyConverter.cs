using System.Text.Json;
using System.Text.Json.Serialization;
using Waves.Api.Models;
using Waves.Api.Models.Communitys.DataCenter;

namespace Waves.Api.JsonConverter;

public partial class GamerChallengeDetilyConverter : JsonConverter<ChallengeInfo>
{
    public override ChallengeInfo? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        ChallengeInfo info = new ChallengeInfo() { Detilys = new() };
        var jsonObj = JsonElement.ParseValue(ref reader);

        foreach (var item in jsonObj.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item2 in item.Value.EnumerateArray())
                {
                    var detily = item2.Deserialize<Detilys>(CommunityContext.Default.Detilys);
                    if (detily != null)
                    {
                        detily.BossId = item.Name;
                        info.Detilys.Add(detily);
                    }
                }
            }
        }
        return info;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ChallengeInfo value,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException();
    }
}
