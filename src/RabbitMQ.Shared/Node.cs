using Newtonsoft.Json;
using System;

namespace RabbitMQ.Shared
{
    public class Node
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public EStatus Status { get; set; }
        public bool Master { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public enum EStatus
        {
            Created,
            Starting,
            Running,
            Stopping,
            Stopped
        }
    }
}