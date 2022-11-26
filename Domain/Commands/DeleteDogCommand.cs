using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class DeleteDogCommand : IRequest<DeleteDogCommandResult>
    {
        public int DogId { get; init; }
        public string RootPath { get; init; }
    }

    public class DeleteDogCommandResult
    {
        public bool DeletingIsSuccessful { get; init; }
    }

    internal class DeleteDogCommandHandler : IRequestHandler<DeleteDogCommand, DeleteDogCommandResult>
    {
        private readonly DogesDbContext _dbContext;

        public DeleteDogCommandHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<DeleteDogCommandResult> Handle(DeleteDogCommand request, CancellationToken cancellationToken = default)
        {
            Contracts.Database.Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

            Directory.Delete($"{request.RootPath}\\images\\{request.DogId}", true);

            _ = _dbContext.Doges.Attach(dog);
            _ = _dbContext.Doges.Remove(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteDogCommandResult
            {
                DeletingIsSuccessful = true
            };
        }
    }
}