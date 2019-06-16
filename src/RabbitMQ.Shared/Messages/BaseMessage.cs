using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RabbitMQ.Shared.Messages
{
    [Serializable]
    public abstract class BaseMessage
    {
        public BaseMessage()
        {
            // To allow serialization/deserialization
        }

        public BaseMessage(string sourceId)
        {
            SourceId = sourceId;
            MessageId = Guid.NewGuid().ToString();
            Time = DateTime.Now;
        }

        public string SourceId { get; }
        public string MessageId { get; }
        public DateTime Time { get; }
        public abstract MessageType Type { get; }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public string GenerateLog()
        {
            return $"[{DateTime.Now.ToString("HH:mm:ss")}] " + this.ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static BaseMessage FromBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream) as BaseMessage;
            }
        }

        public static T FromBytes<T>(byte[] bytes) where T : BaseMessage
        {
            using (var stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream) as T;
            }
        }
    }

    public enum MessageType
    {
        Discover,
        MasterStatus,
        HealthCheck
    }
}
