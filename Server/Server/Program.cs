using System;
using RabbitMQ.Client;
using RabbitMQ;
using RabbitMQ.Util;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        static IConnectionFactory factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin"
        };
        static void Main(string[] args)
        {
            //DefaultExchange();
            //DirectExchange();
            //FanoutExchange();
            //TopicExchange();
            HeaderExchange();
        }

        /// <summary>
        /// 默认交换机发布消息
        /// </summary>
        static void DefaultExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("DefaultQueue", true, false, false, null);
                channel.BasicPublish("", "DefaultQueue", null, Encoding.UTF8.GetBytes("Default Queue Message"));
                Console.WriteLine("Default Queue Message publish complete");
            }
        }

        /// <summary>
        /// 直连交换机发布消息。适用于发布任务
        /// </summary>
        static void DirectExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("DirectQueue", true, false, false, null);
                channel.ExchangeDeclare("DirectQueue_Ex", "direct", true, false, null);
                channel.QueueBind("DirectQueue", "DirectQueue_Ex", "DirectQueue", null);
                channel.BasicPublish("DirectQueue_Ex", "DirectQueue", null, Encoding.UTF8.GetBytes("DirectQueue Message"));
                Console.WriteLine("Direct Queue Message publish complete");
            }
        }

        /// <summary>
        /// 扇形交换机，用于发布订阅。
        /// </summary>
        static void FanoutExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("FanoutQueue", true, false, false, null);
                channel.QueueDeclare("FanoutQueue1", true, false, false, null);
                channel.ExchangeDeclare("FanoutQueue_Ex", "fanout", true, false, null);
                channel.QueueBind("FanoutQueue", "FanoutQueue_Ex", "DirectQueue", null);
                channel.QueueBind("FanoutQueue1", "FanoutQueue_Ex", "DirectQueue", null);
                channel.BasicPublish("FanoutQueue_Ex", "", null, Encoding.UTF8.GetBytes("FanoutQueue Message"));
                Console.WriteLine("FanoutQueue Queue Message publish complete");
            }
        }

        /// <summary>
        /// 主题交换机，根据路由键分发给不同的队列。当路由键值为#时，与扇形交换机一致。
        /// </summary>
        static void TopicExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("GameTopicQueue", true, false, false, null);
                channel.QueueDeclare("LiveTopicQueue", true, false, false, null);
                channel.QueueDeclare("AllTopicQueue", true, false, false, null);
                channel.ExchangeDeclare("ApplicationExchange", "topic", true, false, null);
                channel.QueueBind("GameTopicQueue", "ApplicationExchange", "Game", null);
                channel.QueueBind("LiveTopicQueue", "ApplicationExchange", "Live", null);
                channel.QueueBind("AllTopicQueue", "ApplicationExchange", "#", null);
                channel.BasicPublish("ApplicationExchange", "Game", null, Encoding.UTF8.GetBytes("Game Message"));
                channel.BasicPublish("ApplicationExchange", "Live", null, Encoding.UTF8.GetBytes("Live Message"));
                Console.WriteLine("FanoutQueue Queue Message publish complete");

            }
        }

        /// <summary>
        /// 头交换机。根据消息头部进行投送消息。路由键值为Headers
        /// </summary>
        static void HeaderExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("HeaderQueue", true, false, false, null);
                channel.ExchangeDeclare("HeaderQueue_Ex", "headers", true, false, null);
                var prop = channel.CreateBasicProperties();
                prop.Headers = new Dictionary<string, object>();
                prop.Headers.Add("name", "tim");
                channel.QueueBind("HeaderQueue", "HeaderQueue_Ex", "Game",prop.Headers);
                channel.BasicPublish("HeaderQueue_Ex", "Game", prop, Encoding.UTF8.GetBytes("Headers Message"));
                Console.WriteLine("HeaderQueue Queue Message publish complete");
                Console.ReadKey();

            }
        }

    }
}
