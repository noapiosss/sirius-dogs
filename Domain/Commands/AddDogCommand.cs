using System;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

namespace Domain.Commands
{
    public class AddDogCommand : IRequest<AddDogCommandResult>
    {
        public string Name { get; init; }
        public string Breed { get; init; }
        public string Size { get; init; }
        public DateTime BirthDate { get; init; }
        public string About { get; init; }
        public int Row { get; init; }
        public int Enclosure { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class AddDogCommandResult
    {
        public Dog Dog { get; init; }
        public string Comment { get; init; }
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
            if (string.IsNullOrEmpty(request.UpdatedBy))
            {
                return new AddDogCommandResult
                {
                    Dog = null,
                    Comment = "Unauthorized"
                };
            }

            Dog dog = new()
            {
                Name = request.Name,
                Breed = request.Breed,
                Size = request.Size,
                BirthDate = request.BirthDate.ToUniversalTime(),
                About = request.About,
                Row = request.Row,
                Enclosure = request.Enclosure,

                LastUpdate = DateTime.UtcNow,
                UpdatedBy = request.UpdatedBy
            };

            _ = await _dbContext.AddAsync(dog, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddDogCommandResult
            {
                Dog = dog
            };
        }
    }
}