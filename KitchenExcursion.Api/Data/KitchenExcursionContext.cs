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
    public DbSet<RecipeRevision> RecipeRevisions => Set<RecipeRevision>();
    public DbSet<RecipeCategory> RecipeCategories => Set<RecipeCategory>();
    public DbSet<RecipeStatus> RecipeStatuses => Set<RecipeStatus>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipeShoppingItem> RecipeShoppingItems => Set<RecipeShoppingItem>();
    public DbSet<RecipeCookLog> RecipeCookLogs => Set<RecipeCookLog>();
    public DbSet<RecipeCookRating> RecipeCookRatings => Set<RecipeCookRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("kitchen");

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(r => r.Slug).IsUnique();
            entity.HasIndex(r => r.HouseholdId);

            entity.Property(r => r.Slug).HasMaxLength(160);
            entity.Property(r => r.Image).HasMaxLength(500);

            entity.HasOne(r => r.CurrentRevision)
                .WithMany()
                .HasForeignKey(r => r.CurrentRevisionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RecipeRevision>(entity =>
        {
            entity.HasIndex(r => new { r.RecipeId, r.RevisionNumber }).IsUnique();
            entity.Property(r => r.Title).HasMaxLength(240);
            entity.Property(r => r.Badge).HasMaxLength(80);
            entity.Property(r => r.ImageAlt).HasMaxLength(500);
            entity.Property(r => r.PrepTime).HasMaxLength(80);
            entity.Property(r => r.CookTime).HasMaxLength(80);
            entity.Property(r => r.Serves).HasMaxLength(80);
            entity.Property(r => r.Meal).HasMaxLength(80);
            entity.Property(r => r.Protein).HasMaxLength(80);
            entity.Property(r => r.Method).HasMaxLength(80);
            entity.Property(r => r.ChangeNote).HasMaxLength(500);

            entity.HasOne(r => r.Recipe)
                .WithMany(r => r.Revisions)
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeCategory>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(120);
            entity.HasIndex(c => new { c.RecipeRevisionId, c.SortOrder }).IsUnique();
            entity.HasOne(c => c.Revision)
                .WithMany(r => r.Categories)
                .HasForeignKey(c => c.RecipeRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeStatus>(entity =>
        {
            entity.Property(s => s.Value).HasMaxLength(80);
            entity.HasIndex(s => new { s.RecipeRevisionId, s.SortOrder }).IsUnique();
            entity.HasOne(s => s.Revision)
                .WithMany(r => r.Statuses)
                .HasForeignKey(s => s.RecipeRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasIndex(i => new { i.RecipeRevisionId, i.SortOrder }).IsUnique();
            entity.HasOne(i => i.Revision)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(i => i.RecipeRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasIndex(s => new { s.RecipeRevisionId, s.SortOrder }).IsUnique();
            entity.HasOne(s => s.Revision)
                .WithMany(r => r.Steps)
                .HasForeignKey(s => s.RecipeRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeShoppingItem>(entity =>
        {
            entity.HasIndex(i => new { i.RecipeRevisionId, i.SortOrder }).IsUnique();
            entity.HasOne(i => i.Revision)
                .WithMany(r => r.ShoppingItems)
                .HasForeignKey(i => i.RecipeRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeCookLog>(entity =>
        {
            entity.Property(c => c.Author).HasMaxLength(120);
            entity.HasIndex(c => new { c.RecipeId, c.CookedAt });
            entity.HasOne(c => c.Recipe)
                .WithMany(r => r.CookLogs)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeCookRating>(entity =>
        {
            entity.Property(r => r.Rater).HasMaxLength(120);
            entity.HasIndex(r => new { r.RecipeCookLogId, r.Rater }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_RecipeCookRatings_Stars",
                "[Stars] >= 1 AND [Stars] <= 5"));

            entity.HasOne(r => r.CookLog)
                .WithMany(c => c.Ratings)
                .HasForeignKey(r => r.RecipeCookLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
