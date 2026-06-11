using Microsoft.AspNetCore.Mvc;

namespace SistemaAN.Api.Controllers;

/// <summary>
/// Endpoint técnico (não-negócio) que expõe metadados da API. Serve para
/// validar que toda a fundação (pipeline, DI, Swagger) está operacional.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MetaController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public MetaController(IWebHostEnvironment environment) => _environment = environment;

    /// <summary>Retorna nome, versão e ambiente da API.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        name = "SistemaAN API",
        version = "0.1.0",
        environment = _environment.EnvironmentName,
        timestampUtc = DateTimeOffset.UtcNow,
    });
}
