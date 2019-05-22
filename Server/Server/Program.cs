using System;
using RabbitMQ.Client;
using RabbitMQ;
using RabbitMQ.Util;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client.Events;
using System.Threading;

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
            if (args.Length == 0)
            {
                return;
            }
            switch (args[0])
            {
                case "default":
                    DefaultExchange();
                    break;
                case "fanout":
                    FanoutExchange();
                    break;
                case "topic":
                    TopicExchange();
                    break;
                case "header":
                    HeaderExchange();
                    break;
                default:
                    Console.WriteLine("no select exchange");
                    break;
            }
            RelativeApi();
            DefaultExchange();
            //DirectExchange();
            //FanoutExchange();
            //TopicExchange();
            //HeaderExchange();
            Console.ReadKey();
        }
        static void RelativeApi()
        {
            IConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",//rabbitmq主机名称
                Port = 5672,//端口位置
                UserName = "admin",//登陆用户名
                Password = "admin"//密码
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                connection.Abort();//中断连接
                //connection.ChannelMax;最大信道数量。默认为2047；
                //connection.ClientProvidedName;提供给消息代理的可读的客户端名称。
                channel.Abort();//关闭会话。
                channel.ConfirmSelect();//启用发布确认。
                channel.WaitForConfirms();
                //时间
                channel.BasicAcks += PublishSuccess;//发布成功回调;
                channel.BasicNacks += PublishError;//发布消息失败时触发;
                channel.BasicRecoverOk += BrokerRecover;//消息代理恢复时触发；
                channel.BasicReturn += ReturnCommand;//当然会一个basic.return命令时触发。
                channel.CallbackException += ExceptionCall;//当发生异常时触发。
                channel.ModelShutdown += ModalShutDown;//通道关闭时。

            }

        }

        private static void ModalShutDown(object sender, ShutdownEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ExceptionCall(object sender, CallbackExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ReturnCommand(object sender, BasicReturnEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BrokerRecover(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PublishError(object sender, BasicNackEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PublishSuccess(object sender, BasicAckEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 默认交换机发布消息
        /// </summary>
        static void DefaultExchange()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //开启Confirm模式,防止生产者丢失数据
                channel.ConfirmSelect();
                //当消息队列收到消息时触发
                channel.BasicAcks += Channel_BasicAcks;
                //当消息队列接收消息错误时触发
                channel.BasicNacks += Channel_BasicNacks;
                channel.QueueDeclare("DefaultQueue", true, false, false, null);
                //当设置mandatory设置为true时，交换机根据路由键找不到对应的队列时，将强制将消息返回到生产者，此时可以。
                channel.BasicReturn += Channel_BasicReturn;
                //设置持久化模式为persistent防止消息队列丢失消息。
                var prop = channel.CreateBasicProperties();
                prop.DeliveryMode = 2;
                channel.BasicPublish("", "DefaultQueue", prop, Encoding.UTF8.GetBytes("Default Queue Message"));
                Console.WriteLine("Default Queue Message publish complete");
                channel.BasicPublish("", "DefaultQueue", prop, Encoding.UTF8.GetBytes("Another default queue message"));
                Console.WriteLine("Another default queue message publish complete");
                Thread.Sleep(3000);
                Console.ReadLine();
            }
        }

        private static void Channel_BasicReturn(object sender, BasicReturnEventArgs e)
        {
            //e.Body;为发送到队列失败的消息。
        }

        private static void Channel_BasicNacks(object sender, BasicNackEventArgs e)
        {
            Console.WriteLine("消息发送失败。消息Id为:\t" + e.DeliveryTag);
        }

        private static void Channel_BasicAcks(object sender, BasicAckEventArgs e)
        {
            Console.WriteLine("消息发送成功。消息Id为：\t" + e.DeliveryTag);
            Console.WriteLine("publish Complete");
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
