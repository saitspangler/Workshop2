using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelExperts.Models;

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
