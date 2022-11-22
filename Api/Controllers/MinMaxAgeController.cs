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

[Route("api/age")]
public class MinMaxAgeController : BaseController
{
    private readonly ILogger<DogsController> _logger;
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public MinMaxAgeController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment)
    {
        _logger = logger;
        _mediator = mediator;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> GetMinMaxAge(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMinMaxAgeQuery{}, cancellationToken);

        return Ok(result);
    } 
}
