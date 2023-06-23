using BulkyBookweb.Models;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Models.ViewModel;
using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookweb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _Unit;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public ProductController(IUnitOfWork Unit, IWebHostEnvironment webHostEnvironment)
        {
            _Unit = Unit;
            _WebHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
          
            return View();
        }
        //get
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new(),
                CategoryList = _Unit.Category.GetAll().Select(
               u => new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString()
               }
               ),
                CoverTypeList = _Unit.CoverType.GetAll().Select(
             u => new SelectListItem
             {
                 Text = u.Name,
                 Value = u.Id.ToString()
             }
             ),
            };
         

           

            if (id == null || id == 0)
            {
              
                //create product
                return View(productVM);
            }
            else
            {
               productVM.Product = _Unit.Product.GetFirstOrDefault(u=>u.Id==id);
                return View(productVM);
                //update product
            }


          
        }
        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwroot = _WebHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwroot, @"Image\Products");
                    var exttension = Path.GetExtension(file.FileName);
                    if (obj.Product.ImageUrl != null)
                    {
                        var oldimagepath = Path.Combine(wwwroot, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimagepath))
                        {
                            System.IO.File.Delete(oldimagepath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + exttension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\Image\Products\" + fileName + exttension;
                }
                    
                    if (obj.Product.Id == 0)
                   {
                        _Unit.Product.Add(obj.Product);
                    }
                   else
                   {
                        _Unit.Product.Update(obj.Product);
                   }
                
            
                _Unit.Save();
                TempData["success"] = "Product created successfuly";
                return RedirectToAction("Index");
            }
            return View(obj);

        }
    

        #region API CALLS        
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _Unit.Product.GetAll(includeproperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        [HttpDelete]
      
        public IActionResult Delete (int? id)
        
        {
           
            var ProductFromDb = _Unit.Product.GetFirstOrDefault(u => u.Id == id);
            if (ProductFromDb == null)
            {
                return Json(new { success = false,message="Error While Deleting" }) ;
            }
            var oldimagepath = Path.Combine(_WebHostEnvironment.WebRootPath, ProductFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldimagepath))
            {
                System.IO.File.Delete(oldimagepath);
            }
            _Unit.Product.Remove(ProductFromDb);
            _Unit.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }


        #endregion

    }
}
