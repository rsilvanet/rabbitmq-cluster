using System;
using System.Net.Http;
using System.Threading;
using RabbitMQ.Shared;

namespace RabbitMQ.Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Actions.StartNewNode().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString() + ": " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}
