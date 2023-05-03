using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelExperts.Models;
using TravelExperts.ViewModels;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;

namespace TravelExperts.Controllers
{
    public class HomeController : Controller
    {
        private readonly TravelExpertsContext _context;

        public HomeController(TravelExpertsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var packages = await _context.Packages.ToListAsync();
            ViewBag.PackageNames = packages.Select(p => p.PkgName).ToList();
            return View(packages);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactForm model)
        {
            if (ModelState.IsValid)
            {
                await SendEmail(model);
                return RedirectToAction("ContactSuccess");
            }

            return View(model);
        }

        private async Task SendEmail(ContactForm model)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Your Name", "your@email.com"));
            email.To.Add(new MailboxAddress("Recipient Name", "recipient@email.com"));
            email.Subject = "Contact Us Form Submission";
            email.Body = new TextPart("plain")
            {
                Text = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.your-email-provider.com", 587, false);
                await client.AuthenticateAsync("your@email.com", "your-email-password");
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
        }

        public IActionResult ContactSuccess()
        {
            return View();
        }

        public async Task<IActionResult> SearchPackages(string selectedPackage)
        {
            if (string.IsNullOrEmpty(selectedPackage))
            {
                return RedirectToAction("Index");
            }

            var selectedPackageDetails = await _context.Packages
                .FirstOrDefaultAsync(p => p.PkgName == selectedPackage);

            if (selectedPackageDetails == null)
            {
                return NotFound();
            }

            return View(selectedPackageDetails);
        }
    }
}
