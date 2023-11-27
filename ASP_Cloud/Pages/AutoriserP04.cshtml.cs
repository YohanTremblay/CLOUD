using ASP_Cloud.Models;
using ASP_Cloud.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASP_Cloud.Pages
{
    public class AutoriserP04Model : PageModel
    {
        private CosmosService _cosmosService;

        public AutoriserP04Model(ICosmosService cosmosService)
        {
            _cosmosService = new CosmosService();
            Console.WriteLine("Création Product");
            Product product = new(
                id: "20",
                codeProtocole: "P04",
                Date: DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")
            );
            Console.WriteLine("Envoie");
            Envoi(product); 
        }

        private async void Envoi(Product product)
        {
            Console.WriteLine("Insert");
            await _cosmosService.container.UpsertItemAsync<Product>(item: product);
        }
    }
}
