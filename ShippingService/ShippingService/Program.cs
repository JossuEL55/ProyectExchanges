﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "direct_order_exchange", type: ExchangeType.Direct);
        var queueName = channel.QueueDeclare(queue: "OrderQueue").QueueName;
        channel.QueueBind(queue: queueName, exchange: "direct_order_exchange", routingKey: "order.created");

        Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received: {message}");
        };
        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.ReadLine();
    }
}