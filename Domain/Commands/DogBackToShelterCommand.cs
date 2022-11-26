using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands;

public class DogBackToShelterCommand : IRequest<DogBackToShelterCommandResult>
{
    public int DogId { get; init; }
}

public class DogBackToShelterCommandResult
{
    public bool StatusIsChanged { get; init; }
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
        var dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

        dog.WentHome = false;

        _dbContext.Doges.Update(dog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DogBackToShelterCommandResult
        {
            StatusIsChanged = true
        };
    }
}