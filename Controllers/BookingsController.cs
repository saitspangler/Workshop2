using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelExperts.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using TravelExperts.ViewModels;

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
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var bookingDetails = _context.BookingDetails
                .Include(bd => bd.Booking)
                .ThenInclude(b => b.Customer)
                .Where(bd => bd.Booking.CustomerId == customerId)
                .Select(bd => new BookingDetailViewModel
                {
                    BookingId = bd.BookingId,
                    Booking = bd.Booking,
                    TripStart = bd.TripStart,
                    TripEnd = bd.TripEnd,
                    Description = bd.Description,
                    Destination = bd.Destination,
                    BasePrice = bd.BasePrice,
                    CustFirstName = bd.Booking.Customer.CustFirstName,
                    CustLastName = bd.Booking.Customer.CustLastName
                })
                .ToList();

            return View(bookingDetails);
        }


    }
}
