# SystemExamWeb

## Centralización del manejo de JWT en controladores

Para consumir la API protegida con JWT desde los controladores de ASP.NET MVC, se implementó el patrón de un **controlador base** llamado `JwtControllerBase`.

### ¿Cómo funciona?
- `JwtControllerBase` provee el método protegido `CreateHttpClientWithJwt()`, que crea un `HttpClient` con el token JWT (obtenido de la cookie) ya configurado en la cabecera `Authorization`.
- Los controladores que necesiten autenticación (por ejemplo, `UsersController`, `CategoryController`, `ExamsController`) deben **heredar de `JwtControllerBase`**.
- En cada método que haga peticiones a la API, simplemente usa:
  ```csharp
  using var httpClient = CreateHttpClientWithJwt();
  ```

### Ventajas
- **Evita duplicación de código**: No necesitas repetir el código para obtener el token y configurar el header en cada método.
- **Más seguro**: Menos riesgo de olvidar agregar el token.
- **Fácil de mantener**: Si cambia la forma de obtener el token, solo se modifica en un lugar.

### Ejemplo de uso
```csharp
public class UsersController : JwtControllerBase
{
    public async Task<IActionResult> Index()
    {
        using var httpClient = CreateHttpClientWithJwt();
        var response = await httpClient.GetAsync("https://api/Users");
        // ...
    }
}
```

---

Para más detalles, revisa el archivo `JwtControllerBase.cs` y los controladores que lo heredan. 