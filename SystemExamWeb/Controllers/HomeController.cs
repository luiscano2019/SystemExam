using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SystemExamWeb.Models;
using SystemExamWeb.Filters;

namespace SystemExamWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Si está autenticado, redirigir al dashboard
            if (Request.Cookies.ContainsKey("jwt_token"))
            {
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Login", "Auth");
        }

        [RequireAuth]
        public IActionResult Dashboard()
        {
            // Aquí podrías leer información del usuario desde el token JWT
            ViewBag.UserEmail = "usuario@ejemplo.com";
            ViewBag.UserRole = "admin";
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
