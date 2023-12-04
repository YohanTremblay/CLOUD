using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public class CosmosDB
    {
        private const string EndpointUri = "https://yohanbg.documents.azure.com:443/";
        private const string PrimaryKey = "FodB9JWfQVp7h06eoHM2urK2B9WN7eJhFBUWgcCf3LQKtDkYbpkWfzqEep6V0rxBGj6gEaLmEFSqACDbY6teiw==";
        private const string DatabaseId = "cosmicworks";
        private const string ContainerId = "products";

        private CosmosClient cosmosClient;
        private Database database;
        private Container container;

        public CosmosDB()
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            Initialize().Wait();
        }

        private async Task Initialize()
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            container = await database.CreateContainerIfNotExistsAsync(ContainerId, "/codeProtocole");
        }

        public async Task<ItemResponse<T>> AddItemAsync<T>(T item)
        {
            return await container.CreateItemAsync(item);
        }

        public async Task<T> GetItemAsync<T>(string id, string partitionKey)
        {
            try
            {
                ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default(T);
            }
        }

        public CosmosClient CosmosClient { get { return cosmosClient; } }
        public Database Database { get { return database; } }
        public Container Container { get { return container; } }
        public string IdDatabase { get { return DatabaseId; } }
        public string IdContainer { get { return ContainerId; } }
    }
}
