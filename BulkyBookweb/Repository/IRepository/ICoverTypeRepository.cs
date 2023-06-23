using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
    public interface ICoverTypeRepository :IRepository<CoverType>
    {
       void Update(CoverType coverType);
    }
}
