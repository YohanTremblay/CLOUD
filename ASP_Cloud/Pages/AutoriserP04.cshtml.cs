using ASP_Cloud.Models;
using ASP_Cloud.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace ASP_Cloud.Pages
{
    public class AutoriserP04Model : PageModel
    {
        private CosmosService _cosmosService;

        public IEnumerable<Product>? Products { get; set; }

        public async Task OnGetAsync()
        {
            Products ??= await _cosmosService.RetrieveActiveProductsAsync();
        }

        public AutoriserP04Model(ICosmosService cosmosService)
        {
            _cosmosService = new CosmosService();

            // Récupérer les éléments de la dernière heure et les trier par date
            List<Product> produits = (Products ?? Enumerable.Empty<Product>())
                                            .OrderBy(p => p.Date)
                                            .ToList();

            // Check if the sequence is not empty before calling Max
            int idLePlusGrand = produits.DefaultIfEmpty().Max(p => Convert.ToInt32(p?.id ?? "0"));

            Console.WriteLine("Création Product");
            Product product = new Product
            (
                id: Convert.ToString(idLePlusGrand + 1),
                codeProtocole: "P04",
                Date: DateTime.UtcNow.ToString()
            );

            Console.WriteLine("Envoi");
            Envoi(product);
        }


        private async void Envoi(Product product)
        {
            Console.WriteLine("Insert");
            await _cosmosService.container.UpsertItemAsync<Product>(item: product);
        }
    }
}
