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

public class MonService : ServiceBase
{
    private IMqttClient _mqttClient;

    public MonService()
    {
        ServiceName = "CLOUD_SERVICE";

    }

    protected override void OnStart(string[] args)
    {
        // Initialisation du service
        WriteToFile("Service démarré a " + DateTime.Now);
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

        await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("CLOUD/BALDURS_SLAVE/MODULE/STATUT/CODE").Build());

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

    public void WriteToFile(string text)
    {
        //string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string filePath = path + "\\ServiceLog" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".txt";

        if (!File.Exists(filePath))
        {
            using (System.IO.StreamWriter sw = File.CreateText(filePath))
            {
                sw.Write(text);
            }

        }
        else
        {
            using (System.IO.StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(text);
            }

        }
    }

    protected override void OnStop()
    {

    }
}
