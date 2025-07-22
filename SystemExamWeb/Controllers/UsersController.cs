 using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SystemExamWeb.Helpers;
using SystemExamWeb.Models;
using SystemExamWeb.Models.ViewModels;
using SystemExamWeb.Responses;

/// <summary>
/// Controlador para gestión de usuarios. Hereda de JwtControllerBase para centralizar el manejo del JWT en las peticiones HTTP.
/// Usa CreateHttpClientWithJwt() para obtener un HttpClient con el token JWT automáticamente configurado.
/// </summary>
public class UsersController : JwtControllerBase
{
    private readonly string _apiBaseUrl;
    //private readonly IHttpClientFactory _httpClientFactory;

    public UsersController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
     
    }

    public async Task<IActionResult> Index()
    {
        var apiUrl = $"{_apiBaseUrl}/api/Users";
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync(apiUrl);
        if (!response.IsSuccessStatusCode) return View(Enumerable.Empty<User>());
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<User>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data ?? new List<User>());
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(UserViewModel user)
    {
        if (!ModelState.IsValid) return View(user);
        var apiUrl = $"{_apiBaseUrl}/api/Users";
        using var httpClient = CreateHttpClientWithJwt();
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(apiUrl, content);
        if (response.IsSuccessStatusCode) return RedirectToAction("Index");
        ModelState.AddModelError("", "Error al crear usuario");
        return View(user);
    }

    public async Task<IActionResult> Edit(Guid id)
    {

        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Users/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(User user)
    {
        if (!ModelState.IsValid) return View(user);
        using var httpClient = CreateHttpClientWithJwt();
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync($"{_apiBaseUrl}/api/Users/{user.Id}", content);
        if (response.IsSuccessStatusCode) return RedirectToAction("Index");
        ModelState.AddModelError("", "Error al editar usuario");
        return View(user);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Users/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Users/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(apiResponse?.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {

        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/Users/{id}");
        return RedirectToAction("Index");
    }
}
