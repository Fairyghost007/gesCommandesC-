using Microsoft.EntityFrameworkCore;
using gesCommandes.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace gesCommandes.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<DetailCommande> DetailCommandes { get; set; }
        public DbSet<Livreur> Livreurs { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Livraison> Livraisons { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints

            modelBuilder.Entity<Produit>()
                .Ignore(p => p.ImgFile);
            modelBuilder.Entity<User>()
                .Ignore(p => p.ImgFile);

            // Commande - Client (One-to-Many)
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.client)
                .WithMany(cl => cl.Commandes)
                .HasForeignKey(c => c.clientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TPT for Client
            modelBuilder.Entity<Client>().ToTable("Clients");

            modelBuilder.Entity<Client>()
                .Property(cl => cl.Solde)
                .HasDefaultValue(0.00);

            modelBuilder.Entity<Client>()
                .Property(cl => cl.Adresse)
                .HasMaxLength(255);

            // Configure the relationship between Client and User
            modelBuilder.Entity<Client>()
                .HasOne(cl => cl.user)  // A Client has one User
                .WithOne()  // A User is related to only one Client (one-to-one)
                .HasForeignKey<Client>(cl => cl.userId)  // userId is the foreign key in Client
                .OnDelete(DeleteBehavior.Cascade);  // Restrict delete behavior (optional)

            // Configure the relationship between Commande and Livreur (One-to-Many)
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.livreur)
                .WithMany(l => l.Commandes)
                .HasForeignKey(c => c.livreurId)
                .IsRequired(false) // Ensures `livreurId` is optional
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship between Commande and Paiement (One-to-One)
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.paiement)
                .WithOne(p => p.commande)
                .HasForeignKey<Commande>(c => c.paiementId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship between DetailCommande and Commande (One-to-Many)
            modelBuilder.Entity<DetailCommande>()
                .HasOne(dc => dc.Commande)
                .WithMany(c => c.detailCommandes)
                .HasForeignKey(dc => dc.commandeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between DetailCommande and Produit (Many-to-One)
            modelBuilder.Entity<DetailCommande>()
                .HasOne(dc => dc.Produit)
                .WithMany()
                .HasForeignKey(dc => dc.ProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship between Livraison and Commande (One-to-One)
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.Commande)
                .WithOne()
                .HasForeignKey<Livraison>(l => l.CommandeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between Livraison and Livreur (Many-to-One)
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.Livreur)
                .WithMany()
                .HasForeignKey(l => l.LivreurId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
