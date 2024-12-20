using RabbitMQ.Client;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "direct_order_exchange", type: ExchangeType.Direct);

        Console.WriteLine("Enter order details (Type 'exit' to quit):");
        while (true)
        {
            var message = Console.ReadLine();
            if (message?.ToLower() == "exit") break;

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "direct_order_exchange", routingKey: "order.created", body: body);
            Console.WriteLine($" [x] Sent: {message}");
        }
    }
}
