using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormClient
{
    public partial class RabbitClient : Form
    {
        private readonly IConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        SynchronizationContext m_SyncContext = null;
        private EventingBasicConsumer _consumer;
        public RabbitClient()
        {
            m_SyncContext = SynchronizationContext.Current;
            var taskschecue = TaskScheduler.FromCurrentSynchronizationContext();
            InitializeComponent();
            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin"
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void RabbitClient_Load(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(_consumer == null)
            {
                _consumer = new EventingBasicConsumer(_channel);
                _consumer.Received += Consumer_Received;
                //设置不自动发送ack，防止消费者丢失消息。
                _channel.BasicConsume("DefaultQueue", false, _consumer);
                btnStart.Enabled = false;
            }
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var consumer = sender as IBasicConsumer;
            consumer.Model.BasicAck(e.DeliveryTag, false);
            var msg = "Recevied:\t" + Encoding.UTF8.GetString(e.Body);
            m_SyncContext.Post(s =>
            {
                this.textBox1.Text += DateTime.Now + ":" + (string)s;
            }, msg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
