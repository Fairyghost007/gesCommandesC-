using gesCommandes.Models;
using gesCommandes.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace gesCommandes.Data.Fixtures
{
    public class Fixtures
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Seed Users
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Nom = "Admin",
                        Prenom = "Super",
                        Telephone = "0000000000",
                        Login = "admin",
                        Password = "admin123", // Note: Password should be hashed in real scenarios
                        Role = Role.RS
                    },
                    new User
                    {
                        Nom = "John",
                        Prenom = "Doe",
                        Telephone = "1111111111",
                        Login = "johndoe",
                        Password = "password123",
                        Role = Role.COMPTABLE
                    }
                );
            }

            // Seed Clients
            if (!context.Clients.Any())
            {
                // Create Users for Clients
                var aliceUser = new User
                {
                    Nom = "Alice",
                    Prenom = "Smith",
                    Telephone = "2222222222",
                    Login = "alice",
                    Password = "alice123",  // Ideally hashed
                    Role = Role.CLIENT
                };

                var bobUser = new User
                {
                    Nom = "Bob",
                    Prenom = "Brown",
                    Telephone = "3333333333",
                    Login = "bob",
                    Password = "bob123",  // Ideally hashed
                    Role = Role.CLIENT
                };

                // Add Users first, then create Clients with the corresponding userId
                context.Users.AddRange(aliceUser, bobUser);

                context.Clients.AddRange(
                    new Client
                    {
                        Adresse = "123 Main St",
                        Solde = 100.50,
                        userId = aliceUser.Id,  // Set the userId
                        user = aliceUser         // Associate User with Client
                    },
                    new Client
                    {
                        Adresse = "456 Market Ave",
                        Solde = 200.75,
                        userId = bobUser.Id,    // Set the userId
                        user = bobUser          // Associate User with Client
                    }
                );
            }

            // // Seed Commandes
            // if (!context.Commandes.Any())
            // {
            //     var aliceClient = context.Clients.Single(c => c.user.Login == "alice");

            //     context.Commandes.AddRange(
            //         new Commande
            //         {
            //             clientId = aliceClient.Id,
            //             client = aliceClient,
            //             montantCommande = 50.00,
            //         }
            //     );
            // }

            // Save changes to the database
            context.SaveChanges();
        }
    }
}

