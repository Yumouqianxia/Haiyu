using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Waves.Api.Models.Record
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
    public class WeaponProp
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("aname")]
        public string Aname { get; set; }

        [JsonPropertyName("IsRatio")]
        public bool IsRatio { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public class CommunityWeaponData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("star")]
        public int Star { get; set; }

        [JsonPropertyName("prop")]
        public WeaponProp Prop { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("typeIcon")]
        public string TypeIcon { get; set; }

        [JsonPropertyName("propDes")]
        public string PropDes { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
