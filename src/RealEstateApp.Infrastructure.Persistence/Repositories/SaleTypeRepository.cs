using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class SaleTypeRepository : GenericRepository<SaleType>, IGenericRepository<SaleType>
{
    public SaleTypeRepository(AppDbContext context)
        : base(context) { }
}
