using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

public class AddTitlePhotoCommand : IRequest<AddTitlePhotoCommandResult>
{
    public int DogId { get; init; }
    public Stream TitlePhotoStream { get; init; }
    public string RootPath { get; init; }
    public string UpdatedBy { get; init; }
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

        if (File.Exists($"{request.RootPath}\\images\\{request.DogId}\\Title.jpg"))
        {
            File.Delete($"{request.RootPath}\\images\\{request.DogId}\\Title.jpg");
        }

        using (var fileStream = new FileStream($"{request.RootPath}\\images\\{request.DogId}\\Title.jpg", FileMode.Create))
        {
            request.TitlePhotoStream.Seek(0, SeekOrigin.Begin);
            request.TitlePhotoStream.CopyTo(fileStream);
        }

        dog.LastUpdate = DateTime.UtcNow;
        dog.UpdatedBy = request.UpdatedBy;

        _dbContext.Doges.Update(dog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddTitlePhotoCommandResult
        {
            TitlePhotoIsAdded = true
        };
    }
}