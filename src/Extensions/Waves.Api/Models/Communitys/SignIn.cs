using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Waves.Api.Models.Communitys;

public class SignIn
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
    public SignInData Data { get; set; }

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

public class SignInData
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("disposableGoodsList")]
    public List<DisposableGoodsListItem> DisposableGoodsList { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("disposableSignNum")]
    public int DisposableSignNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("eventEndTimes")]
    public string EventEndTimes { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("eventStartTimes")]
    public string EventStartTimes { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expendGold")]
    public int ExpendGold { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expendNum")]
    public int ExpendNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isSigIn")]
    public bool IsSigIn { get; set; }

    /// <summary>
    /// 无
    /// </summary>
    [JsonPropertyName("loopDescription")]
    public string LoopDescription { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("loopEndTimes")]
    public string LoopEndTimes { get; set; }

    /// <summary>
    /// 限时签到活动
    /// </summary>
    [JsonPropertyName("loopSignName")]
    public string LoopSignName { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("loopSignNum")]
    public int LoopSignNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("loopStartTimes")]
    public string LoopStartTimes { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("nowServerTimes")]
    public string NowServerTimes { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("omissionNnm")]
    public int OmissionNnm { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("openNotifica")]
    public bool OpenNotifica { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("redirectContent")]
    public string RedirectContent { get; set; }

    /// <summary>
    /// 任务中心
    /// </summary>
    [JsonPropertyName("redirectText")]
    public string RedirectText { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("redirectType")]
    public int RedirectType { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("repleNum")]
    public int RepleNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("sigInNum")]
    public int SigInNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("signInGoodsConfigs")]
    public List<SignInGoodsConfigsItem> SignInGoodsConfigs { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("signLoopGoodsList")]
    public List<SignInGoodsConfigsItem> SignLoopGoodsList { get; set; }
}

public class SignInGoodsConfigsItem
{
    private int goodNum;
    private int serialNum;

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsId")]
    public int GoodsId { get; set; }

    /// <summary>
    /// 中级共鸣促剂
    /// </summary>
    [JsonPropertyName("goodsName")]
    public string GoodsName { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsNum")]
    public int GoodsNum
    {
        get { return goodNum; }
        set { goodNum = value; }
    }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsUrl")]
    public string GoodsUrl { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isGain")]
    public bool IsGain { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("serialNum")]
    public int SerialNum
    {
        get => serialNum;
        set { serialNum = value; }
    }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("signId")]
    public int SignId { get; set; }

    [JsonIgnore]
    public string SignResult { get; set; }

    [JsonIgnore]
    public bool IsSign { get; set; }
}

public class DisposableGoodsListItem
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsId")]
    public int GoodsId { get; set; }

    /// <summary>
    /// 星声
    /// </summary>
    [JsonPropertyName("goodsName")]
    public string GoodsName { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsNum")]
    public int GoodsNum { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsUrl")]
    public string GoodsUrl { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("isGain")]
    public bool IsGain { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("serialNum")]
    public int SerialNum { get; set; }
}
