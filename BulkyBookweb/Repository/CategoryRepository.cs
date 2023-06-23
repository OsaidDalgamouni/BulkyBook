using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepsitory
    {
        private readonly AppDBContext _db;

        public CategoryRepository(AppDBContext db) :base(db)
        {
            _db = db;
        }

       

        void ICategoryRepsitory.Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
