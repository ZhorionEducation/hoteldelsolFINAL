// Controllers/ManualController.cs
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Hotel.Controllers
{
    public class ManualController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Download()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/manuals/manual.pdf");
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "manual.pdf");
        }
    }
}