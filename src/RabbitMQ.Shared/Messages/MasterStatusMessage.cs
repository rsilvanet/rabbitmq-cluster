using System;

namespace RabbitMQ.Shared.Messages
{
    [Serializable]
    public class MasterStatusMessage : BaseMessage
    {
        public MasterStatusMessage(string masterId, DateTime masterSince) : base(masterId)
        {
            Since = masterSince;
        }

        public DateTime Since { get; }

        public override MessageType Type => MessageType.MasterStatus;

        public override string ToString() => $"The master is {SourceId} since {Since}.";
    }
}
