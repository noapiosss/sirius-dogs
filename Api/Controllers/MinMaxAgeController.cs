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
        var result = await _mediator.Send(new GetMinMaxAgeQuery { }, cancellationToken);

        return Ok(result);
    }
}