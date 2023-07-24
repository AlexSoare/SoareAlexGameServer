using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoareAlexGameServer.WebAPI.Models.Gifts;
using SoareAlexGameServer.WebAPI.Models.Resources;
using System.Data;
using System.Net;

namespace SoareAlexWebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class GiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("SendGift")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        public async Task<IActionResult> SendGift(SendGift.QueryRequest query)
        {
            SendGift.QueryResponse response = await _mediator.Send(query);
            return StatusCode((int)response.Status, response);
        }
    }
}
