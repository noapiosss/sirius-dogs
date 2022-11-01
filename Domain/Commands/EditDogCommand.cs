using System;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands;

public class EditDogCommand : IRequest<EditDogCommandResult>
{
    public Dog Dog { get; init; }
}

public class EditDogCommandResult
{
    public bool EditingIsSuccessful { get; init; }
}

internal class EditDogCommandHandler : IRequestHandler<EditDogCommand, EditDogCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public EditDogCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<EditDogCommandResult> Handle(EditDogCommand request, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Doges.AnyAsync(d => d.Id == request.Dog.Id))
        {
            return new EditDogCommandResult
            {
                EditingIsSuccessful = false
            };
        }

        request.Dog.LastUpdate = DateTime.UtcNow;
        _dbContext.Doges.Update(request.Dog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EditDogCommandResult
        {
            EditingIsSuccessful = true
        };
    }
}