using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Api.Models;

using Contracts.Database;
using Contracts.Http;

using Domain.Commands;
using Domain.Queries;

using MediatR;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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