using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using MediatR;

namespace Domain.Commands;

public class DeleteDogCommand : IRequest<DeleteDogCommandResult>
{
    public int DogId { get; init; }
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
        var dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

        _dbContext.Doges.Attach(dog);
        _dbContext.Doges.Remove(dog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteDogCommandResult
        {
            DeletingIsSuccessful = true
        };
    }
}