using RabbitMQ.Client;
using System.Text.Json;

ConnectionFactory factory = new()
{
    HostName = "localhost",
};
using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare("users", false, false);