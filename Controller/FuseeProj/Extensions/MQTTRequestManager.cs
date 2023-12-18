using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MQTTnet.Client;
//using MQTTnet.Extensions.WebSocket4Net;
using MQTTnet.Formatter;
using MQTTnet;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Server;
using System.Xml.Schema;
//using MQTTnet.Samples.Helpers;
using FuseeProj.Composants;
using System.Configuration;

namespace FuseeProj.Extensions
{

    internal class MQTTRequestManager
    {
        private readonly ComposantsManager composantsManager;
        private readonly MqttFactory mqttFactory;
        private readonly IMqttClient mqttClient;

        public MQTTRequestManager(ComposantsManager composantManager)
        {
            this.composantsManager = composantManager;
            mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            _SetupMqttMessageToConsole();
        }

             
        public async Task Connect_Client()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(ConfigurationManager.AppSettings["MqttServer"])
                .WithCredentials(ConfigurationManager.AppSettings["MqttId"], ConfigurationManager.AppSettings["MqttPass"])
                .Build();

            // This will throw an exception if the server is not available. The result
            // from this message returns additional data which was sent from the server.
            var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            Console.WriteLine("The MQTT client is connected.");
        }

        public async Task Disconnect_Clean()
        {
            await mqttClient.DisconnectAsync();
            Console.WriteLine("The MQTT client is disconnected.");
        }

        public async void SendMessage(string key, string affichage = "", string son = "")
        {
            string message = $"{{\"affichage\" : \"{affichage}\" , \"son\" :  \"{son}\" }}";
            await _Publish_Message(key, message);
        }

        public async Task Subscribe_Topic()
        {
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(ConfigurationManager.AppSettings["MqttTopic1"]);
                    })
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(ConfigurationManager.AppSettings["MqttTopic2"]);
                    })
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(ConfigurationManager.AppSettings["MqttTopic3"]);
                    })
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(ConfigurationManager.AppSettings["MqttTopic4"]);
                    }).Build();

            var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topics.");
        }
        
        private void _SetupMqttMessageToConsole()
        {
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                try
                {
                    string topic = e.ApplicationMessage.Topic;
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    //Verification Reception
                    _ReceptionCheckStatut(topic, payload);

                    // Print Message
                    //Console.WriteLine("Received application message : " + payload);

                    return Task.CompletedTask;
                }
                catch
                {
                    throw;
                }
            };
        }

        private void _ReceptionCheckStatut(string topic, string payload)
        {
            string patternRoute = @"^CLOUD/BALDURS_SLAVE/MODULE/STATUT.*";

            var topics = topic.Split('/');

            if (Regex.IsMatch(topic, patternRoute))
            {
                if (topics.Length == 5)
                {
                    _ReceptionModuleStatutCode(topic, payload, topics[4]);
                }
                else
                {
                    _ReceptionModuleStatut(topic, payload);
                }
            }
        }

        private void _ReceptionModuleStatut(string topic, string payload)
        {
            Console.WriteLine("Message Module Status");
            Console.WriteLine("Message from " + topic + ": " + payload);
            composantsManager.Module.Status = !composantsManager.Module.Status;
        }

        private void _ReceptionModuleStatutCode(string topic, string payload, string code)
        {
            Console.WriteLine("Message with code " + code + " from " + topic + ": " + payload);
        }

        public void ChangeModuleStatus()
        {
            if (composantsManager.Module.Status)
            {
                composantsManager.Module.Status = false;
            }
            else
                composantsManager.Module.Status = true;

        }

        private async Task _Publish_Message(string key, string message)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"CLOUD/BALDURS_SLAVE/CONTROLLER/CMD/{key}")
                .WithPayload(message)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            Console.WriteLine("MQTT application message is published.");
        }
    }
}
