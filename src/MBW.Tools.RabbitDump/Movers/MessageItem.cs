using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers
{
    class MessageItem
    {
        [JsonIgnore]
        public byte[] Data { get; set; }

        [JsonProperty("r")]
        public string RoutingKey { get; set; }

        [JsonProperty("e")]
        public string Exchange { get; set; }

        [JsonProperty("ts")]
        public DateTime? Created { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Properties { get; set; }
    }
}