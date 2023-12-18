using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AzureFunctions
{
    public class Functions
    {
        private readonly ILogger _logger;
        CosmosDB database;

        public Functions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Functions>();
            database = new CosmosDB();
        }

        [Function("POST_Protocole")]
        public async Task<HttpResponseData> Post([HttpTrigger
                (AuthorizationLevel.Anonymous,
                "post",
                Route ="POST_Protocole")]
            HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a POST request.");

            var response = req.CreateResponse();

            string protocole = req.Query["codeProtocole"];
            string date = DateTime.UtcNow.ToString();
            string id = await GetID(protocole, "POST");

            await response.WriteAsJsonAsync(new
            {
                Protocole = protocole,
                Date = date,
                Id = id
            });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Proto proto = new Proto
                {
                    codeProtocole = protocole,
                    Date = date,
                    id = id
                };
                var container = database.CosmosClient.GetContainer(database.IdDatabase, database.IdContainer);
                await container.CreateItemAsync(proto);
            }

            return response;
        }

        [Function("GET_Protocole")]
        public async Task<HttpResponseData> Get([HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "GET_Protocole")]
            HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a GET request.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            string protocole = req.Query["codeProtocole"];
            string id = await GetID(protocole, "GET");

            var retrievedItem = await database.GetItemAsync<Proto>(id, protocole);

            await response.WriteAsJsonAsync(new
            {
                Protocole = retrievedItem.codeProtocole,
                Date = retrievedItem.Date
            });

            return response;
        }

        private async Task<string> GetID(string codeProtocole, string status)
        {
            var container = database.CosmosClient.GetContainer(database.IdDatabase, database.IdContainer);

            // Exécutez une requête pour récupérer tous les documents avec le protocole spécifié
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.codeProtocole = @codeProtocole")
                .WithParameter("@codeProtocole", codeProtocole);

            var iterator = container.GetItemQueryIterator<Proto>(query);
            var maxId = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
                    // Parsez l'ID du document et mettez à jour maxId si l'ID est plus grand
                    if (int.TryParse(item.id, out var currentId) && currentId > maxId)
                    {
                        maxId = currentId;
                    }
                }
            }

            if (status == "GET")
                return maxId.ToString();
            return (maxId + 1).ToString();
        }

    }
}
