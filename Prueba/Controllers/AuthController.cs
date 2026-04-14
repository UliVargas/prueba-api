using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba.Contracts;
using Prueba.Data;
using Prueba.Security;

namespace Prueba.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest("Correo y contraseña son obligatorios.");
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Correo == correo);

        if (user is null || !PasswordHasher.VerifyPassword(request.Contrasena, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized("Credenciales inválidas.");
        }

        return Ok(new
        {
            message = "Inicio de sesión exitoso.",
            user = new UserResponse(user.Id, user.Nombre, user.Correo)
        });
    }
}
