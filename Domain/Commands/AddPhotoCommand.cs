using System;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class AddPhotoCommand : IRequest<AddPhotoCommandResult>
    {
        public int DogId { get; init; }
        public string PhotoUrl { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class AddPhotoCommandResult
    {
        public string PhotoUrl { get; init; }
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
            Image image = new()
            {
                DogId = request.DogId,
                PhotoPath = request.PhotoUrl
            };

            _ = await _dbContext.Images.AddAsync(image, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddPhotoCommandResult
            {
                PhotoUrl = request.PhotoUrl
            };
        }
    }
}