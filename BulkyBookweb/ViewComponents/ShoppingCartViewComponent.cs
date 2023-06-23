using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookweb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _UnitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
    }
}
