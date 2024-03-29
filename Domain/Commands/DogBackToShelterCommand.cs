using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;
using Contracts.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class DogBackToShelterCommand : IRequest<DogBackToShelterCommandResult>
    {
        public int DogId { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class DogBackToShelterCommandResult
    {
        public bool StatusIsChanged { get; init; }
        public string Comment { get; init; }
    }

    internal class DogBackToShelterCommandHandler : IRequestHandler<DogBackToShelterCommand, DogBackToShelterCommandResult>
    {
        private readonly DogesDbContext _dbContext;

        public DogBackToShelterCommandHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<DogBackToShelterCommandResult> Handle(DogBackToShelterCommand request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.UpdatedBy))
            {
                return new DogBackToShelterCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Unauthorized"
                };
            }

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            if (dog == null)
            {
                return new DogBackToShelterCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Dog not found"
                };
            }

            if (!dog.WentHome)
            {
                return new DogBackToShelterCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Dog is already in shelter"
                };
            }

            dog.WentHome = false;
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new DogBackToShelterCommandResult
            {
                StatusIsChanged = true
            };
        }
    }
}