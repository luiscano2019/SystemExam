using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SystemExamWeb.Helpers;
using SystemExamWeb.Models;
using SystemExamWeb.Responses;

namespace SystemExamWeb.Controllers
{
    public class ExamsController : JwtControllerBase
    {
        private readonly string _apiBaseUrl;
      

        public ExamsController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
          
        }

        public async Task<IActionResult> IndexPorCategory(Guid categoryId)
        {
            var exams = new List<Exam>();
            var apiUrl = $"{_apiBaseUrl}/api/Exam/by-category/{categoryId}";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ViewBag.ErrorMessage = "No tienes permisos para ver los exámenes.";
                return View(new List<Exam>());
            }
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<Exam>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                exams = apiResponse?.Data?.ToList() ?? new List<Exam>();
            }
            ViewBag.CategoryId = categoryId;
            return View(exams);
        }

        public async Task<IActionResult> Index()
        {
            var apiUrl = $"{_apiBaseUrl}/api/Exam";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.GetAsync(apiUrl);
     
            if (!response.IsSuccessStatusCode) return View(Enumerable.Empty<Exam>());
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Exam>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(apiResponse?.Data ?? new List<Exam>());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid) return View(exam);
            var apiUrl = $"{_apiBaseUrl}/api/Exam";
            using var httpClient = CreateHttpClientWithJwt();
            var json = JsonSerializer.Serialize(exam);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiUrl, content);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError("", "No tienes permisos para crear exámenes.");
                return View(exam);
            }
            if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            ModelState.AddModelError("", "Error al crear examen");
            return View(exam);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Exam/{id}";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ViewBag.ErrorMessage = "No tienes permisos para editar este examen.";
                return View();
            }
            if (!response.IsSuccessStatusCode) return NotFound();
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<Exam>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(apiResponse?.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Exam exam)
        {
            if (!ModelState.IsValid) return View(exam);
            var apiUrl = $"{_apiBaseUrl}/api/Exam/{exam.Id}";
            using var httpClient = CreateHttpClientWithJwt();
            var json = JsonSerializer.Serialize(exam);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(apiUrl, content);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError("", "No tienes permisos para editar exámenes.");
                return View(exam);
            }
            if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            ModelState.AddModelError("", "Error al editar examen");
            return View(exam);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Exam/{id}";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ViewBag.ErrorMessage = "No tienes permisos para ver este examen.";
                return View();
            }
            if (!response.IsSuccessStatusCode) return NotFound();
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<Exam>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(apiResponse?.Data);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Exam/{id}";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ViewBag.ErrorMessage = "No tienes permisos para eliminar este examen.";
                return View();
            }
            if (!response.IsSuccessStatusCode) return NotFound();
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<Exam>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(apiResponse?.Data);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Exam/{id}";
            using var httpClient = CreateHttpClientWithJwt();
            var response = await httpClient.DeleteAsync(apiUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar exámenes.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}