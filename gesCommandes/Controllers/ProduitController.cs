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
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace gesCommandes.Controllers
{
    [Authorize]
    [Authorize(Roles = nameof(Role.CLIENT) + "," + nameof(Role.RS))]
    public class ProduitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProduitController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Produit
        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("userId").Value;
            var client = _context.Clients
                .Include(c => c.user)
                .FirstOrDefault(c => c.userId == userId);
            var produits = await _context.Produits.ToListAsync();
            if (HttpContext.Session.GetObject<List<DetailCommande>>("Panier") == null)
            {
                HttpContext.Session.SetObject("Panier", new List<DetailCommande>());
            }

            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");
            ViewData["PanierCount"] = panier.Count;
            ViewBag.Client = client;
            return View(produits);
        }

        // GET: Produit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produit == null)
            {
                return NotFound();
            }
            if (HttpContext.Session.GetObject<List<DetailCommande>>("Panier") == null)
            {
                HttpContext.Session.SetObject("Panier", new List<DetailCommande>());
            }

            var panier = HttpContext.Session.GetObject<List<DetailCommande>>("Panier");
            ViewData["PanierCount"] = panier.Count;

            return View(produit);
        }

        // GET: Produit/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> save(Produit produit)
        {
            if (ModelState.IsValid)
            {
                if (produit.ImgFile != null)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "imgs", "produits");

                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + produit.ImgFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await produit.ImgFile.CopyToAsync(fileStream);
                    }

                    produit.ImgPath = $"/imgs/produits/{uniqueFileName}";
                }

                _context.Produits.Add(produit);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(produit);
        }


        // POST: Produit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Libelle,Prix,QteStock,Id,CreatedAt,UpdatedAt")] Produit produit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produit);
        }

        // GET: Produit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
            {
                return NotFound();
            }
            return View(produit);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Libelle,Prix,QteStock,Id,ImgFile,ImgPath,CreatedAt,UpdatedAt")] Produit produit)
        {
            if (id != produit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduitExists(produit.Id))
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
            return View(produit);
        }


        private bool ProduitExists(int id)
        {
            return _context.Produits.Any(e => e.Id == id);
        }

        // GET: Produit/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(produit.ImgPath))
            {
                var imagePath = Path.Combine("wwwroot", produit.ImgPath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Produits.Remove(produit);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
