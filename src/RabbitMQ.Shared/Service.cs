using System;

namespace RabbitMQ.Shared
{
    public static class Service
    {
        public static Node CreateNewNode()
        {
            return new Node()
            {
                Id = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                Active = true,
                Master = false
            };
        }
    }
}
