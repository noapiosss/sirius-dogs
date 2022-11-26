using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Http;

using Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/photos")]
    public class PhotoController : BaseController
    {
        private readonly ILogger<DogsController> _logger;
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;

        public PhotoController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment)
        {
            _logger = logger;
            _mediator = mediator;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto([FromForm] int dogId, IFormFile photo, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            // AddPhoto
            AddPhotosResponse response;
            using (System.IO.Stream photoStream = photo.OpenReadStream())
            {
                AddPhotoCommand addPhotoCommand = new()
                {
                    DogId = dogId,
                    PhotoStream = photoStream,
                    RootPath = _environment.WebRootPath,
                    UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                };

                AddPhotoCommandResult result = await _mediator.Send(addPhotoCommand, cancellationToken);
                response = new AddPhotosResponse
                {
                    PhotoPath = result.PhotoPath
                };
            }

            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoRequest request, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            DeletePhotoCommand query = new()
            {
                DogId = request.DogId,
                PhotoPath = request.PhotoPath,
                RootPath = _environment.WebRootPath,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            };

            _ = await _mediator.Send(query, cancellationToken);

            return Ok();
        }
    }
}