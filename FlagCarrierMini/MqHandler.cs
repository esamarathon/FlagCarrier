using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace FlagCarrierMini
{
    public class MqHandler : IDisposable
    {
        private IConnection connection = null;
        private IModel channel = null;
        private string queue = null;

        public MqHandler()
        {
        }

        ~MqHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (channel != null)
            {
                channel.Close();
                channel.Dispose();
                channel = null;
            }

            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        public void Close()
        {
            Dispose();
        }

        public bool IsConnected
        {
            get
            {
                if (connection == null)
                    return false;
                if (!connection.IsOpen)
                    return false;
                return queue != null;
            }
        }


        public void Connect(string host, string username = null, string password = null, ushort port = 0, bool tls = true)
        {
            Close();

            var factory = new ConnectionFactory();

            string[] hosts = host.Split("/", 2);

            factory.HostName = hosts[0];
            factory.VirtualHost = (hosts.Length > 1) ? hosts[1] : "/";

            if (username != null && username != "")
                factory.UserName = username;

            if (password != null && password != "")
                factory.Password = password;

            if (port != 0)
                factory.Port = port;
            else
                factory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;

            if (tls)
            {
                factory.Ssl.Enabled = true;
                factory.Ssl.Version = SslProtocols.Tls12;
                factory.Ssl.ServerName = factory.HostName;
            }
            else
            {
                factory.Ssl.Enabled = false;
            }

            connection = factory.CreateConnection();
        }

        public void Subscribe(string queue)
        {
            if (this.channel != null)
            {
                this.channel.Close();
                this.channel = null;
            }

            if (connection == null)
                throw new Exception("MqHandler not connected");

            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            this.queue = queue;
            this.channel = channel;
        }

        public void Publish(TagScannedEvent tagEvent)
        {
            if (channel == null)
                throw new Exception("No channel established when trying to publish event.");

            if (!channel.IsOpen)
                Subscribe(queue);

            string json = JsonConvert.SerializeObject(tagEvent, Formatting.None);

            channel.BasicPublish(exchange: "",
                                 routingKey: queue,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(json));
        }
    }
}
