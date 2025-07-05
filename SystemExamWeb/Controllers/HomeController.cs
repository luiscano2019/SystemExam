using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;
using SystemExamWeb.Filters;
using SystemExamWeb.Helpers;
using SystemExamWeb.Models;
using SystemExamWeb.Responses;

namespace SystemExamWeb.Controllers
{
    public class HomeController : Controller
    {
       
        private readonly string _apiBaseUrl;

        public HomeController(IConfiguration config)
        {
         
            _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
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
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel();
            var errorMessages = new List<string>();
            var apiUrl = $"{_apiBaseUrl}/api/Category";

            try
            {
                var token = TokenHelper.GetJwtToken(Request);
                using var httpClient = new HttpClient();

                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CategoryResponse>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    model.Categories = apiResponse?.Data?.ToList() ?? new List<CategoryResponse>();
                }
                else
                {
                    errorMessages.Add($"Error al obtener categorías: {response.StatusCode} - {response.ReasonPhrase}");
                    model.Categories = new List<CategoryResponse>();
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Excepción: {ex.Message}");
                model.Categories = new List<CategoryResponse>();
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
