using BulkyBookweb.Models;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Models.ViewModel;
using BulkyBookweb.Repository;
using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Claims;

namespace BulkyBookweb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _UnitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
          OrderVM=new OrderVM() { 

                OrderHeader=_UnitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id==orderId,includeproperties: "ApplicationUser"),
                Details=_UnitOfWork.OrderDetails.GetAll(u=>u.OrderId== orderId ,includeproperties:"Product"),


            };

            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetaile()
        {
            var orderHeaderFromDB = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDB.Name= OrderVM.OrderHeader.Name;
            orderHeaderFromDB.PhoneNumber= OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDB.StreetAddress= OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDB.City= OrderVM.OrderHeader.City;
            orderHeaderFromDB.State= OrderVM.OrderHeader.State;
            orderHeaderFromDB.PostalCode= OrderVM.OrderHeader.PostalCode;
            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDB.Carrier= OrderVM.OrderHeader.Carrier;
            }
            if(OrderVM.OrderHeader.TrackingNumber!= null)
            {
                orderHeaderFromDB.TrackingNumber= OrderVM.OrderHeader.TrackingNumber;
            }
            _UnitOfWork.OrderHeader.Update(orderHeaderFromDB);
            _UnitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDB.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {

            _UnitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _UnitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
           
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin )]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus==SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate=DateTime.Now.AddDays(30);
            }
            _UnitOfWork.OrderHeader.Update(orderHeader);
            _UnitOfWork.Save();
            TempData["success"] = "Order Shipped  Successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin )]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId

                };
                var service = new RefundService();
                Refund refund=service.Create(options);
                _UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id,SD.StatusCancelled, SD.StatusCancelled);
            }
          
            _UnitOfWork.Save();
            TempData["success"] = "Order Cancelled  Successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.OrderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeproperties: "ApplicationUser");
            OrderVM.Details = _UnitOfWork.OrderDetails.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeproperties: "Product");


            //strip settings
            var domain =Request.Scheme+"://"+ Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderheaderid={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };
            foreach (var Item in OrderVM.Details)
            {

                var sessionlineitem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(Item.Price * 100),//20.00->2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = Item.Product.Title
                        },
                    },
                    Quantity = Item.Count,
                };
                options.LineItems.Add(sessionlineitem);

            }
            var service = new SessionService();
            Session session = service.Create(options);
            _UnitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _UnitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


            
        }
        public IActionResult PaymentConfirmation(int orderheaderid)
        {

            OrderHeader orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderheaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //check stripe status
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _UnitOfWork.OrderHeader.UpdateStripePaymentId(orderheaderid, orderHeader.SessionId, session.PaymentIntentId);
                    _UnitOfWork.OrderHeader.UpdateStatus(orderheaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _UnitOfWork.Save();
                }
            }

          
            _UnitOfWork.Save();
            return View(orderheaderid);
        }

        #region API CALLS        
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
           
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _UnitOfWork.OrderHeader.GetAll(includeproperties: "ApplicationUser");
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _UnitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId==claim.Value,includeproperties: "ApplicationUser");
            }
            switch (status) {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                 
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
