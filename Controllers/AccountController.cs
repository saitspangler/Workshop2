using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TravelExperts.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace TravelExperts.Controllers
{
    public class AccountController : Controller
    {
        private readonly TravelExpertsContext _context;

        public AccountController(TravelExpertsContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Customer customer)
        {
            var validCustomer = _context.Customers
                                .FirstOrDefault(c => c.Username == customer.Username && c.Password == customer.Password);

            if (validCustomer != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, validCustomer.Username),
            new Claim(ClaimTypes.NameIdentifier, validCustomer.CustomerId.ToString())
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }

        public async Task<IActionResult> ReservePackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            // Check if the user is logged in
            if (HttpContext.Session.GetInt32("CustomerId") == null)
            {
                // Redirect to the login page if the user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the logged-in user's CustomerId
            int customerId = HttpContext.Session.GetInt32("CustomerId").Value;

            // Create a new Booking
            Booking booking = new Booking
            {
                CustomerId = customerId,
                PackageId = id,
                BookingDate = DateTime.Now,
                // Add other necessary properties
            };

            // Save the new booking to the database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Redirect the user to their bookings page
            return RedirectToAction("Index", "Bookings");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Save the customer to the database
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Customer> entityEntry = _context.Add(customer);
                await _context.SaveChangesAsync();

                // Redirect the user to the login page after successful registration
                return RedirectToAction(nameof(Login));
            }

            return View(customer);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
