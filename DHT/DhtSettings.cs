using System.Collections.Generic;
using Newtonsoft.Json;

namespace DHT
{
    [JsonObject("DhtSettings")]
    public class DhtSettings
    {
            [JsonProperty("maxRetryAttempts")] public int MaxRetryAttempts { get; set; }
            [JsonProperty("timeToLiveInSeconds")] public int TimeToLiveInSeconds { get; set; }
            [JsonProperty("connectionUrl")] public string ConnectionUrl { get; set; }
            [JsonProperty("maxNumberOfNodes")] public uint MaxNumberOfNodes { get; set; }
            [JsonProperty("intervalBetweenPeriodicCallsInSeconds")] public int IntervalBetweenPeriodicCallsInSeconds { get; set; }
            [JsonProperty("checkPredecessorCallInSeconds")] public int CheckPredecessorCallInSeconds { get; set; }
            [JsonProperty("stabilizeCallInSeconds")] public int StabilizeCallInSeconds { get; set; }
            [JsonProperty("fixFingersCallInSeconds")] public int FixFingersCallInSeconds { get; set; }
            [JsonProperty("keySpace")] public uint KeySpace { get; set; }
            [JsonProperty("bootstrapUrls")] public List<string?> BootstrapUrls { get; set; }
            [JsonProperty("replicas")] public int Replicas { get; set; }
    }
}