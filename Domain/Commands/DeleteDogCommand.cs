using System.Threading;
using System.Threading.Tasks;

using Domain.Database;
using Contracts.Database;

using MediatR;

namespace Domain.Commands
{
    public class DeleteDogCommand : IRequest<DeleteDogCommandResult>
    {
        public int DogId { get; init; }
    }

    public class DeleteDogCommandResult
    {
        public bool DeletingIsSuccessful { get; init; }
        public string Comment { get; init; }
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
            Dog dog = new() { Id = request.DogId };

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