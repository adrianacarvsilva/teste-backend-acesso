using RabbitMQ.Client;
using System;
using System.Text;

namespace rabbit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connection = GetConnectionFactory();

            var iconnection = CreateConnection(connection);

            //var queue = CreateQueue("Transfer", iconnection);

            WriteMessageOnQueue("Criando transferencia", "Transfer", iconnection);
        }

        public static ConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            return connectionFactory;
        }

        public static IConnection CreateConnection(ConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection();
        }

        public static QueueDeclareOk CreateQueue(string queueName, IConnection connection)
        {
            QueueDeclareOk queue;
            using (var channel = connection.CreateModel())
            {
                queue = channel.QueueDeclare(queueName, false, false, false, null);
            }
            return queue;
        }

        public static bool WriteMessageOnQueue(string message, string queueName, IConnection connection)
        {
            using (var channel = connection.CreateModel())
            {
                channel.BasicPublish(string.Empty, queueName, null, Encoding.ASCII.GetBytes(message));
            }

            return true;
        }

        public static string RetrieveSingleMessage(string queueName, IConnection connection)
        {
            BasicGetResult data;
            using (var channel = connection.CreateModel())
            {
                data = channel.BasicGet(queueName, true);
            }

            return data.Body.ToString();

            //return data != null ? System.Text.Encoding.UTF8.GetString(data.Body) : null;
        }

    }
}
