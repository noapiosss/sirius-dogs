using System;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class EditDogCommand : IRequest<EditDogCommandResult>
    {
        public Dog Dog { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class EditDogCommandResult
    {
        public Dog Dog { get; init; }
        public string Comment { get; init; }
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
            if (string.IsNullOrEmpty(request.UpdatedBy))
            {
                return new EditDogCommandResult
                {
                    Dog = null,
                    Comment = "Unauthorized"
                };
            }

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.Dog.Id, cancellationToken);

            if (dog == null)
            {
                return new EditDogCommandResult
                {
                    Dog = null,
                    Comment = "Dog not found"
                };
            }

            dog.Name = request.Dog.Name;
            dog.Gender = request.Dog.Gender;
            dog.Breed = request.Dog.Breed;
            dog.Size = request.Dog.Size;
            dog.BirthDate = request.Dog.BirthDate.ToUniversalTime();
            dog.About = request.Dog.About;
            dog.Row = request.Dog.Row;
            dog.Enclosure = request.Dog.Enclosure;
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new EditDogCommandResult
            {
                Dog = dog
            };
        }
    }
}