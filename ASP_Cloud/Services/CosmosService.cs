using ASP_Cloud.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ASP_Cloud.Services;

public class CosmosService : ICosmosService
{
    private CosmosClient _client;

    public CosmosService()
    {
        _client = new CosmosClient(
            connectionString: "AccountEndpoint=https://yohanbg.documents.azure.com:443/;AccountKey=FodB9JWfQVp7h06eoHM2urK2B9WN7eJhFBUWgcCf3LQKtDkYbpkWfzqEep6V0rxBGj6gEaLmEFSqACDbY6teiw==;"
        );
    }

    public Container container
    {
        get => _client.GetDatabase("cosmicworks").GetContainer("products");
    }

    public async Task<IEnumerable<Product>> RetrieveAllProductsAsync()
    {
        var queryable = container.GetItemLinqQueryable<Product>();
        using FeedIterator<Product> feed = queryable
            .Select(p => p)
            .ToFeedIterator();

        List<Product> results = new();
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }
        return results;
    }
    public async Task<IEnumerable<Product>> RetrieveActiveProductsAsync()
    {
        string sql = """
                    SELECT
                        p.id,
                        p.codeProtocole,
                        p.Date
                    FROM products p
                    JOIN t IN p.tags
                    WHERE t.name = @tagFilter
                    """;

        var query = new QueryDefinition(
        query: sql
        )
            .WithParameter("@tagFilter", "Tag-75");

        using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(
            queryDefinition: query
        );

        List<Product> results = new();

        while (feed.HasMoreResults)
        {
            FeedResponse<Product> response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }

        return results;

    }

}