using Microsoft.EntityFrameworkCore;

namespace Ecommerce520.APIV9.Repos
{
    public interface IProductRepository : IRepository<Product>
    {
        Task AddRange(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    }
}
