using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAppCore.Models;

public partial class EcommerceDbContext : DbContext
{
    public EcommerceDbContext()
    {
    }

    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<DataLog> DataLogs { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<PurchaseAuditLog> PurchaseAuditLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("data source = HP-PAVILION;initial catalog = EcommerceDb;integrated security = true;Trusted_Connection = true;TrustServerCertificate = true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ActivityLogs");

            entity.Property(e => e.ActivityDateAndTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<DataLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.DataLogs");

            entity.Property(e => e.ActivityDateAndTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorId).HasName("PK_dbo.ErrorLogs");

            entity.Property(e => e.DateAndTime).HasColumnType("datetime");
        });

        
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK_dbo.Orders");

            entity.HasIndex(e => e.ProductId, "IX_ProductId");

            entity.HasIndex(e => e.UserId, "IX_UserId");

            entity.Property(e => e.OrderDateAndTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(128);

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_dbo.Orders_dbo.Products_ProductId");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.Orders_dbo.Users_UserId");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK_dbo.Products");
        });

        modelBuilder.Entity<PurchaseAuditLog>(entity =>
        {
            entity.HasKey(e => e.OrderNumber).HasName("PK_dbo.PurchaseAuditLogs");

            entity.Property(e => e.OrderDateAndTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_dbo.Users");

            entity.Property(e => e.UserId).HasMaxLength(128);
            entity.Property(e => e.Dob).HasColumnType("datetime");
            entity.Property(e => e.Pwd).HasMaxLength(25);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
