using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Repository.IRepository;

namespace BulkyBookweb.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private AppDBContext _db;
		public OrderHeaderRepository(AppDBContext db) : base(db)
		{
			_db = db;
		}

		public void Update(OrderHeader orderHeader)
		{
			_db.Update(orderHeader);
		}

		public void UpdateStatus(int id, string orderstatus, string? paymentstatus = null)
		{
			var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id== id);
			if(orderFromDb!=null)
			{
				orderFromDb.OrderStatus= orderstatus;
				if(paymentstatus!=null)
				{
					orderFromDb.PaymentStatus= paymentstatus;
				}
			}
		}

		public void UpdateStripePaymentId(int id, string SessionId, string PaymentIntentId)
		{
			var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
			orderFromDb.PaymentDate = DateTime.Now;
			orderFromDb.SessionId= SessionId;
			orderFromDb.PaymentIntentId= PaymentIntentId;

		}
	}
}
