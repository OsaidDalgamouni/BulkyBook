using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDBContext _db;
        public ProductRepository(AppDBContext db) : base(db)
        {
           _db = db;
        }

        public void Update( Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.Description = obj.Description;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price100 = obj.Price100;
                obj.Author = obj.Author;
                obj.CoverType = obj.CoverType;
                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }

            }
        }
    }
}
