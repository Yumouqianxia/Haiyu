using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys;

public class AccountMine
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("data")]
    public MineData Data { get; set; }

    /// <summary>
    /// 请求成功
    /// </summary>
    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public class MineData
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("mine")]
    public Mine Mine { get; set; }
}

public class Mine
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("collectCount")]
    public int CollectCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("fansCount")]
    public int FansCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("fansNewCount")]
    public int FansNewCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("followCount")]
    public int FollowCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("gender")]
    public int Gender { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goldNum")]
    public int GoldNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("headUrl")]
    public string HeadUrl { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("ifCompleteQuiz")]
    public int IfCompleteQuiz { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isFollow")]
    public int IsFollow { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isLoginUser")]
    public int IsLoginUser { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isMute")]
    public int IsMute { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("lastLoginModelType")]
    public string LastLoginModelType { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("lastLoginTime")]
    public string LastLoginTime { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("levelTotal")]
    public int LevelTotal { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("likeCount")]
    public int LikeCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("medalList")]
    public List<string> MedalList { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("mobile")]
    public string Mobile { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("postCount")]
    public int PostCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("registerTime")]
    public string RegisterTime { get; set; }

    /// <summary>
    /// 这个人很懒，还没有签名。
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("signatureReviewStatus")]
    public int SignatureReviewStatus { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
}
