using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api")]
    public class WentHomeController : BaseController
    {
        private readonly IMediator _mediator;

        public WentHomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{dogId}/wenthome")]
        public async Task<IActionResult> WentHome([FromRoute] int dogId, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            DogWentHomeCommand wentHomeCommand = new()
            {
                DogId = dogId,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value
            };

            _ = await _mediator.Send(wentHomeCommand, cancellationToken);

            return Ok();
        }

        [HttpPost("{dogId}/backtoshelter")]
        public async Task<IActionResult> BackToShelter([FromRoute] int dogId, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            DogBackToShelterCommand backToShelterCommand = new()
            {
                DogId = dogId,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value
            };

            _ = await _mediator.Send(backToShelterCommand, cancellationToken);

            return Ok();
        }
    }
}