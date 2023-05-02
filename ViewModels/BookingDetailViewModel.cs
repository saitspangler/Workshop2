using System;
using TravelExperts.Models;

namespace TravelExperts.ViewModels
{
    public class BookingDetailViewModel
    {
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
        public DateTime? TripStart { get; set; }
        public DateTime? TripEnd { get; set; }
        public string Description { get; set; }
        public string Destination { get; set; }
        public decimal? BasePrice { get; set; }
        public string? CustFirstName { get; set; }
        public string? CustLastName { get; set; }
    }
}
