using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Api.Services.Interfaces;

using Contracts.Http;

using Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/photos")]
    public class PhotoController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICloudStorage _googleStorage;

        public PhotoController(IMediator mediator, ICloudStorage googleStorage)
        {
            _mediator = mediator;
            _googleStorage = googleStorage;
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto([FromForm] int dogId, IFormFile photo, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            // test google bucket
            string photoUrl = await _googleStorage.UploadFileAsync(photo, dogId.ToString(), Guid.NewGuid().ToString() + ".jpeg");

            AddPhotoCommand command = new()
            {
                DogId = dogId,
                PhotoUrl = photoUrl,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            };

            AddPhotoCommandResult response = await _mediator.Send(command, cancellationToken);

            return Ok(photoUrl);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoRequest request, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            await _googleStorage.DeleteFileAsync(request.DogId.ToString(), request.PhotoUrl);

            DeletePhotoCommand query = new()
            {
                DogId = request.DogId,
                PhotoPath = request.PhotoUrl,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            };

            _ = await _mediator.Send(query, cancellationToken);

            return Ok();
        }
    }
}