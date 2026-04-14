using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba.Contracts;
using Prueba.Data;
using Prueba.Models;
using Prueba.Security;

namespace Prueba.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await dbContext.Users
            .OrderBy(u => u.Id)
            .Select(u => new UserResponse(u.Id, u.Nombre, u.Correo))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => new UserResponse(u.Id, u.Nombre, u.Correo))
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest("Nombre, correo y contraseña son obligatorios.");
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(u => u.Correo == correo);
        if (exists)
        {
            return Conflict("Ya existe un usuario con ese correo.");
        }

        var (hash, salt) = PasswordHasher.HashPassword(request.Contrasena);

        var user = new User
        {
            Nombre = request.Nombre.Trim(),
            Correo = correo,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var response = new UserResponse(user.Id, user.Nombre, user.Correo);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Correo))
        {
            return BadRequest("Nombre y correo son obligatorios.");
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(u => u.Correo == correo && u.Id != id);
        if (exists)
        {
            return Conflict("Ya existe un usuario con ese correo.");
        }

        user.Nombre = request.Nombre.Trim();
        user.Correo = correo;

        if (!string.IsNullOrWhiteSpace(request.Contrasena))
        {
            var (hash, salt) = PasswordHasher.HashPassword(request.Contrasena);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        await dbContext.SaveChangesAsync();

        return Ok(new UserResponse(user.Id, user.Nombre, user.Correo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

}
