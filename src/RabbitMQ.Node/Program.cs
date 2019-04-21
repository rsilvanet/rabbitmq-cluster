using System;
using RabbitMQ.Shared;

namespace RabbitMQ.Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Service.StartNewNode();
            Console.Read();
        }
    }
}
