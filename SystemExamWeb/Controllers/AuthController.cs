using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystemExamWeb.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (Request.Cookies.ContainsKey("jwt_token"))
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email y contraseña son requeridos.";
                return View();
            }

            var loginRequest = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            // Cambia la URL por la de tu API real
            var apiUrl = "http://localhost:5272/api/Auth/login";
            
            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Leer el token del JSON de respuesta
                    using var doc = JsonDocument.Parse(responseBody);
                    var token = doc.RootElement.GetProperty("token").GetString();
                    
                    // Guardar el token en una cookie segura
                    Response.Cookies.Append(
                        "jwt_token",
                        token,
                        new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // Solo HTTPS
                            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(2)
                        });
                    
                    // Redirigir al dashboard
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas o error de autenticación.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "No se pudo conectar con el servidor de autenticación.";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error inesperado: " + ex.Message;
            }
            
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Login");
        }
    }
} 