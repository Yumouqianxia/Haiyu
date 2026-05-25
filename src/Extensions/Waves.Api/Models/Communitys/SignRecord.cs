using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys;

public class SignRecord
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
    public List<SignRecordDataItem> Data { get; set; }

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

public class SignRecordDataItem
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("goodsId")]
    public string GoodsId { get; set; }

    /// <summary>
    /// 螺母
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
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("sendState")]
    public bool SendState { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("sendStateV2")]
    public int SendStateV2 { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("sigInDate")]
    public string SigInDate { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }
}
