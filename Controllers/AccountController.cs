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
                // Store the CustomerId in the session
                HttpContext.Session.SetInt32("CustomerId", validCustomer.CustomerId);

                // Add the FirstName claim
                var firstNameClaim = new Claim("FirstName", validCustomer.CustFirstName);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, validCustomer.Username),
            new Claim(ClaimTypes.NameIdentifier, validCustomer.CustomerId.ToString()),
            firstNameClaim // Add the claim to the list of claims
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

            // Find a suitable ProductSupplierId based on PackageId
            var packageProductSupplier = _context.PackagesProductsSuppliers
                                   .FirstOrDefault(pps => pps.PackageId == id);

            int? productSupplierId = null;

            if (packageProductSupplier != null)
            {
                productSupplierId = packageProductSupplier.ProductSupplierId;
            }

            // Create a new Booking
            Booking booking = new Booking
            {
                CustomerId = customerId,
                PackageId = id,
                BookingDate = DateTime.Now,
                // Add other necessary properties
            };

            // Save the new Booking to the database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Create a new BookingDetail
            BookingDetail bookingDetail = new BookingDetail
            {
                BookingId = booking.BookingId,
                ItineraryNo = 1, // Set the ItineraryNo (modify as needed)
                TripStart = package.PkgStartDate,
                TripEnd = package.PkgEndDate,
                Description = package.PkgDesc,
                Destination = "Example Destination", // Set the Destination (modify as needed)
                BasePrice = package.PkgBasePrice,
                AgencyCommission = package.PkgAgencyCommission,
                ProductSupplierId = productSupplierId, // Use the retrieved ProductSupplierId
                                                       // Set other properties for BookingDetail
            };

            // Save the new BookingDetail to the database
            _context.BookingDetails.Add(bookingDetail);
            await _context.SaveChangesAsync();

            // Redirect the user to their bookings page
            return RedirectToAction("MyBookings", "Bookings");
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
