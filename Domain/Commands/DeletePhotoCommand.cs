using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands;

public class DeletePhotoCommand : IRequest<DeletePhotoCommandResult>
{
    public int DogId { get; init; }
    public string PhotoPath { get; init; }
    public string RootPath { get; init; }
    public string UpdatedBy { get; init; }
}

public class DeletePhotoCommandResult
{
    public bool PhotoIsDeleted { get; init; }
}

internal class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand, DeletePhotoCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public DeletePhotoCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<DeletePhotoCommandResult> Handle(DeletePhotoCommand request, CancellationToken cancellationToken = default)
    {
        var photo = await _dbContext.Images.FirstOrDefaultAsync(i => i.DogId == request.DogId && i.PhotoPath == request.PhotoPath, cancellationToken);

        _dbContext.Remove(photo);
        _dbContext.SaveChanges();

        File.Delete($"{request.RootPath}{request.PhotoPath.Replace("/", "\\")}");

        var dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);
        dog.LastUpdate = DateTime.UtcNow;
        dog.UpdatedBy = request.UpdatedBy;

        _dbContext.Doges.Update(dog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeletePhotoCommandResult
        {
            PhotoIsDeleted = true
        };
    }
}