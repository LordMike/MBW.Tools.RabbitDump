using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers
{
    class MessageItem
    {
        [JsonIgnore]
        public byte[] Data { get; set; }

        public string RoutingKey { get; set; }

        public string Exchange { get; set; }

        public DateTime? Created { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Properties { get; set; }
    }
}