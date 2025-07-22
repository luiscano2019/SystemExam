using Microsoft.AspNetCore.Mvc;
using SystemExamWeb.Models;
using System.Text.Json;
using System.Text;
using SystemExamWeb.Helpers;
using SystemExamWeb.Responses;

public class CategoryController : JwtControllerBase
{
    private readonly string _apiBaseUrl;
    //private readonly IHttpClientFactory _httpClientFactory;

    public CategoryController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
        //_httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var apiUrl = $"{_apiBaseUrl}/api/Category";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync(apiUrl);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ViewBag.ErrorMessage = "No tienes permisos para ver las categorías.";
            return View(Enumerable.Empty<Category>());
        }
        if (!response.IsSuccessStatusCode) return View(Enumerable.Empty<Category>());
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Category>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data ?? new List<Category>());
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        var apiUrl = $"{_apiBaseUrl}/api/Category";
        using var httpClient = CreateHttpClientWithJwt();
        var json = JsonSerializer.Serialize(category);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(apiUrl, content);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError("", "No tienes permisos para crear categorías.");
            return View(category);
        }
        if (response.IsSuccessStatusCode) return RedirectToAction("Index");
        ModelState.AddModelError("", "Error al crear categoría");
        return View(category);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var apiUrl = $"{_apiBaseUrl}/api/Category/{id}";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync(apiUrl);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ViewBag.ErrorMessage = "No tienes permisos para editar esta categoría.";
            return View();
        }
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        var apiUrl = $"{_apiBaseUrl}/api/Category/{category.Id}";
        using var httpClient = CreateHttpClientWithJwt();
        var json = JsonSerializer.Serialize(category);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(apiUrl, content);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError("", "No tienes permisos para editar categorías.");
            return View(category);
        }
        if (response.IsSuccessStatusCode) return RedirectToAction("Index");
        ModelState.AddModelError("", "Error al editar categoría");
        return View(category);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var apiUrl = $"{_apiBaseUrl}/api/Category/{id}";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync(apiUrl);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ViewBag.ErrorMessage = "No tienes permisos para ver esta categoría.";
            return View();
        }
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var apiUrl = $"{_apiBaseUrl}/api/Category/{id}";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync(apiUrl);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ViewBag.ErrorMessage = "No tienes permisos para eliminar esta categoría.";
            return View();
        }
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var apiUrl = $"{_apiBaseUrl}/api/Category/{id}";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.DeleteAsync(apiUrl);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            TempData["ErrorMessage"] = "No tienes permisos para eliminar categorías.";
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }
}
