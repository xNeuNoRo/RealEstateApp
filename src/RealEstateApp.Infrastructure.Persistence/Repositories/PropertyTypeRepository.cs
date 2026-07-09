using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class PropertyTypeRepository
    : GenericRepository<PropertyType>,
        IGenericRepository<PropertyType>
{
    public PropertyTypeRepository(AppDbContext context)
        : base(context) { }
}
