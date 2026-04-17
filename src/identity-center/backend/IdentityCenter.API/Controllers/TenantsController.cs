using IdentityCenter.Application.DTOs.Tenants;
using IdentityCenter.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TenantsController : ControllerBase
{
    private readonly TenantService _service;
    public TenantsController(TenantService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTenantRequest request, CancellationToken ct)
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

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarTenantRequest request, CancellationToken ct)
    {
        var result = await _service.ActualizarAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/suspender")]
    public async Task<IActionResult> Suspender(Guid id, CancellationToken ct)
    {
        await _service.SuspenderAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        await _service.ActivarAsync(id, ct);
        return NoContent();
    }
}
