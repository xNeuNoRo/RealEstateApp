using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class ImprovementRepository : GenericRepository<Improvement>, IGenericRepository<Improvement>
{
    public ImprovementRepository(AppDbContext context)
        : base(context) { }
}
