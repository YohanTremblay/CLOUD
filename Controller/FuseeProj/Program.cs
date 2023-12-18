using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Authentication;
using MQTTnet.Client;
//using MQTTnet.Extensions.WebSocket4Net;
using MQTTnet.Formatter;
using MQTTnet;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Server;
//using MQTTnet.Samples.Helpers;

using FuseeProj.Extensions;
using MQTTnet.Packets;
using FuseeProj.Composants;

namespace FuseeProj
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ComposantsManager composantsManager = new ComposantsManager();
            MQTTRequestManager mqtt = new MQTTRequestManager(composantsManager);

            //Est-ce que le module est fonctionnel ? (True / False)
            bool module = true;
            // Inverse de codeFermeture
            bool running = true;

            await mqtt.Connect_Client();
            await mqtt.Subscribe_Topic();

            do
            {
                if(module)
                {
                    Console.WriteLine("Publier un message ? [O]ui, [N]on");
                    string go = Console.ReadLine();
              
                    if (go == "o" || go == "O")
                    {
                        Console.WriteLine("Entrez key :");
                        string key = Console.ReadLine();
                        mqtt.SendMessage(key, "Debut protocole", "");
                    }
                    else
                    {
                        running = false;                    
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Sleeping");
                }
            } while (running);

            await mqtt.Disconnect_Clean();
        }
    }
}
