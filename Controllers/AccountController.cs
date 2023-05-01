using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TravelExpertsData;

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Validate phone number format
                if (!Regex.IsMatch(customer.CustHomePhone, @"^\d{3}-\d{3}-\d{4}$"))
                {
                    ModelState.AddModelError("CustHomePhone", "Phone number must be in the format: 123-456-7890");
                    return View(customer);
                }

                // Validate postal code format
                if (!Regex.IsMatch(customer.CustPostal, @"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$"))
                {
                    ModelState.AddModelError("CustPostal", "Postal code must be in the format: A1A 1A1");
                    return View(customer);
                }

                // Validate email format
                if (!Regex.IsMatch(customer.CustEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    ModelState.AddModelError("CustEmail", "Email must be in a valid format");
                    return View(customer);
                }

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

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Register", "Account");
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
