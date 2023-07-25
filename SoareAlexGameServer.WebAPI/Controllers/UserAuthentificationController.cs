using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoareAlexGameServer.WebAPI.Models.UserAuthentification;
using System.Net;

namespace SoareAlexWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthentificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserAuthentificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Login")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(Login.Login_QueryRequest query)
        {
            Login.QueryResponse response = await _mediator.Send(query);
            return StatusCode((int)response.Status, response);
        }
    }
}
