using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Domain.Database;
using System.IO;
using Contracts.Database;
using System.Linq;
using System;

namespace Domain.Commands;

public class AddPhotoCommand : IRequest<AddPhotoCommandResult>
{
    public int DogId { get; init; }
    public Stream PhotoStream { get; init; }
    public string RootPath { get; init; }
}

public class AddPhotoCommandResult
{
    public bool PhotoIsAdded { get; init; }
}

internal class AddPhotoCommandHandler : IRequestHandler<AddPhotoCommand, AddPhotoCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public AddPhotoCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<AddPhotoCommandResult> Handle(AddPhotoCommand request, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Images.AnyAsync(i => i.DogId == request.DogId, cancellationToken))
        {
            using (var fileStream = new FileStream($"{request.RootPath}\\wwwroot\\images\\{request.DogId}\\1.jpg", FileMode.Create))
            {
                request.PhotoStream.Seek(0, SeekOrigin.Begin);
                request.PhotoStream.CopyTo(fileStream);
            }

            var firstImage = new Image
            {
                DogId = request.DogId,
                PhotoPath = $"/images/{request.DogId}/1.jpg"
            };

            await _dbContext.AddAsync(firstImage, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddPhotoCommandResult
            {
                PhotoIsAdded = true
            };
        }

        var lastPhotoIndex = (await _dbContext.Images
            .Where(i => i.DogId == request.DogId)
            .Select(i => i.PhotoPath)
            .MaxAsync(cancellationToken))
            .Split('/').Last()
            .Split('.').First();
        
        using (var fileStream = new FileStream($"{request.RootPath}\\wwwroot\\images\\{request.DogId}\\{Int64.Parse(lastPhotoIndex)+1}.jpg", FileMode.Create))
        {
            request.PhotoStream.Seek(0, SeekOrigin.Begin);
            request.PhotoStream.CopyTo(fileStream);
        }


        var image = new Image
        {
            DogId = request.DogId,
            PhotoPath = $"/images/{request.DogId}/{Int64.Parse(lastPhotoIndex)+1}.jpg"
        };

        await _dbContext.AddAsync(image, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddPhotoCommandResult
        {
            PhotoIsAdded = true
        };
    }
}