using System.Linq;
using gesCommandes.Models;

namespace gesCommandes.Data.Fixtures
{
    public class ProduitFixtures
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Produits.Any())
            {
                context.Produits.AddRange(
                    new Produit
                    {
                        Libelle = "Product 1",
                        Prix = 200.50,
                        QteStock = 10
                    },
                    new Produit
                    {
                        Libelle = "Product 2",
                        Prix = 150.75,
                        QteStock = 20
                    },
                    new Produit
                    {
                        Libelle = "Product 3",
                        Prix = 99.99,
                        QteStock = 15
                    }
                );
            }

            context.SaveChanges();
        }
    }
}
