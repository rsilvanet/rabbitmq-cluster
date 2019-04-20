using Newtonsoft.Json;
using System;

namespace RabbitMQ.Shared
{
    public class Node
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public bool Master { get; set; }
        public bool Active { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}