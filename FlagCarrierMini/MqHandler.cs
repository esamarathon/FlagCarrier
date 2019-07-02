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
        const string exchangeName = "bigbutton";

        public string TagScannedTopic => $"{AppSettings.DeviceId}.tag_scanned";

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

        public bool IsConnected => connection?.IsOpen == true;


        public void Connect(string host, string username, string password, ushort port, bool tls)
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
                factory.Port = tls ? AmqpTcpEndpoint.DefaultAmqpSslPort : AmqpTcpEndpoint.UseDefaultPort;

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

            factory.AutomaticRecoveryEnabled = true;

            connection = factory.CreateConnection();

            SetupChannel();
        }

        private void SetupChannel()
        {
            if (this.channel != null)
            {
                this.channel.Close();
                this.channel = null;
            }

            if (connection == null)
                throw new Exception("MqHandler not connected");

            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName,
                                    type: "topic",
                                    durable: true,
                                    autoDelete: true,
                                    arguments: new Dictionary<string, object>()
                                    {
                                        { "x-expires", 4 * 60 * 60 * 1000 }
                                    });

            this.channel = channel;
        }

        public void Publish(TagScannedEvent tagEvent)
        {
            if (channel == null)
                throw new Exception("No channel established when trying to publish event.");

            if (!channel.IsOpen)
                SetupChannel();

            string json = JsonConvert.SerializeObject(tagEvent, Formatting.None, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.DateTimeOffset });

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: TagScannedTopic,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(json));
        }
    }
}
