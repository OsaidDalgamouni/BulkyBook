using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
    public interface ICategoryRepsitory : IRepository<Category>
    {
        void Update(Category category);
       
    }
}
