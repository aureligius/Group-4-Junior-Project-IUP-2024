using System.ComponentModel.DataAnnotations;
using CarbonKickers.Application.Locations;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/places")]
public sealed class PlacesController : ControllerBase
{
    private readonly IPlaceSearchService _placeSearchService;

    public PlacesController(IPlaceSearchService placeSearchService)
    {
        _placeSearchService = placeSearchService;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<PlaceSuggestionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlaceSuggestionResponse>>> Search(
        [FromQuery, Required, StringLength(200)] string query
    )
    {
        try
        {
            var places = await _placeSearchService.SearchAsync(query);
            return Ok(places);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Unable to search for places right now." }
            );
        }
    }
}
