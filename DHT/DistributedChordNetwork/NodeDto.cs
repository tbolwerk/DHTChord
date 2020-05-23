using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DHT.Formatting;

namespace DHT
{
    public class NodeDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public NodeDto Predecessor { get; set; }
        public NodeDto Successor { get; set; }

        public override string ToString()
        {
            return JsonCustomFormatter.SerializeObject(this, 2);
        }
    }
}