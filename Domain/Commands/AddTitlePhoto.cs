using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Domain.Database;
using System.IO;

public class AddTitlePhotoCommand : IRequest<AddTitlePhotoCommandResult>
{
    public int DogId { get; init; }
    public Stream TitlePhotoStream { get; init; }
    public string RootPath { get; init; }
}

public class AddTitlePhotoCommandResult
{
    public bool TitlePhotoIsAdded { get; init; }
}

internal class AddTitlePhotoCommandHandler : IRequestHandler<AddTitlePhotoCommand, AddTitlePhotoCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public AddTitlePhotoCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<AddTitlePhotoCommandResult> Handle(AddTitlePhotoCommand request, CancellationToken cancellationToken = default)
    {
        var dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);
        dog.TitlePhoto = $"/images/{request.DogId}/Title.jpg";
        await _dbContext.SaveChangesAsync(cancellationToken);

        using (var fileStream = new FileStream($"{request.RootPath}\\wwwroot\\images\\{request.DogId}\\Title.jpg", FileMode.Create))
        {
            request.TitlePhotoStream.Seek(0, SeekOrigin.Begin);
            request.TitlePhotoStream.CopyTo(fileStream);
        }

        return new AddTitlePhotoCommandResult
        {
            TitlePhotoIsAdded = true
        };
    }
}