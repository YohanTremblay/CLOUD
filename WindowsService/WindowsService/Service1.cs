using System;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Net.Http;
using System.Threading;

namespace WindowsService
{
    public partial class Service1 : ServiceBase
    {
        private IMqttClient client;

        public Service1()
        {
            ServiceName = "CLOUD_SERVICE";
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            WriteToFile("1. Service Start");
            try
            {
                var factory = new MqttFactory();
                client = factory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("dicjLinux01.CegepJonquiere.ca")
                    .WithCredentials("dicj", "dicj")
                    .Build();

                WriteToFile("2. Try to connected to the MQTTSERVER");
                var response = client.ConnectAsync(mqttClientOptions, CancellationToken.None);
                WriteToFile(response.Result.ToString());
                WriteToFile("3. Connected");

                client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("CLOUD/BALDURS_SLAVE/MODULE/STATUT/#").Build());
                WriteToFile("4. Subscribed to module statut");

                client.ApplicationMessageReceivedAsync += async e =>
                {
                    WriteToFile("RECEIVE MESSAGE");
                    WriteToFile($"Message reçu sur le topic : {e.ApplicationMessage.Topic}");
                    WriteToFile($"Contenu du message : {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                    // Vérifier si le message reçu est égal à "Disponible"
                    if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "{\"son\": \"cheering\", \"affichage\": \"Fin P01\"}")
                    {
                        WriteToFile("P01");
                        await SendPost("P01");
                    }
                    else if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "{\"son\": \"cheering\", \"affichage\": \"Fin P02\"}")
                    {
                        WriteToFile("P02");
                        await SendPost("P02");
                    }
                    else if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "{\"son\": \"cheering\", \"affichage\": \"Fin P03\"}")
                    {
                        WriteToFile("P03");
                        await SendPost("P03");
                    }

                    // Effectuer d'autres actions en fonction du message reçu
                    await Task.CompletedTask;
                };
                // Journalisation
                PublishServiceStatus("DISPONIBLE");

                await SendGet("P04");

                // À la fermeture du service
                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    Task.Run(async () =>
                    {
                        PublishServiceStatus("FERMETURE");
                        client.DisconnectAsync();
                    });
                };
            }
            catch (Exception e)
            {
                WriteToFile(e.Message.ToString());
            }
        }

        protected override void OnStop()
        {
            PublishServiceStatus("INDISPONIBLE");
        }

        private void PublishServiceStatus(string status)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("CLOUD/BALDURS_SLAVE/SERVICE/STATUT")
                .WithPayload(status)
                .WithRetainFlag()
                .Build();

            client.PublishAsync(message);
        }
        private void PublishServiceP04(string key)
        {
            string messageSend = $"{{\"affichage\" : \"OK\" , \"son\" :  \"\" }}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"CLOUD/BALDURS_SLAVE/CONTROLLER/CMD/{key}")
                .WithPayload(messageSend)
                .Build();

            client.PublishAsync(message);
        }

        public void WriteToFile(string text)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            string ext = ".txt";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = path + "\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace("/", "_") + ext;

            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(text);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(text);
                }
            }
        }

        public async Task SendPost(string text)
        {
            // URL de l'API à laquelle envoyer la requête POST
            string apiUrl = "https://azurefonctionyohanguillaume.azurewebsites.net/api/POST_Protocole";

            // Paramètre à inclure dans la requête
            string protocoleValue = text;

            // Construction de la chaîne de requête (si nécessaire)
            string queryString = $"?codeProtocole={Uri.EscapeDataString(protocoleValue)}";

            // Contenu de la requête (peut être vide dans ce cas)
            string postData = string.Empty;

            // Si vous avez besoin d'inclure le paramètre dans le corps de la requête, décommentez la ligne suivante
            //postData = $"{{\"protocole\": \"{protocoleValue}\"}}";

            // Encodage du contenu en bytes
            byte[] contentBytes = Encoding.UTF8.GetBytes(postData);

            // Ajout de la chaîne de requête à l'URL
            apiUrl += queryString;

            WriteToFile(apiUrl);

            // Création d'une instance de HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Création du contenu de la requête
                    using (HttpContent content = new ByteArrayContent(contentBytes))
                    {
                        // Définition du type de contenu (application/json dans cet exemple, ajustez selon les besoins)
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        // Envoi de la requête POST
                        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                        // Vérification du succès de la requête
                        if (response.IsSuccessStatusCode)
                        {
                            // Lecture de la réponse
                            string responseContent = await response.Content.ReadAsStringAsync();
                            WriteToFile($"Réponse du serveur : {responseContent}");
                        }
                        else
                        {
                            WriteToFile($"Erreur HTTP : {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToFile($"Erreur : {ex.Message}");
                }
            }
        }
        private async Task<string> SendGet(string codeProtocole)
        {
            // URL de l'API à laquelle envoyer la requête GET
            string apiUrl = $"https://azurefonctionyohanguillaume.azurewebsites.net/api/GET_Protocole?codeProtocole={Uri.EscapeDataString(codeProtocole)}";

            // Création d'une instance de HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Envoi de la requête GET
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Vérification du succès de la requête
                    if (response.IsSuccessStatusCode)
                    {
                        // Lecture de la réponse
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent.Contains("P04") == true)
                        {
                            PublishServiceP04("P04");
                        }
                        
                        return responseContent;
                    }
                    else
                    {
                        Console.WriteLine($"Erreur HTTP : {response.StatusCode}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                    return null;
                }
            }
        }

    }
}
