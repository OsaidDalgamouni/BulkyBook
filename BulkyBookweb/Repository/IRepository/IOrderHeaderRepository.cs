using BulkyBookweb.Models;

namespace BulkyBookweb.Repository.IRepository
{
	public interface IOrderHeaderRepository :IRepository<OrderHeader>

	{
		void Update(OrderHeader orderHeader);
		void UpdateStatus(int id, string orderstatus, string? paymentstatus=null);
		void UpdateStripePaymentId(int id,string SessionId,string PaymentIntentId);
	}
}
