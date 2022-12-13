using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Http;

using Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/birthdates")]
    public class BirthDatesController : BaseController
    {
        private readonly IMediator _mediator;

        public BirthDatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("shelter")]
        public async Task<IActionResult> GetBirthDatesShelter(CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            GetDogsBirthDatesQuery getDogsBirthDatesQuery = new()
            {
                WentHome = false
            };

            GetDogsBirthDatesQueryResult getDogBirthDatesQueryResult = await _mediator.Send(getDogsBirthDatesQuery, cancellationToken);
            GetBirthDatesResponse response = new()
            {
                BirthDates = getDogBirthDatesQueryResult.DogsBirthDates
            };

            return Ok(response);
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetBirthDatesHome(CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            GetDogsBirthDatesQuery getDogsBirthDatesQuery = new()
            {
                WentHome = true
            };

            GetDogsBirthDatesQueryResult getDogBirthDatesQueryResult = await _mediator.Send(getDogsBirthDatesQuery, cancellationToken);
            GetBirthDatesResponse response = new()
            {
                BirthDates = getDogBirthDatesQueryResult.DogsBirthDates
            };

            return Ok(response);
        }
    }
}