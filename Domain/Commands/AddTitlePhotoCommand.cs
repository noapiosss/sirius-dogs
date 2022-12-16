using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;
using Contracts.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class AddTitlePhotoCommand : IRequest<AddTitlePhotoCommandResult>
    {
        public int DogId { get; init; }
        public string PhotoUrl { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class AddTitlePhotoCommandResult
    {
        public string PhotoUrl { get; init; }
        public string Comment { get; init; }
    }

    internal class AddTitlePhotoCommandHandler : IRequestHandler<AddTitlePhotoCommand, AddTitlePhotoCommandResult>
    {
        private readonly DogesDbContext _dbContext;

        public AddTitlePhotoCommandHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<AddTitlePhotoCommandResult> Handle(AddTitlePhotoCommand request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.UpdatedBy))
            {
                return new AddTitlePhotoCommandResult
                {
                    PhotoUrl = null,
                    Comment = "Unauthorized"
                };
            }

            if (string.IsNullOrEmpty(request.PhotoUrl))
            {
                return new AddTitlePhotoCommandResult
                {
                    PhotoUrl = null,
                    Comment = "Invalid photo URL"
                };
            }

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            dog.TitlePhoto = request.PhotoUrl;
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddTitlePhotoCommandResult
            {
                PhotoUrl = request.PhotoUrl
            };
        }
    }
}