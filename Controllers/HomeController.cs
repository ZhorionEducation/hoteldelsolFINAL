using Hotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hotel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HotelContext _context;
        public HomeController(ILogger<HomeController> logger, HotelContext context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Index()
        {
            var habitaciones = _context.Habitaciones
                .Where(h => h.Activo == true)
                .Include(h => h.Comodidades)
                .ToList();
            var servicios = _context.ServiciosAdicionales
                .Where(s => s.Activo == true)
                .ToList();

            var viewModel = new HomeViewModel
            {
                Habitaciones = habitaciones,
                Servicios = servicios
            };

            return View(viewModel);
            

            
            

        }



        [Route("Home/Error")]
        public IActionResult Error(int? statusCode = null)
        {
            var viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (statusCode.HasValue)
            {
                viewModel.StatusCode = statusCode.Value;

                switch (statusCode.Value)
                {
                    case 404:
                        viewModel.Message = "La página que buscas no existe.";
                        break;
                    case 500:
                        viewModel.Message = "Ha ocurrido un error en el servidor.";
                        break;
                    default:
                        viewModel.Message = "Ha ocurrido un error inesperado.";
                        break;
                }
            }

            // Logear el error
            _logger.LogError("Error {StatusCode}: {Message} - RequestId: {RequestId}", statusCode, viewModel.Message, viewModel.RequestId);

            return View("Error", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
