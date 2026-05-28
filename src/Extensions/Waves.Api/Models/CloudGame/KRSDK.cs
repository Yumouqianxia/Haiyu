using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Waves.Api.Models.CloudGame
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class LoginConfig
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("sdk_openid")]
        public int SdkOpenid { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("cuid")]
        public string Cuid { get; set; }

        [JsonPropertyName("sdkuserid")]
        public string Sdkuserid { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("loginType")]
        public int LoginType { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("idStat")]
        public int IdStat { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("showPaw")]
        public bool ShowPaw { get; set; }

        [JsonPropertyName("bindDevStat")]
        public int BindDevStat { get; set; }

        [JsonPropertyName("autoToken")]
        public string AutoToken { get; set; }

        [JsonPropertyName("autoTokenStatus")]
        public bool AutoTokenStatus { get; set; }

        [JsonPropertyName("firstLgn")]
        public int FirstLgn { get; set; }

        [JsonPropertyName("phoneCheck")]
        public int PhoneCheck { get; set; }

        [JsonPropertyName("phoneToken")]
        public string PhoneToken { get; set; }
    }

    public class RoleData
    {
    }

    public class KRSDK
    {
        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("gameId")]
        public string GameId { get; set; }

        [JsonPropertyName("pkgId")]
        public string PkgId { get; set; }

        [JsonPropertyName("pkg")]
        public string Pkg { get; set; }

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("viewType")]
        public string ViewType { get; set; }

        [JsonPropertyName("loginConfig")]
        public LoginConfig LoginConfig { get; set; }

        [JsonPropertyName("roleData")]
        public RoleData RoleData { get; set; }
    }


}
