using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;
using Contracts.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class DogWentHomeCommand : IRequest<DogWentHomeCommandResult>
    {
        public int DogId { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class DogWentHomeCommandResult
    {
        public bool StatusIsChanged { get; init; }
        public string Comment { get; init; }
    }

    internal class DogWentHomeCommandHandler : IRequestHandler<DogWentHomeCommand, DogWentHomeCommandResult>
    {
        private readonly DogesDbContext _dbContext;

        public DogWentHomeCommandHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<DogWentHomeCommandResult> Handle(DogWentHomeCommand request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.UpdatedBy))
            {
                return new DogWentHomeCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Unauthorized"
                };
            }

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            if (dog == null)
            {
                return new DogWentHomeCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Dog not found"
                };
            }

            if (dog.WentHome)
            {
                return new DogWentHomeCommandResult
                {
                    StatusIsChanged = false,
                    Comment = "Dog is already at home"
                };
            }

            dog.WentHome = true;
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new DogWentHomeCommandResult
            {
                StatusIsChanged = true
            };
        }
    }
}