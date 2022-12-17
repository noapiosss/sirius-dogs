using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;
using Contracts.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class DeletePhotoCommand : IRequest<DeletePhotoCommandResult>
    {
        public int DogId { get; init; }
        public string PhotoPath { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class DeletePhotoCommandResult
    {
        public bool PhotoIsDeleted { get; init; }
        public string Comment { get; init; }
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
            if (!await _dbContext.Doges.AnyAsync(d => d.Id == request.DogId, cancellationToken))
            {
                return new DeletePhotoCommandResult
                {
                    PhotoIsDeleted = false,
                    Comment = "Dog not found"
                };
            }

            Image photo = new()
            {
                DogId = request.DogId,
                PhotoPath = request.PhotoPath
            };

            _ = _dbContext.Images.Attach(photo);
            _ = _dbContext.Images.Remove(photo);

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeletePhotoCommandResult
            {
                PhotoIsDeleted = true
            };
        }
    }
}