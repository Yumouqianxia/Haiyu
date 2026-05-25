using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys;

public class GamerDataModel
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 请求成功
    /// </summary>
    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("data")]
    public GameData Data { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class GameData
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("gameId")]
    public int GameId { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("serverTime")]
    public int ServerTime { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }

    /// <summary>
    /// 鸣潮
    /// </summary>
    [JsonPropertyName("serverName")]
    public string ServerName { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("signInUrl")]
    public string SignInUrl { get; set; }

    /// <summary>
    /// 已完成签到
    /// </summary>
    [JsonPropertyName("signInTxt")]
    public string SignInTxt { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("hasSignIn")]
    public bool HasSignIn { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("roleId")]
    public string RoleId { get; set; }

    /// <summary>
    /// 怪
    /// </summary>
    [JsonPropertyName("roleName")]
    public string RoleName { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("energyData")]
    public EnergyData EnergyData { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("livenessData")]
    public LivenessData LivenessData { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("battlePassData")]
    public ObservableCollection<BattlePassDataItem> BattlePassData { get; set; }
}

public class EnergyData
{
    /// <summary>
    /// 结晶波片
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("img")]
    public string Img { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("refreshTimeStamp")]
    public int RefreshTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expireTimeStamp")]
    public string ExpireTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("cur")]
    public int Cur { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class LivenessData
{
    /// <summary>
    /// 活跃度
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("img")]
    public string Img { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("refreshTimeStamp")]
    public string RefreshTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expireTimeStamp")]
    public string ExpireTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("cur")]
    public int Cur { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class BattlePassDataItem
{
    /// <summary>
    /// 电台等级
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("img")]
    public string Img { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("refreshTimeStamp")]
    public string RefreshTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expireTimeStamp")]
    public string ExpireTimeStamp { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("cur")]
    public int Cur { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}
