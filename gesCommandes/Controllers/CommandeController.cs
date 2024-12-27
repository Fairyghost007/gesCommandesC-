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
    [Authorize(Roles = nameof(Role.RS))]
    public class CommandeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommandeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Commande
        public IActionResult Index()
        {
            var commandes = _context.Commandes
                .Include(c => c.client)
                    .ThenInclude(client => client.user) // Include user related to client
                .Include(c => c.livreur)
                .Include(c => c.paiement)
                .ToList();
            if (commandes != null)
            {
                Console.WriteLine($"❌Found {commandes.Count} commandes❌");
            }
            ViewBag.commandes = commandes;
            return View("Index");
        }

        // GET: Commande/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commande = await _context.Commandes
                .Include(c => c.client)
                .Include(c => c.livreur)
                .Include(c => c.paiement)
                .Include(c => c.detailCommandes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commande == null)
            {
                return NotFound();
            }

            return View(commande);
        }

        // GET: Commande/Create
        
    }
}

