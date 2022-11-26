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

    public IActionResult Index(ICollection<Dog> dogs, CancellationToken cancellationToken = default)
    {
        return View(dogs);
    }
    public async Task<IActionResult> Shelter(CancellationToken cancellationToken = default)
    {
        var query = new GetShelterDogsQuery { };
        var result = await _mediator.Send(query, cancellationToken);

        return View("Index", result.Dogs);
    }
    public async Task<IActionResult> Home(CancellationToken cancellationToken = default)
    {
        var query = new GetHomeDogsQuery { };
        var result = await _mediator.Send(query, cancellationToken);

        return View("Index", result.Dogs);
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromForm] string something, string searchRequest, int filterAge, int filterRow, int filterEnclosure, CancellationToken cancellationToken = default)
    {
        var query = new SearchDogQuery
        {
            SearchRequest = searchRequest,
            MaxAge = filterAge,
            Row = filterRow,
            Enclosure = filterEnclosure,
            WentHome = Request.Headers["Referer"].ToString().Split("/").Last() == "Home" ? true : false
        };

        var result = await _mediator.Send(query, cancellationToken);

        return View(result.Dogs);
    }

    public IActionResult Create()
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Dog dog, IFormFile croppedImage, List<IFormFile> allPhotos, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

        if (!ModelState.IsValid)
        {
            return View(dog);
        }

        var command = new AddDogCommand
        {
            Name = dog.Name,
            Breed = dog.Breed,
            Size = dog.Size,
            BirthDate = dog.BirthDate,
            About = dog.About,
            Row = dog.Row,
            Enclosure = dog.Enclosure,
            RootPath = _environment.WebRootPath,
            UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
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
                    RootPath = _environment.WebRootPath,
                    UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
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
                        RootPath = _environment.WebRootPath,
                        UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    };

                    await _mediator.Send(addPhotoCommand, cancellationToken);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

        var query = new GetDogByIdQuery { DogId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return View(result.Dog);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteDog(int id, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

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
        var query = new GetDogByIdQuery { DogId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return View(result.Dog);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Dog dog, IFormFile croppedImage, List<IFormFile> allPhotos, [FromRoute] int id, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

        if (!ModelState.IsValid)
        {
            return View(dog);
        }

        dog.Id = id;
        var command = new EditDogCommand { Dog = dog };
        await _mediator.Send(command, cancellationToken);

        // ChangeTitlePhoto
        if (croppedImage != null)
        {
            using (var titlePhotoStream = croppedImage.OpenReadStream())
            {
                var addTitlePhotoCommand = new AddTitlePhotoCommand
                {
                    DogId = id,
                    TitlePhotoStream = titlePhotoStream,
                    RootPath = _environment.WebRootPath,
                    UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
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
                        DogId = id,
                        PhotoStream = photoStream,
                        RootPath = _environment.WebRootPath,
                        UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    };

                    await _mediator.Send(addPhotoCommand, cancellationToken);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
        {
            return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
        }

        var query = new GetDogByIdQuery { DogId = id };
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