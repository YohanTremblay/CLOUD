using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using System.Timers;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Net.Http;
using System.Text;

public class MonService : ServiceBase
{
    private IMqttClient _mqttClient;

    public MonService()
    {
        ServiceName = "CLOUD_SERVICE";

    }

    protected override void OnStart(string[] args)
    {
        Task.Run(() => ConnectAndStartAsync());
    }

    private async Task ConnectAndStartAsync()
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("dicjLinux01.CegepJonquiere.ca", 1883)
            .WithCredentials("dicj", "dicj")
            .Build();

        await _mqttClient.ConnectAsync(options);

        await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("CLOUD/ASGARD/MODULE/STATUT/CODE").Build());

        client.ApplicationMessageReceivedAsync += async e =>
        {
            Console.WriteLine($"Message reçu sur le topic : {e.ApplicationMessage.Topic}");
            Console.WriteLine($"Contenu du message : {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

            // Vérifier si le message reçu est égal à "Disponible"
            if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "Fin P01")
            {
                SendPost("P01");
            }
            else if(Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "Protocole 2 terminé")
            {
                SendPost("P02");
            }
            else if (Encoding.UTF8.GetString(e.ApplicationMessage.Payload) == "Protocole 3 terminé")
            {
                SendPost("P03");
            }

            // Effectuer d'autres actions en fonction du message reçu

            await Task.CompletedTask;
        };

        await PublishServiceStatus("DEMARAGE");

        // Journalisation
       
        
        // À la fermeture du service
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            Task.Run(async () =>
            {
                await PublishServiceStatus("FERMETURE");
                await _mqttClient.DisconnectAsync();

            });
        };
    }

    private async Task PublishServiceStatus(string status)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("CLOUD/BALDURS_SLAVE/SERVICE/STATUT")
            .WithPayload(status)
            .WithRetainFlag()
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    public async Task SendPost(string text)
    {
        // URL de l'API à laquelle envoyer la requête POST
        string apiUrl = "https://azurefonctionyohanguillaume.azurewebsites.net/api/POST_Protocole?";

        // Paramètre à inclure dans la requête
        string protocoleValue = text;

        // Construction de la chaîne de requête (si nécessaire)
        string queryString = $"?protocole={Uri.EscapeDataString(protocoleValue)}";

        // Contenu de la requête (peut être vide dans ce cas)
        string postData = string.Empty;

        // Si vous avez besoin d'inclure le paramètre dans le corps de la requête, décommentez la ligne suivante
        //postData = $"{{\"protocole\": \"{protocoleValue}\"}}";

        // Encodage du contenu en bytes
        byte[] contentBytes = Encoding.UTF8.GetBytes(postData);

        // Ajout de la chaîne de requête à l'URL
        apiUrl += queryString;

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
                        Console.WriteLine($"Réponse du serveur : {responseContent}");
                    }
                    else
                    {
                        Console.WriteLine($"Erreur HTTP : {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
    }
    protected override void OnStop()
    {

    }
}
