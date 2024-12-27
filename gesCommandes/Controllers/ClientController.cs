using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using gesCommandes.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using gesCommandes.Models;
using gesCommandes.Enums;
using System.Text.Json;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gesCommandes.Controllers
{
    [Authorize]
    [Authorize(Roles = nameof(Role.CLIENT) + "," + nameof(Role.RS))]
    // [Authorize(Roles = nameof(Role.CLIENT))]
    // [Authorize(Roles = nameof(Role.RS))]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;



        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DetailCommandes(int commandeId)
        {
            try
            {
                Console.WriteLine($"❌Attempting to fetch details for Commande ID: {commandeId}");

                // Fetch the commande
                var commande = await _context.Commandes
                    .Include(c => c.client)
                        .ThenInclude(c => c.user)
                    .FirstOrDefaultAsync(c => c.Id == commandeId);

                if (commande == null || commande.client == null || commande.client.user == null)
                {
                    Console.WriteLine($"❌Invalid data for Commande ID: {commandeId}");
                    return NotFound("Commande, client, or user not found.");
                }

                var detailCommandes = await _context.DetailCommandes
                    .Include(dc => dc.Produit)
                    .Where(dc => dc.commandeId == commandeId)
                    .ToListAsync();
                Console.WriteLine($"❌Details for Commande ID: {commandeId}, Count: {detailCommandes.Count}");

                if (detailCommandes == null || !detailCommandes.Any())
                {
                    Console.WriteLine($"❌No details found for Commande ID: {commandeId}");
                    return NotFound("No details found for this order.");
                }

                // Prepare view data
                ViewBag.Commande = commande;
                ViewBag.DetailCommandes = detailCommandes;

                ViewBag.TotalMontant = commande.montantCommande;
                ViewBag.ClientName = commande.client.user.Nom ?? "Unknown";


                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌Error in DetailCommandes: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching order details");
            }
        }


        public IActionResult Commandes(int userId)
        {
            Console.WriteLine($"************UserId: {userId}");
            HttpContext.Session.SetInt32("userId", userId);
            var client = _context.Clients
                .Include(c => c.user)
                .FirstOrDefault(c => c.userId == userId);
            Console.WriteLine($"*******************Client: {client}");
            if (client == null)
            {
                return NotFound("Client not found");
            }

            var userOrders = _context.Commandes
                .Where(c => c.clientId == client.Id)
                .Include(c => c.client)
                .ToList();
            Console.WriteLine($"*************************UserOrders: {userOrders.Count}");
            if (HttpContext.Session.GetObject<List<DetailCommande>>("Panier") == null)
            {
                HttpContext.Session.SetObject("Panier", new List<DetailCommande>());
            }
            int clientId = client.Id;
            HttpContext.Session.SetInt32("ClientId", clientId);


            // Retrieve the Panier from the session and calculate the item count
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");
            ViewData["PanierCount"] = panier.Count;

            ViewBag.Client = client;
            ViewBag.UserOrders = userOrders;
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> CreateCommande(int clientId)
        {
            Console.WriteLine($"❌ Debug: clientId = {clientId} ❌");

            var client = await _context.Clients
                .Include(c => c.user)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null || clientId == 0)
            {
                return RedirectToAction("Login", "User");
            }

            var produits = await _context.Produits
                .Where(p => p.QteStock > 0)
                .ToListAsync();

            // Retrieve or initialize the panier
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier") ?? new List<DetailCommande>();

            // Store clientId in session for later use
            HttpContext.Session.SetInt32("ClientId", clientId);

            // Calculate total price
            var total = panier.Sum(p => p.Prix);

            ViewData["PanierCount"] = panier.Count;
            // Pass data to the view
            ViewData["Client"] = client;
            ViewData["Produits"] = produits;
            ViewData["Panier"] = panier;
            ViewData["Total"] = total;

            return View();
        }



        // Add to Panier
        [HttpPost]
        public async Task<IActionResult> AddToPanier(int produitId, int qteCommande, int clientId)
        {
            var produit = await _context.Produits.FindAsync(produitId);
            if (produit == null || qteCommande > produit.QteStock)
            {
                return BadRequest("❌Invalid product or quantity exceeds stock");
            }

            // Log session data for debugging
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");
            if (panier == null)
            {
                Console.WriteLine("❌Panier is null, creating a new panier.");
                panier = new List<DetailCommande>();
            }
            Console.WriteLine($"❌ClienID: {clientId}");
            // Log product and quantity
            Console.WriteLine($"❌Adding product: {produit.Libelle}, Quantity: {qteCommande}");

            // Add or update the product in the panier
            var existingDetail = panier.FirstOrDefault(d => d.ProduitId == produitId);
            if (existingDetail != null)
            {
                existingDetail.qteCommande += qteCommande;
                existingDetail.Prix = existingDetail.qteCommande * produit.Prix;
            }
            else
            {
                panier.Add(new DetailCommande
                {
                    ProduitId = produitId,
                    Produit = produit,
                    qteCommande = qteCommande,
                    Prix = produit.Prix * qteCommande
                });
            }

            // Log the panier contents
            Console.WriteLine($"❌Panier contains {panier.Count} items.");

            // Store the updated panier back in session
            HttpContext.Session.SetObject("Panier", panier);

            // Get client data
            // var clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Retrieve client and products
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
            {
                Console.WriteLine("❌Client not found.");
                return NotFound("❌Client not found.");
            }

            // Get available products
            var produits = await _context.Produits
                .Where(p => p.QteStock > 0)
                .ToListAsync();

            // Prepare view data
            ViewBag.Client = client;
            ViewBag.Produits = produits;
            ViewBag.Panier = panier;
            ViewBag.Total = panier.Sum(d => d.Prix);
            return RedirectToAction("Index", "Produit");

            // return RedirectToAction("CreateCommande", new { clientId = clientId });
        }


        // Remove from Panier
        [HttpPost]
        public async Task<IActionResult> RemoveFromPanier(int produitId)
        {
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");

            if (panier != null)
            {
                var itemToRemove = panier.FirstOrDefault(d => d.ProduitId == produitId);
                if (itemToRemove != null)
                {
                    panier.Remove(itemToRemove);
                    HttpContext.Session.SetObject("Panier", panier);
                }
            }

            // Get clientId from session
            var clientId = HttpContext.Session.GetInt32("ClientId");

            // Reload the CreateCommande page with the updated panier
            var produits = await _context.Produits
                .Where(p => p.QteStock > 0)
                .ToListAsync(); // Awaited this query

            // Use ViewBag to pass data to the view
            ViewBag.Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
            ViewBag.Produits = produits;
            ViewBag.Panier = panier;
            ViewBag.Total = panier?.Sum(d => d.Prix) ?? 0;

            return RedirectToAction("CreateCommande", new { clientId = clientId });
        }



        [HttpPost]
        public async Task<IActionResult> SaveCommande()
        {
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");

            if (panier == null || !panier.Any())
            {
                return BadRequest("Panier is empty");
            }

            // Get clientId from session
            var clientId = HttpContext.Session.GetInt32("ClientId");

            if (clientId == null || clientId == 0)
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                // Create new Commande
                var commande = new Commande
                {
                    clientId = clientId.Value,
                    montantCommande = panier.Sum(d => d.Prix),
                };

                // Add details to the commande
                commande.detailCommandes = panier.Select(p => new DetailCommande
                {
                    ProduitId = p.ProduitId,
                    qteCommande = p.qteCommande,
                    Prix = p.Prix,
                    Commande = commande
                }).ToList();

                // 🎗🎗🎗🎗🎗🎗🎗Reduce stock for each product🎗🎗🎗🎗🎗🎗🎗🎗🎗🎗🎗
                // foreach (var detail in commande.detailCommandes)
                // {
                //     var produit = await _context.Produits.FindAsync(detail.ProduitId);
                //     if (produit != null)
                //     {
                //         produit.QteStock -= detail.qteCommande;
                //     }
                // }

                // Add and save the commande
                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                // Clear the session panier
                HttpContext.Session.Remove("Panier");

                // Get the current user's ID to pass to Commandes action
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Redirect to Commandes with the correct user ID
                return RedirectToAction("Commandes", new { userId = int.Parse(userId) });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving commande: {ex.Message}");
                return BadRequest("An error occurred while saving the order");
            }
        }
        // Update Quantity in Panier
        [HttpPost("update-quantity/{clientId}/{produitId}")]
        public async Task<IActionResult> UpdateQuantity(int produitId, string action, int clientId)
        {
            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");

            if (panier == null)
            {
                return BadRequest("❌ Panier is empty");
            }

            // Find the product in the panier
            var itemToUpdate = panier.FirstOrDefault(d => d.ProduitId == produitId);

            if (itemToUpdate == null)
            {
                return NotFound("❌ Product not found in panier");
            }

            // Fetch the product from the database to check stock availability
            var produit = await _context.Produits.FindAsync(produitId);

            if (produit == null)
            {
                return NotFound("❌ Product not found in database");
            }

            // Update the quantity based on the action
            if (action == "increment")
            {
                if (itemToUpdate.qteCommande < produit.QteStock)
                {
                    itemToUpdate.qteCommande++;
                }
                else
                {
                    return BadRequest("❌ Cannot exceed available stock");
                }
            }
            else if (action == "decrement")
            {
                if (itemToUpdate.qteCommande > 1)
                {
                    itemToUpdate.qteCommande--;
                }
                else
                {
                    return BadRequest("❌ Quantity cannot be less than 1");
                }
            }
            else
            {
                return BadRequest("❌ Invalid action");
            }

            // Update the price for the item
            itemToUpdate.Prix = itemToUpdate.qteCommande * produit.Prix;

            // Update the panier in session
            HttpContext.Session.SetObject("Panier", panier);

            // Recalculate total
            var total = panier.Sum(d => d.Prix);

            // Redirect back to the command page
            return RedirectToAction("CreateCommande", new { clientId = clientId });
        }
        // [HttpPost("Index/{userId}")]
        public IActionResult Index(int userId)
        {
            HttpContext.Session.SetInt32("userId", userId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            ViewData["User"] = user;
            ViewData["clients"] = _context.Clients
                .Include(c => c.user)
                .ToList();
            return View();
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.user)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // public IActionResult Create()
        // {
        //     ViewData["userId"] = new SelectList(_context.Users, "Id", "Id");
        //     return View();
        // }

        [HttpPost]
        public async Task<IActionResult> Save(Client client)
        {
            Console.WriteLine("❌Saving client...");

            // Ensure Solde is set to 0
            client.Solde = 0;

            // Initialize Commandes if null
            if (client.Commandes == null)
            {
                client.Commandes = new List<Commande>();
            }

            // If user is not fully filled out, set to null
            if (client.user != null &&
                string.IsNullOrWhiteSpace(client.user.Login) &&
                string.IsNullOrWhiteSpace(client.user.Password))
            {
                client.user = null;
            }

            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌Model state is invalid.");

                // If user is null or empty, remove user fields from model state
                if (client.user == null ||
                    (string.IsNullOrWhiteSpace(client.user.Nom) &&
                     string.IsNullOrWhiteSpace(client.user.Prenom) &&
                     string.IsNullOrWhiteSpace(client.user.Telephone) &&
                     string.IsNullOrWhiteSpace(client.user.Login) &&
                     string.IsNullOrWhiteSpace(client.user.Password)))
                {
                    ModelState.Remove("user.Nom");
                    ModelState.Remove("user.Prenom");
                    ModelState.Remove("user.Telephone");
                    ModelState.Remove("user.Login");
                    ModelState.Remove("user.Password");
                }
                // _context.Clients.Add(client);
                // await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Handle file upload for user image if it exists
                if (client.user != null && client.user.ImgFile != null)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine("wwwroot", "imgs", "users");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + client.user.ImgFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await client.user.ImgFile.CopyToAsync(fileStream);
                        }

                        client.user.ImgPath = $"/imgs/users/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌Error uploading file: {ex.Message}");
                        ModelState.AddModelError("User.ImgFile", "An error occurred while uploading the file.");
                        return View("Create", client);
                    }
                }

                // Save the user first if it exists
                if (client.user != null)
                {
                    Console.WriteLine("Creating user...");

                    // Set the user role to CLIENT
                    client.user.Role = Role.CLIENT;

                    // Add the user to the context
                    _context.Users.Add(client.user);
                    await _context.SaveChangesAsync();

                    // Update client with user ID
                    client.userId = client.user.Id;
                }

                // Save the client
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                Console.WriteLine($"❌Client saved: {client.Id} {client.Adresse}");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"❌Error saving data: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the entity changes. See the inner exception for details.");
                return View("Create", client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌Error saving data: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the data.");
                return View("Create", client);
            }
        }






        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            ViewData["userId"] = new SelectList(_context.Users, "Id", "Id", client.userId);
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Solde,Adresse,userId,Id,CreatedAt,UpdatedAt")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["userId"] = new SelectList(_context.Users, "Id", "Id", client.userId);
            return View(client);
        }

        public async Task<IActionResult> Delete(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    // Fetch the client
    var client = await _context.Clients
        .FirstOrDefaultAsync(c => c.Id == id);

    if (client == null)
    {
        return NotFound();
    }

    // Check if the client has an associated user
    if (client.userId != null)
    {
        // Find the user by userId
        var user = await _context.Users.FindAsync(client.userId);

        if (user != null)
        {
            // Delete the user's image file if it exists
            if (!string.IsNullOrWhiteSpace(user.ImgPath))
            {
                var imagePath = Path.Combine("wwwroot", user.ImgPath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Remove the user
            _context.Users.Remove(user);
        }
    }

    // Remove the client
    _context.Clients.Remove(client);

    // Save changes to the database
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }



    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }






}
