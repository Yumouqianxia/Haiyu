using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Waves.Api.Models.CloudGame;

public class CloudGameNode
{
    [JsonPropertyName("regionName")]
    public string RegionName { get; set; }

    [JsonPropertyName("regionDelay")]
    public int RegionDelay { get; set; }

    [JsonPropertyName("regionScore")]
    public int RegionScore { get; set; }

    [JsonPropertyName("regionState")]
    public int RegionState { get; set; }

    [JsonPropertyName("fastWaiting")]
    public int FastWaiting { get; set; }

    [JsonPropertyName("slowWaiting")]
    public int SlowWaiting { get; set; }
    public int Delay { get; private set; }

    [JsonPropertyName("nodeList")]
    public List<NodeList> NodeList
    {
        get => field;
        set 
        {
            if(value != null && value.Count>0)
                this.Delay = value.Select(x => x.Delay).Sum();
            field = value; 
        }
    }
}

public class NodeList
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("delay")]
    public int Delay { get; set; }
}
