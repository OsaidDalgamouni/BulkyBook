using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
    public class CompanyRepository :  Repository<Company>,ICompanyRepository
    {
          private readonly AppDBContext _db;
    public CompanyRepository(AppDBContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Company obj)
    {
        _db.Companies.Update(obj);
    }
}
}
