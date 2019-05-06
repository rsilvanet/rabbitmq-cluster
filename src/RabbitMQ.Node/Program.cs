using System;
using System.Threading.Tasks;
using RabbitMQ.Shared;

namespace RabbitMQ.Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Actions.StartNewNode().Wait();
        }
    }
}
