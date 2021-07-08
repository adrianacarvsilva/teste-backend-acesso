using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using teste_backend_acesso.Domain.Interfaces;
using teste_backend_acesso.Dto;

namespace Processing
{
    public class Transactions : BackgroundService
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly IServiceAccount _serviceAccount;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;

        public Transactions(IServiceAccount serviceAccount)
        {
            _hostname = "127.0.0.1";
            _queueName = "TRANSACAO";
            _username = "guest";
            _password = "guest";
            _serviceAccount = serviceAccount;
            InitializeRabbitMqListener();
        }

        private void InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var updateCustomerFullNameModel = JsonConvert.DeserializeObject<Transfer>(content);

                HandleMessage(updateCustomerFullNameModel);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(_queueName, false, consumer);

            return Task.CompletedTask;
        }

        public void Create(byte[] body)
        {
            _channel.BasicPublish(exchange: "", routingKey: _queueName, false, basicProperties: null, body: body);
        }

        private void HandleMessage(Transfer transfer)
        {
            _serviceAccount.ExecuteTransfer(transfer);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }


}
