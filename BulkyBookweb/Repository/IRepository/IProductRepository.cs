using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}
