using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
	public class OrderDetailsRepository : Repository<OrderDetail>, IOrderDetailsRepository
	{
		private AppDBContext _db;
		public OrderDetailsRepository(AppDBContext db) : base(db)
		{
			_db= db;
		}

		public void Update(OrderDetail orderDetail)
		{
			_db.Update(orderDetail);
		}
	}
}
