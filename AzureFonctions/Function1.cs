using System.Net;
using System.Security.Policy;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFonctions
{
    public class FunctionFinProtocole
    {
        private readonly ILogger _logger;

        public FunctionFinProtocole(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FunctionFinProtocole>();
        }

        [Function("FunctionFinProtocole")]
        public async Task<HttpResponseData> Run([HttpTrigger
            (AuthorizationLevel.Anonymous,
            "post",
            Route ="FinProtocole")]
             HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var response = req.CreateResponse();
            string Nameparam = "PO4";
            DateTime dateparam = DateTime.Now;
            await response.WriteAsJsonAsync(new
            {
                Nameparam,
                dateparam,
            });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var url = "https://bd-projet-technuagiques.documents.azure.com:443/";
                var db = "bdprojet";
                var cle = "ZUlHLFClx0oAnEkmoPGOTpmd63tpF4KxzafFOIGCuoBRSj2dYQUYHnejhtdwYGbdf5ACEowmXOQeACDbXFKRxg==";
                CosmosClient cosmosClient = new CosmosClient(url, cle);
                Database baseDonnees = cosmosClient.GetDatabase("id");
                Container conteneur = cosmosClient.GetContainer(db, "id");

                Item i = new Item
                {
                    id = "7",
                    category = "blabla ......  oooh je suis decu ",
                    name = Nameparam,
                    description = dateparam.ToString(),
                    isComplete = false
                };
                conteneur.CreateItemAsync(i);
                

            }


            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
        [Function("FunctionValidationLancement")]
        public async Task<bool> ValidationLancement([HttpTrigger
            (AuthorizationLevel.Anonymous,
            "get",
            Route ="validationLancement")]
             HttpRequestData req)
        {
            bool retour = false;
            var response = req.CreateResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var url = "https://bd-projet-technuagiques.documents.azure.com:443/";
                var db = "bdprojet";
                var cle = "ZUlHLFClx0oAnEkmoPGOTpmd63tpF4KxzafFOIGCuoBRSj2dYQUYHnejhtdwYGbdf5ACEowmXOQeACDbXFKRxg==";
                CosmosClient cosmosClient = new CosmosClient(url, cle);
                Database baseDonnees = cosmosClient.GetDatabase("id");
                Container conteneur = cosmosClient.GetContainer(db, "id");

                var queryText = "SELECT TOP 1 * FROM c ORDER BY c.date DESC";
                QueryDefinition queryDefinition = new QueryDefinition(queryText);
                FeedIterator<Item> feedIterator = conteneur.GetItemQueryIterator<Item>(queryDefinition);

                while (feedIterator.HasMoreResults)
                {
                    var rep = await feedIterator.ReadNextAsync();
                    foreach (var item in rep)
                    {
                        // Récupérez la dernière valeur insérée
                        var derniereValeurInserer = item.name;
                        var autreValeur = "PO4";
                        if (derniereValeurInserer == autreValeur)
                        {
                            retour = true;
                        }
                        else
                        {
                            retour = false;
                        }
                    }
                }
            }
            return retour;
        }
    }
}
