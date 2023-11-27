using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

class Program
{
    static async Task Main(string[] args)
    {
        string mqttServer = ConfigurationManager.AppSettings["MqttServer"];
        string mqttPort = ConfigurationManager.AppSettings["MqttPort"];
        string mqttUsername = ConfigurationManager.AppSettings["MqttUsername"];
        string mqttPassword = ConfigurationManager.AppSettings["MqttPassword"];
        string mqttTopic1 = ConfigurationManager.AppSettings["MqttTopic1"];
        string mqttTopic2 = ConfigurationManager.AppSettings["MqttTopic2"];

        int mqttPortInt = Convert.ToInt32(mqttPort);

        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttServer, mqttPortInt)
            .WithCredentials(mqttUsername, mqttPassword)
            .Build();

        await client.ConnectAsync(options);

        if (client.IsConnected)
        {
            Console.WriteLine("Connexion MQTT réussie !");
        }
        else
        {
            Console.WriteLine("Échec de la connexion MQTT.");
            return;
        }
        
        await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(mqttTopic1).Build());
        await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(mqttTopic2).Build());

        bool moduleFonctionnel = false;

        client.ApplicationMessageReceivedAsync += async e =>
        {
            Console.WriteLine($"Message reçu sur le topic : {e.ApplicationMessage.Topic}");
            Console.WriteLine($"Contenu du message : {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

            // Vérifier si le message reçu est égal à "Disponible"
            if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "Disponible")
            {
                // Activer le module fonctionnel ici
                moduleFonctionnel = true;
                Console.WriteLine("Module fonctionnel activé.");
            }
            else
            {
                // Désactiver le module fonctionnel ici si nécessaire
                moduleFonctionnel = false;
                Console.WriteLine("Module fonctionnel désactivé.");
            }

            // Effectuer d'autres actions en fonction du message reçu

            await Task.CompletedTask;
        };

        string CODE_FERMETURE = "exit";

        // Boucle de réception des messages
        while (true)
        {
            if (moduleFonctionnel)
            {
                Console.Write("Entrez le code de protocole : ");
                string code = Console.ReadLine();

                if (code == CODE_FERMETURE)
                {
                    break;
                }

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(mqttTopic1)
                    .WithPayload(code)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await client.PublishAsync(message);

                Console.WriteLine("Code de protocole envoyé.");
            }
            else
            {
                Console.WriteLine("En attente...");
            }

            // Réception des messages
            client.ApplicationMessageReceivedAsync += e =>
            {
                    Console.WriteLine($"Message reçu sur le topic : {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"Contenu du message : {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                return Task.CompletedTask;
            };
        }

        await client.DisconnectAsync();
    }
}

