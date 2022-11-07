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

namespace Api.Controllers;

public class DogsController : Controller
{
    private readonly ILogger<DogsController> _logger;
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public DogsController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment)
    {
        _logger = logger;
        _mediator = mediator;
        _environment = environment;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var query = new GetAllDogsQuery{};
        var result = await _mediator.Send(query, cancellationToken);
        
        return View(result.Dogs);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromForm] string searchRequest, CancellationToken cancellationToken = default)
    {
        var query = new SearchGodQuery{SearchRequest = searchRequest};
        var result = await _mediator.Send(query, cancellationToken);
        
        return View(result.Dogs);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Dog dog, IFormFile croppedImage, List<IFormFile> allPhotos, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(dog);
        }

        var path = _environment.WebRootPath;

        var command = new AddDogCommand
        {
            Name = dog.Name,
            Breed = dog.Breed,
            Size = dog.Size,
            BirthDate = dog.BirthDate,
            About = dog.About,
            Row = dog.Row,
            Enclosure = dog.Enclosure,
            RootPath = path
        };

        var response = await _mediator.Send(command, cancellationToken);

        // AddTitlePhoto
        if (croppedImage != null)
        {
            using (var titlePhotoStream = croppedImage.OpenReadStream())
            {
                var addTitlePhotoCommand = new AddTitlePhotoCommand
                {
                    DogId = response.Dog.Id,
                    TitlePhotoStream = titlePhotoStream,
                    RootPath = path
                };

                await _mediator.Send(addTitlePhotoCommand, cancellationToken);
            }
        }

        // AddOthersPhotos
        if (allPhotos != null)
        {
            foreach (var photo in allPhotos)
            {
                using (var photoStream = photo.OpenReadStream())
                {
                    var addPhotoCommand = new AddPhotoCommand
                    {
                        DogId = response.Dog.Id,
                        PhotoStream = photoStream,
                        RootPath = path
                    };

                    await _mediator.Send(addPhotoCommand, cancellationToken);
                }
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
        var command = new DeleteDogCommand
        {
            DogId = id,
            RootPath = _environment.WebRootPath
        };
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
        result.Dog.BirthDate = result.Dog.BirthDate.ToLocalTime();
        return View(result.Dog);
    }    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
