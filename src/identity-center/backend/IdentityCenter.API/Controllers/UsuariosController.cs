using IdentityCenter.Application.DTOs.Usuarios;
using IdentityCenter.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController : ControllerBase
{
    private readonly UsuarioService _service;
    public UsuariosController(UsuarioService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request, CancellationToken ct)
    {
        var result = await _service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("por-tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await _service.GetByTenantAsync(tenantId, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarUsuarioRequest request, CancellationToken ct)
    {
        var result = await _service.ActualizarAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cambiar-password")]
    public async Task<IActionResult> CambiarPassword(Guid id, [FromBody] CambiarPasswordRequest request, CancellationToken ct)
    {
        await _service.CambiarPasswordAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/bloquear")]
    public async Task<IActionResult> Bloquear(Guid id, CancellationToken ct)
    {
        await _service.BloquearAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        await _service.ActivarAsync(id, ct);
        return NoContent();
    }
}
