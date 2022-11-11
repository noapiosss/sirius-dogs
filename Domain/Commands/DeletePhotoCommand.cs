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

public class DeletePhotoCommand : IRequest<DeletePhotoCommandResult>
{
    public int DogId { get; init; }
    public string PhotoPath { get; init; }
    public string RootPath { get; init; }
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

        File.Delete($"{request.RootPath}{request.PhotoPath.Replace("/","\\")}");
        
        return new DeletePhotoCommandResult
        {
            PhotoIsDeleted = true
        };
    }
}