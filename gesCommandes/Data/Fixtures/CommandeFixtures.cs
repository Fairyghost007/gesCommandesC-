using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using gesCommandes.Enums;
using gesCommandes.Models;

namespace gesCommandes.Data.Fixtures
{
    public class CommandeFixtures
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Commandes.Any())
            {
                context.Commandes.AddRange(
                    new Commande
                    {
                        montantCommande = 1000,
                        clientId = 1,
                    }
                );
            }

            context.SaveChanges();
        }
    }
}

