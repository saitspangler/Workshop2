using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelExperts.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace TravelExperts.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly TravelExpertsContext _context;

        public BookingsController(TravelExpertsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyBookings()
        {
            // Retrieve the CustomerId of the currently logged-in user
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get booking details related to the logged-in user
            var bookingDetails = _context.BookingDetails
                .Include(bd => bd.Booking)
                .ThenInclude(b => b.Customer)
                .Where(bd => bd.Booking.CustomerId == customerId)
                .ToList();

            return View(bookingDetails);
        }
    }
}
