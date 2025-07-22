using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using SystemExamWeb.Helpers;

public abstract class JwtControllerBase : Controller
{
    protected HttpClient CreateHttpClientWithJwt()
    {
        var token = TokenHelper.GetJwtToken(Request);
        var client = new HttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }
} 