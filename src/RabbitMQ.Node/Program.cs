using System;
using RabbitMQ.Shared;

namespace RabbitMQ.Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var node = Service.CreateNewNode();

            Console.WriteLine($"Hello, your id is {node.Id}");
        }
    }
}
