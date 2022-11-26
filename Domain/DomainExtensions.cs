using System;

using Domain.Commands;
using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class TwitterCloneDomainExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> dbOptionsAction)
        {
            return services.AddMediatR(typeof(AddDogCommand))
                .AddDbContext<DogesDbContext>(dbOptionsAction);
        }
    }
}