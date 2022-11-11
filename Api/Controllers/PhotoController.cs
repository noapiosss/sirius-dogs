using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Domain.Queries;
using System.Linq;
using Contracts.Database;
using Contracts.Http;
using Domain.Commands;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace Api.Controllers;

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
        using (var photoStream = photo.OpenReadStream())
        {
            var addPhotoCommand = new AddPhotoCommand
            {
                DogId = dogId,
                PhotoStream = photoStream,
                RootPath = _environment.WebRootPath,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            };

            var result = await _mediator.Send(addPhotoCommand, cancellationToken);
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

        var query = new DeletePhotoCommand
        {
            DogId = request.DogId,
            PhotoPath = request.PhotoPath,
            RootPath = _environment.WebRootPath,
            UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
        };
        
        await _mediator.Send(query, cancellationToken);

        return Ok();
    }    
}
