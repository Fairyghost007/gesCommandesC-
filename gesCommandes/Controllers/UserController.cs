using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using gesCommandes.Data;
using gesCommandes.Models;
using gesCommandes.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Newtonsoft.Json;




namespace gesCommandes.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }
        [HttpPost]
        public async Task<IActionResult> Save(User user)
        {
            // Handle file upload if provided
            if (user.ImgFile != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "imgs", "users");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + user.ImgFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await user.ImgFile.CopyToAsync(fileStream);
                }

                user.ImgPath = $"/imgs/users/{uniqueFileName}";
            }

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: User/Login
        public IActionResult Login()
        {
            // ClaimsPrincipal claimUser= HttpContext.User;

            // if (claimUser.Identity.IsAuthenticated){
            //     if(claimUser.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Client"))
            //         return RedirectToAction("Commandes", "Client");
            //     return RedirectToAction("Index", "Home");
            // }

            return View();
        }



        // POST: User/Login
        [HttpPost]
        public async Task<IActionResult> Login(User modelLogin)
        {
            // Validate input
            if (string.IsNullOrEmpty(modelLogin.Login) || string.IsNullOrEmpty(modelLogin.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            // Retrieve user from database
            var user = _context.Users.FirstOrDefault(u =>
                u.Login == modelLogin.Login && u.Password == modelLogin.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            // Use the retrieved user's ID instead of the input model's ID
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties authProperties = new AuthenticationProperties()
            {
                IsPersistent = true,
                AllowRefresh = true
            };


            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ClaimsPrincipal currentUser = user;
            ClaimsPrincipal currentUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            // // Debugging logs
            Console.WriteLine($"Logged in User ID: {currentUser.FindFirstValue(ClaimTypes.NameIdentifier)}");
            Console.WriteLine($"User Role: {currentUser.FindFirstValue(ClaimTypes.Role)}");

            if (currentUser.Identity.IsAuthenticated)
            {
                // HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));

                if (currentUser.FindFirstValue(ClaimTypes.Role) == Role.CLIENT.ToString())
                {
                    return RedirectToAction("Commandes", "Client", new { userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)) });
                }
                if (currentUser.FindFirstValue(ClaimTypes.Role) == Role.RS.ToString())
                {
                    return RedirectToAction("Index", "Client", new { userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)) });
                }
                else
                {
                    Console.WriteLine($"❌❌❌❌❌❌❌❌❌User is IsAuthenticated but not a client❌❌❌❌❌❌❌❌❌❌❌");
                    return RedirectToAction("Index", "Home");
                }
            }
            Console.WriteLine($"❌❌❌❌❌❌❌❌❌User not IsAuthenticated❌❌❌❌❌❌❌❌❌❌❌");
            return RedirectToAction("Index", "Home");
        }





        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Login,Password,Role,Nom,Prenom,Telephone,Id,CreatedAt,UpdatedAt")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Login,Password,Role,Nom,Prenom,Telephone,Id,CreatedAt,UpdatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
