using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        int IncrementCount(ShoppingCart cart,int count);
        int DecrementCount(ShoppingCart cart,int count);
    }
}
