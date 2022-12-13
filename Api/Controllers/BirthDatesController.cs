using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Http;

using Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/birthdates")]
    public class BirthDatesController : BaseController
    {
        private readonly ILogger<DogsController> _logger;
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;

        public BirthDatesController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment)
        {
            _logger = logger;
            _mediator = mediator;
            _environment = environment;
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