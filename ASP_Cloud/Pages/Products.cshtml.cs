using Microsoft.AspNetCore.Mvc.RazorPages;
using ASP_Cloud.Models;
using ASP_Cloud.Services;

namespace ASP_Cloud.Pages;

public class ProductsPageModel : PageModel
{
    private readonly ICosmosService _cosmosService;

    public IEnumerable<Product>? Products { get; set; }

    public ProductsPageModel(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task OnGetAsync()
    {
        Products ??= await _cosmosService.RetrieveAllProductsAsync();
    }
}