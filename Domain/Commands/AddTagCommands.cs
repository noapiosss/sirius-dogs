using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands;

public class AddTagsCommand : IRequest<AddTagsCommandResult>
{
    public int DogId { get; init; }
    public ICollection<string> Tags { get; init; }
}

public class AddTagsCommandResult
{
    public bool AddingIsSuccessful { get; init; }
}

internal class AddTagsCommandHandler : IRequestHandler<AddTagsCommand, AddTagsCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public AddTagsCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<AddTagsCommandResult> Handle(AddTagsCommand request, CancellationToken cancellationToken = default)
    {
        var existingTags = await _dbContext.Tags
            .Where(t => t.DogId == request.DogId)
            .Select(t => t.TagName)
            .ToListAsync(cancellationToken);

        foreach (var tag in request.Tags)
        {
            if (existingTags.Contains(tag))
            {
                continue;
            }

            var dogTag = new Tag
            {
                DogId = request.DogId,
                TagName = tag
            };
            await _dbContext.AddAsync(dogTag, cancellationToken);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dog = await _dbContext.Doges.FirstAsync(d => d.Id == request.DogId);
        dog.LastUpdate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddTagsCommandResult
        {
            AddingIsSuccessful = true
        };
    }
}