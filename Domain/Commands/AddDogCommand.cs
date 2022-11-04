using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands;

public class AddDogCommand : IRequest<AddDogCommandResult>
{
    public string Name { get; init; }
    public string Breed { get; init; }
    public string Size { get; init; }
    public int Age { get; init; }
    public string About { get; init; }
    public int Row { get; init; }
    public int Enclosure { get; init; }
    public string RootPath { get; init; }
}

public class AddDogCommandResult
{
    public Dog Dog { get; init; }
}

internal class AddDogCommandHandler : IRequestHandler<AddDogCommand, AddDogCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public AddDogCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<AddDogCommandResult> Handle(AddDogCommand request, CancellationToken cancellationToken = default)
    {
        var dog = new Dog
        {
            Name = request.Name,
            Breed = request.Breed,
            Size = request.Size,
            Age = request.Age,
            About = request.About,
            Row = request.Row,
            Enclosure = request.Enclosure,

            LastUpdate = DateTime.UtcNow
        };
        
        await _dbContext.AddAsync(dog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        Directory.CreateDirectory($"{request.RootPath}\\images\\{dog.Id}");            

        return new AddDogCommandResult
        {
            Dog = dog
        };
    }
}