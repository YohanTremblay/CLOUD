using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
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

            var rmdId = new Random();
            string protocole = req.Query["protocole"];
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            string id = Convert.ToString(rmdId.Next(1, 700));

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

            string protocole = req.Query["protocole"];
            string id = req.Query["id"];

            var retrievedItem = await database.GetItemAsync<Proto>(id, protocole);

            await response.WriteAsJsonAsync(new
            {
                Protocole = retrievedItem.codeProtocole,
                Date = retrievedItem.Date
            });

            return response;
        }
    }
}
