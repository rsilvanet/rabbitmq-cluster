namespace RabbitMQ.Shared.Commands
{
    public class AskForMasterCmd
    {
        public AskForMasterCmd(string requesterId)
        {
            RequesterId = requesterId;
        }

        public string RequesterId { get; }
    }
}
