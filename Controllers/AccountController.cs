using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TravelExperts.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TravelExpertsData.Models;

namespace TravelExperts.Controllers
{
    public class AccountController : Controller
    {
        private readonly TravelExpertsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(TravelExpertsContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Customer model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
            }

            return View(model);
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
