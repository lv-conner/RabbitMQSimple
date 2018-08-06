using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static IConnectionFactory factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };
        static void Main(string[] args)
        {
            //DefaultQueueComsumer();
            //DirectQueueComsumer();
            //Subscribe();
            //TopicQueue();
            HeaderQueue();
            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    //channel.QueueDeclare("BroadCast", true, false, true, null);
            //    var consumer = new EventingBasicConsumer(channel);
            //    consumer.Received += Consumer_Received;
            //    //var value = channel.BasicConsume("BroadCast", false, consumer);
            //    channel.BasicConsume("DirectQueue", false, consumer);
            //    Console.WriteLine("start listen");
            //    Console.ReadKey();
            //}
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var consumer = sender as IBasicConsumer;
            consumer.Model.BasicAck(e.DeliveryTag, false);
            Console.WriteLine("Recevied:\t" + Encoding.UTF8.GetString(e.Body));
            Console.WriteLine(sender.GetType().FullName);
        }

        static void DefaultQueueComsumer()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                //设置不自动发送ack，防止消费者丢失消息。
                channel.BasicConsume("DefaultQueue", false, consumer);
                Console.ReadKey();
            }
        }

        static void DirectQueueComsumer()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("DirectQueue", false, consumer);
                Console.ReadKey();
            }
        }


        static void Subscribe()
        {
            Task.WaitAll(Task.Run(() => FanoutQueueComsumer()), Task.Run(() => FanoutQueueComsumer1()));
        }
        static void FanoutQueueComsumer()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("FanoutQueue", false, consumer);
                Console.ReadKey();
            }
        }

        static void FanoutQueueComsumer1()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("FanoutQueue1", false, consumer);
                Console.ReadKey();
            }
        }




        static void TopicQueue()
        {
            Task.WaitAll(Task.Run(() => GameQueue()), Task.Run(() => LiveQueue()), Task.Run(() => AllQueue()));
        }
        static void GameQueue()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("GameTopicQueue", false, consumer);
                Console.ReadKey();
            }
        }

        static void LiveQueue()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("LiveTopicQueue", false, consumer);
                Console.ReadKey();
            }
        }

        static void AllQueue()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("AllTopicQueue", false, consumer);
                Console.ReadKey();
            }
        }

        static void HeaderQueue()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume("HeaderQueue", false, consumer);
                Console.ReadKey();
            }
        }
    }
}
