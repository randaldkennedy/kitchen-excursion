using KitchenExcursion.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.Data;

public class LaUltimaExcursionDbContext : DbContext
{
    public LaUltimaExcursionDbContext(
        DbContextOptions<LaUltimaExcursionDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdMember> HouseholdMembers => Set<HouseholdMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("platform");

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.EntraObjectId)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne<Household>()
            .WithMany()
            .HasForeignKey(u => u.DefaultHouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HouseholdMember>()
            .HasKey(hm => new
            {
                hm.HouseholdId,
                hm.UserId
            });

        modelBuilder.Entity<HouseholdMember>()
            .HasOne(hm => hm.Household)
            .WithMany(h => h.Members)
            .HasForeignKey(hm => hm.HouseholdId);

        modelBuilder.Entity<HouseholdMember>()
            .HasOne(hm => hm.User)
            .WithMany(u => u.HouseholdMemberships)
            .HasForeignKey(hm => hm.UserId);
    }
}
