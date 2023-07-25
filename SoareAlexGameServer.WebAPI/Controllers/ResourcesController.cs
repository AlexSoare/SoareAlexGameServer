using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoareAlexGameServer.WebAPI.Models.Resources;

namespace SoareAlexWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResourcesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("UpdateResources")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateResource(UpdateResources.UpdateResources_QueryRequest query)
        {
            UpdateResources.QueryResponse response = await _mediator.Send(query);
            return StatusCode((int)response.Status, response);
        }

        [HttpGet("GetResource")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResource([FromQuery]GetResource.GetResource_QueryRequest query)
        {
            GetResource.QueryResponse response = await _mediator.Send(query);
            return StatusCode((int)response.Status, response);
        }
    }
}
