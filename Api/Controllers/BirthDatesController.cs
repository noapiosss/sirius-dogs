using System.Threading;
using System.Threading.Tasks;

using Contracts.Http;

using Domain.Queries;

using MediatR;

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