using ListService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListService.Data;

public class ListServiceDbContext(DbContextOptions<ListServiceDbContext> options) : DbContext(options)
{
    public DbSet<ShoppingListEntity> Lists => Set<ShoppingListEntity>();

    public DbSet<ShoppingListItemEntity> Items => Set<ShoppingListItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingListEntity>(entity =>
        {
            entity.ToTable("lists");
            entity.HasKey(list => list.Id);

            entity.Property(list => list.Id)
                .HasColumnName("id")
                .HasColumnType("varchar");

            entity.Property(list => list.OwnerId)
                .HasColumnName("owner_id")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(list => list.Name)
                .HasColumnName("name")
                .HasColumnType("varchar");

            entity.Property(list => list.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");

            entity.HasMany(list => list.Items)
                .WithOne(item => item.List)
                .HasForeignKey(item => item.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(list => list.OwnerId)
                .HasDatabaseName("IX_lists_owner_id");
        });

        modelBuilder.Entity<ShoppingListItemEntity>(entity =>
        {
            entity.ToTable("items");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(item => item.ListId)
                .HasColumnName("list_id")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(item => item.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(item => item.StoreId)
                .HasColumnName("store_id")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(item => item.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("integer")
                .HasDefaultValue(1);

            entity.Property(item => item.Price)
                .HasColumnName("price")
                .HasColumnType("numeric(12,2)")
                .HasDefaultValue(0m);

            entity.Property(item => item.Checked)
                .HasColumnName("checked")
                .HasDefaultValue(false);
        });
    }
}
