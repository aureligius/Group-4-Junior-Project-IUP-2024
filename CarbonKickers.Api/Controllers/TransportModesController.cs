using CarbonKickers.Infrastructure.Services;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Route("api/transport-modes")]
public sealed class TransportModesController : ControllerBase
{
    private readonly TransportService _transportService;

    public TransportModesController(TransportService transportService)
    {
        _transportService = transportService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransportModeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransportModeResponse>>> GetAll()
    {
        var modes = await _transportService.GetAllModesAsync();
        return Ok(modes);
    }
}