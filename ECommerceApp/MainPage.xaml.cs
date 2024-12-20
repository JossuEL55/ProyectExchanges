using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.ObjectModel;
using System.Text;

namespace ECommerceApp
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
        private IConnection _connection;
        private IModel _channel;

        public MainPage()
        {
            InitializeComponent();
            MessagesListView.ItemsSource = Messages;

            InitializeRabbitMQ();
            StartListening();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Configurar el exchange y la cola
            _channel.ExchangeDeclare(exchange: "topic_demo_exchange", type: ExchangeType.Topic);
            _channel.QueueDeclare(queue: "DemoQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "DemoQueue", exchange: "topic_demo_exchange", routingKey: "demo.#");
        }

        private void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                MainThread.BeginInvokeOnMainThread(() => Messages.Add($"Received: {message}"));
            };
            _channel.BasicConsume(queue: "DemoQueue", autoAck: true, consumer: consumer);
        }

        private void OnSendMessageClicked(object sender, EventArgs e)
        {
            var message = MessageEntry.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                DisplayAlert("Error", "Message cannot be empty.", "OK");
                return;
            }

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "topic_demo_exchange", routingKey: "demo.message", basicProperties: null, body: body);
            Messages.Add($"Sent: {message}");
            MessageEntry.Text = string.Empty;
        }

        protected override void OnDisappearing()
        {
            _channel?.Close();
            _connection?.Close();
            base.OnDisappearing();
        }
    }
}
