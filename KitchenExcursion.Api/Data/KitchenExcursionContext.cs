using KitchenExcursion.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.Data;

public class KitchenExcursionContext : DbContext
{
    public KitchenExcursionContext(
        DbContextOptions<KitchenExcursionContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
}