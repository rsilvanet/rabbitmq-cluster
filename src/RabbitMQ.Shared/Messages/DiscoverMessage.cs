﻿using System;

namespace RabbitMQ.Shared.Messages
{
    [Serializable]
    public class DiscoverMessage : BaseMessage
    {
        public DiscoverMessage() : base(Guid.NewGuid().ToString()) { }

        public override MessageType Type => MessageType.Discover;

        public override string ToString() => $"Node {SourceId} asked for the master.";
    }
}
