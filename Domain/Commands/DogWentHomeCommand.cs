using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class DogWentHomeCommand : IRequest<DogWentHomeCommandResult>
    {
        public int DogId { get; init; }
    }

    public class DogWentHomeCommandResult
    {
        public bool StatusIsChanged { get; init; }
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
            Contracts.Database.Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            dog.WentHome = true;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new DogWentHomeCommandResult
            {
                StatusIsChanged = true
            };
        }
    }
}