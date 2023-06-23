using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(AppDBContext db) : base(db)
        {
        }
    }
}
