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

namespace Api.Controllers;

public class DogsController : Controller
{
    private readonly ILogger<DogsController> _logger;
    private readonly IMediator _mediator;

    public DogsController(ILogger<DogsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var query = new GetAllDogsQuery{};
        var result = await _mediator.Send(query, cancellationToken);
        
        return View(result.Dogs);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Dog dog, IFormFile croppedImage, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(dog);
        }
        
        var command = new AddDogCommand
        {
            Name = dog.Name,
            Breed = dog.Breed,
            Size = dog.Size,
            Age = dog.Age,
            About = dog.About,
            Row = dog.Row,
            Enclosure = dog.Enclosure
        };
        
        var response = await _mediator.Send(command, cancellationToken);

        if (croppedImage != null)
        {   
            Directory.CreateDirectory($"wwwroot/images/{response.Dog.Id}");
            using (var fileStream = new FileStream($"wwwroot/{response.Dog.TitlePhoto}", FileMode.Create))
            {
                croppedImage.CopyTo(fileStream);
            }
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var query = new GetDogByIdQuery{DogId = id};
        var result = await _mediator.Send(query, cancellationToken);
        return View(result.Dog);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteDog(int id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteDogCommand{DogId = id};
        var result = await _mediator.Send(command, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var query = new GetDogByIdQuery{DogId = id};
        var result = await _mediator.Send(query, cancellationToken);
        return View(result.Dog);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Dog dog, [FromRoute] int id, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(dog);
        }

        dog.Id = id;
        var command = new EditDogCommand{ Dog = dog };
        await _mediator.Send(command, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var query = new GetDogByIdQuery{DogId = id};
        var result = await _mediator.Send(query, cancellationToken);
        return View(result.Dog);
    }    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
