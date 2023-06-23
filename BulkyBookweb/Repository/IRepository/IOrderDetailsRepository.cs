using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
	public interface IOrderDetailsRepository :IRepository<OrderDetail>
	{
		void Update(OrderDetail orderDetail);
	}
}
