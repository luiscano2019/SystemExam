using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SystemExamWeb.Helpers;
using SystemExamWeb.Models;
using System.Text.Json;
using SystemExamWeb.Responses;

namespace SystemExamWeb.Controllers
{
    public class ExamsController : Controller
    {
        private readonly string _apiBaseUrl;

        public ExamsController(IConfiguration config)
        {
            _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index(Guid categoryId)
        {
            var exams = new List<Exam>();
            var apiUrl = $"{_apiBaseUrl}/api/Exam/by-category/{categoryId}";
      
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
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<Exam>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    exams = apiResponse?.Data?.ToList() ?? new List<Exam>();
                }
            }
            catch
            {
                exams = new List<Exam>();
            }

            ViewBag.CategoryId = categoryId;
            return View(exams);
        }
    }
}