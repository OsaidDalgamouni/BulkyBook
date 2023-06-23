using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
    public class CoverTypeRepository : Repository<CoverType>,ICoverTypeRepository
    {
        private readonly AppDBContext _db;
        public CoverTypeRepository(AppDBContext db) : base(db) {
            _db = db;
        }

        public void Update(CoverType coverType)
        {
            _db.Cover.Update(coverType);
        }
    }
}
