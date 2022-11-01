using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitmapNet;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands;

public class AddPhotosCommand : IRequest<AddPhotosCommandResult>
{
    public int DogId { get; init; }
    public ICollection<string> PhotosPath { get; init; }
}

public class AddPhotosCommandResult
{
    public bool AddingIsSuccessful { get; init; }
}

internal class AddPhotosCommandHandler : IRequestHandler<AddPhotosCommand, AddPhotosCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public AddPhotosCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<AddPhotosCommandResult> Handle(AddPhotosCommand request, CancellationToken cancellationToken = default)
    {
        foreach (var photoPath in request.PhotosPath)
        {
            var dogPhoto = new Image
            {
                DogId = request.DogId,
                PhotoPath = photoPath
            };
            await _dbContext.AddAsync(dogPhoto, cancellationToken);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dog = await _dbContext.Doges.FirstAsync(d => d.Id == request.DogId);
        dog.LastUpdate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddPhotosCommandResult
        {
            AddingIsSuccessful = true
        };
    }
}