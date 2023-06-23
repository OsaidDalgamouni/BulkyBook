using BulkyBookweb.Models;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Models.ViewModel;
using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http;

namespace BulkyBookweb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
		private readonly IEmailSender _emailSender;
		[BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitOfWork unitofwork, IEmailSender emailSender)
        {
            _unitofwork = unitofwork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {  //get user Id
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value,includeproperties:"Product"),
				OrderHeader=new()
            };
			foreach(var cart in ShoppingCartVM.ListCart)
			{
			
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
			
			}

			return View(ShoppingCartVM);
        }

		public IActionResult Plus(int cartId) {
			var cart=_unitofwork.ShoppingCart.GetFirstOrDefault(u=>u.Id==cartId);
			  _unitofwork.ShoppingCart.IncrementCount(cart,1);
			_unitofwork.Save();
			return RedirectToAction("Index");
		}
		public IActionResult minus(int cartId)
		{
			var cart = _unitofwork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			if (cart.Count <= 1)
			{
				_unitofwork.ShoppingCart.Remove(cart);
                var count = _unitofwork.ShoppingCart.GetAll(u => u.Id == cart.Id).ToList().Count-1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);

            }
			else
			{
				_unitofwork.ShoppingCart.DecrementCount(cart, 1);
			}
		
			_unitofwork.Save();
			return RedirectToAction("Index");
		}
		
		
		
		public IActionResult Remove(int cartId)
		{
			var cart = _unitofwork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			_unitofwork.ShoppingCart.Remove(cart);
			_unitofwork.Save();
			var count=_unitofwork.ShoppingCart.GetAll(u=>u.Id==cart.Id).ToList().Count;
			HttpContext.Session.SetInt32(SD.SessionCart,count);
			return RedirectToAction("Index");

		}
		public IActionResult Summary()
		{
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeproperties: "Product"),
				OrderHeader = new()
			};
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
			foreach (var cart in ShoppingCartVM.ListCart)
			{

				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);

			}
			return View(ShoppingCartVM);
		}
		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPost()
		{
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.ListCart = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeproperties: "Product");
			
			ShoppingCartVM.OrderHeader.OrderDate= DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
			
			
			foreach (var cart in ShoppingCartVM.ListCart)
			{

				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);

			}
            ApplicationUser applicationUser = _unitofwork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
			else
			{
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitofwork.Save();
			foreach(var cart in ShoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitofwork.OrderDetails.Add(orderDetail);
				_unitofwork.Save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{



				//strip settings
				var domain = Request.Scheme + "://" + Request.Host.Value + "/";
				var options = new SessionCreateOptions
				{
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"Customer/Cart/Index",
				};
				foreach (var Item in ShoppingCartVM.ListCart)
					
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
				_unitofwork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitofwork.Save();

				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new {id=ShoppingCartVM.OrderHeader.Id});
			}
			//_unitofwork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
			//_unitofwork.Save();
			//return RedirectToAction("Index","Home");
		
		}
		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader=_unitofwork.OrderHeader.GetFirstOrDefault(u=>u.Id== id,includeproperties: "ApplicationUser");
			if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
                //check stripe status
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofwork.OrderHeader.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }
            }
			_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bullky Book", "<p>New Order Created </p>");
			
			List<ShoppingCart> shoppingCarts= _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			HttpContext.Session.Clear();
			_unitofwork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitofwork.Save();
			return View(id);

		}
		private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
		{
			if (quantity <= 50)
			{
				return price;
			}
			else
			{
				if (quantity <= 100)
				{
					return price50;
				}
				return price100;
			}
		}
	}
   
}
