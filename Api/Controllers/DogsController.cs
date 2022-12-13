using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Api.Models;
using Api.Services.Interfaces;

using Contracts.Database;

using Domain.Commands;
using Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    public class DogsController : Controller
    {
        private readonly ILogger<DogsController> _logger;
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;
        private readonly ICloudStorage _googleStorage;

        public DogsController(ILogger<DogsController> logger, IMediator mediator, IWebHostEnvironment environment, ICloudStorage googleStorage)
        {
            _logger = logger;
            _mediator = mediator;
            _environment = environment;
            _googleStorage = googleStorage;
        }

        public IActionResult Index(ICollection<Dog> dogs)
        {
            return View(dogs);
        }

        public async Task<IActionResult> Shelter(CancellationToken cancellationToken = default)
        {
            GetShelterDogsQuery query = new() { };
            GetShelterDogsQueryResult result = await _mediator.Send(query, cancellationToken);

            return View("Index", result.Dogs);
        }
        public async Task<IActionResult> Home(CancellationToken cancellationToken = default)
        {
            GetHomeDogsQuery query = new() { };
            GetHomeDogsQueryResult result = await _mediator.Send(query, cancellationToken);

            return View("Index", result.Dogs);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromForm] string something, string searchRequest, int filterAge, int filterRow, int filterEnclosure, CancellationToken cancellationToken = default)
        {
            SearchDogQuery query = new()
            {
                SearchRequest = searchRequest,
                MaxAge = filterAge,
                Row = filterRow,
                Enclosure = filterEnclosure,
                WentHome = Request.Headers["Referer"]
                    .ToString()
                    .Split("/")
                    .Last() == "Home"
            };

            SearchDogQueryResult result = await _mediator.Send(query, cancellationToken);

            return View(result.Dogs);
        }

        public IActionResult Create()
        {
            return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null
                ? Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}")
                : View();
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

            AddDogCommand addDogCommand = new()
            {
                Name = dog.Name,
                Breed = dog.Breed,
                Size = dog.Size,
                BirthDate = dog.BirthDate,
                About = dog.About,
                Row = dog.Row,
                Enclosure = dog.Enclosure,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            };

            AddDogCommandResult response = await _mediator.Send(addDogCommand, cancellationToken);

            // AddTitlePhoto
            if (croppedImage != null)
            {
                string titleUrl = await _googleStorage.UploadFileAsync(croppedImage, response.Dog.Id.ToString(), Guid.NewGuid().ToString() + ".jpeg");
                AddTitlePhotoCommand addTitlePhotoCommand = new()
                {
                    DogId = response.Dog.Id,
                    PhotoUrl = titleUrl,
                    UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                };

                _ = await _mediator.Send(addTitlePhotoCommand, cancellationToken);
            }

            // AddOthersPhotos
            if (allPhotos != null)
            {
                foreach (IFormFile photo in allPhotos)
                {
                    string photoUrl = await _googleStorage.UploadFileAsync(photo, response.Dog.Id.ToString(), Guid.NewGuid().ToString() + ".jpeg");
                    AddPhotoCommand addPhotoCommand = new()
                    {
                        DogId = response.Dog.Id,
                        PhotoUrl = photoUrl,
                        UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    };

                    _ = await _mediator.Send(addPhotoCommand, cancellationToken);
                }
            }

            return RedirectToAction(nameof(Shelter));
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            GetDogByIdQuery query = new() { DogId = id };
            GetDogByIdQueryResult result = await _mediator.Send(query, cancellationToken);
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

            DeleteDogCommand command = new()
            {
                DogId = id
            };

            _ = await _mediator.Send(command, cancellationToken);
            await _googleStorage.DeleteFolderAsync(id.ToString());

            return RedirectToAction(nameof(Shelter));
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            GetDogByIdQuery query = new() { DogId = id };
            GetDogByIdQueryResult result = await _mediator.Send(query, cancellationToken);
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
            EditDogCommand command = new()
            {
                Dog = dog,
                UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value
            };
            Dog editedDog = (await _mediator.Send(command, cancellationToken)).Dog;

            // ChangeTitlePhoto
            if (croppedImage != null)
            {
                await _googleStorage.DeleteFileAsync(id.ToString(), editedDog.TitlePhoto);
                string titleUrl = await _googleStorage.UploadFileAsync(croppedImage, id.ToString(), Guid.NewGuid().ToString() + ".jpeg");
                AddTitlePhotoCommand addTitlePhotoCommand = new()
                {
                    DogId = id,
                    PhotoUrl = titleUrl,
                    UpdatedBy = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                };

                _ = await _mediator.Send(addTitlePhotoCommand, cancellationToken);
            }

            bool dogWentHome = (await _mediator.Send(new GetDogByIdQuery { DogId = dog.Id }, cancellationToken)).Dog.WentHome;
            return dogWentHome ? RedirectToAction(nameof(Home)) : RedirectToAction(nameof(Shelter));
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null)
            {
                return Redirect($"{Request.Headers["Origin"]}/Session/Signin?{Request.Path}");
            }

            GetDogByIdQuery query = new() { DogId = id };
            GetDogByIdQueryResult result = await _mediator.Send(query, cancellationToken);
            return View(result.Dog);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}