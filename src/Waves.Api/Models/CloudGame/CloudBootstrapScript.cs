using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Waves.Api.Models.CloudGame;

namespace Haiyu.Models
{
    public class CloudBootstrapScript
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("storageItems")]
        public IReadOnlyDictionary<string, string> StorageItems { get; set; }
    }

    public class CloudGameAppStore
    {
        [JsonPropertyName("sdkLoginInfo")]
        public CloudSdkLoginInfo SdkLoginInfo { get; set; }

        [JsonPropertyName("appLoginInfo")]
        public CloudAppLoginInfo AppLoginInfo { get; set; }
    }

    public class CloudSdkLoginInfo
    {
        [JsonPropertyName("cuid")]
        public string Cuid { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("sdk_openid")]
        public int SdkOpenid { get; set; }
    }

    public class CloudAppLoginInfo
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("uniqueId")]
        public string UniqueId { get; set; }

        [JsonPropertyName("walletData")]
        public WalletData WalletData { get; set; }
    }
}
