using gesCommandes.Data;
using gesCommandes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gesCommandes.Controllers
{
    [Authorize(Roles = "RS")]
    public class LivraisonController : Controller
    {
        private readonly ApplicationDbContext  _context;

        public LivraisonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Livraisons
        public async Task<IActionResult> Index()
        {
            var gesCommandesContext = _context.Livraisons.Include(l => l.Commande).Include(l => l.Livreur);
            return View(await gesCommandesContext.ToListAsync());
        }

        // GET: Livraisons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livraison = await _context.Livraisons
                .Include(l => l.Commande)
                .Include(l => l.Livreur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (livraison == null)
            {
                return NotFound();
            }

            return View(livraison);
        }

        // GET: Livraisons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Livraisons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateLivraison,CommandeId,LivreurId")] Livraison livraison)
        {
            if (ModelState.IsValid)
            {
                _context.Add(livraison);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(livraison);
        }

        // GET: Livraisons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livraison = await _context.Livraisons.FindAsync(id);
            if (livraison == null)
            {
                return NotFound();
            }
            return View(livraison);
        }

        // POST: Livraisons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateLivraison,CommandeId,LivreurId")] Livraison livraison)
        {
            if (id != livraison.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(livraison);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LivraisonExists(livraison.Id))
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
            return View(livraison);
        }

        // GET: Livraisons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livraison = await _context.Livraisons
                .Include(l => l.Commande)
                .Include(l => l.Livreur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (livraison == null)
            {
                return NotFound();
            }

            return View(livraison);
        }

        // POST: Livraisons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var livraison = await _context.Livraisons.FindAsync(id);
            _context.Livraisons.Remove(livraison);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LivraisonExists(int id)
        {
            return _context.Livraisons.Any(e => e.Id == id);
        }
    }
}
