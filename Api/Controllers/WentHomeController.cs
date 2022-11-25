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

[Route("api")]
public class WentHomeController : BaseController
{
    private readonly ILogger<DogsController> _logger;
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public WentHomeController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment)
    {
        _logger = logger;
        _mediator = mediator;
        _environment = environment;
    }

    [HttpPost("{dogId}/wenthome")]
    public async Task<IActionResult> WentHome([FromRoute] int dogId, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }
        
        var wentHomeCommand = new DogWentHomeCommand
        {
            DogId = dogId
        };

        await _mediator.Send(wentHomeCommand, cancellationToken);

        return Ok();
    }

    [HttpPost("{dogId}/backtoshelter")]
    public async Task<IActionResult> BackToShelter([FromRoute] int dogId, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

        var backToShelterCommand = new DogBackToShelterCommand
        {
            DogId = dogId
        };

        await _mediator.Send(backToShelterCommand, cancellationToken);

        return Ok();
    }    
}
