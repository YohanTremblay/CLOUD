using ASP_Cloud.Models;

namespace ASP_Cloud.Services;

public interface ICosmosService
{
    public async Task<IEnumerable<Product>> RetrieveActiveProductsAsync()
    {
        await Task.Delay(1);

        return new List<Product>()
        {
            new Product(id: "1", codeProtocole: "P01", Date: "2019-09-26T07:58:30"),
            new Product(id: "3",codeProtocole: "P02", Date: "2019-11-26T07:58:30")

        };
    }
    public async Task<IEnumerable<Product>> RetrieveAllProductsAsync()
    {
        await Task.Delay(1);

        return new List<Product>()
        {
            new Product(id: "5", codeProtocole: "P01", Date: "2030-09-26T07:58:30"),
            new Product(id: "7",codeProtocole: "P03", Date: "2025-09-26T07:58:30")
        };
    }
}