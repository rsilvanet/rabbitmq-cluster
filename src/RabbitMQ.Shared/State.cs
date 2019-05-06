using RabbitMQ.Client;
using RabbitMQ.Shared.RPC;

namespace RabbitMQ.Shared
{
    public static class State
    {
        public static Node Master;
        public static Node Current;
        public static IModel Channel;
        public static RPCServer RPCServer;
        public static RPCClient RPCClient;
    }
}
