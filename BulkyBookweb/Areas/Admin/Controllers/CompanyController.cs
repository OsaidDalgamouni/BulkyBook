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
    public class CompanyController : Controller
    {
        
      
            private readonly IUnitOfWork _Unit;
            
            public CompanyController(IUnitOfWork Unit )
            {
                _Unit = Unit;
               

            }
            public IActionResult Index()
            {

                return View();
            }
            //get
            public IActionResult Upsert(int? id)
            {
            Company company = new Company();
               




                if (id == null || id == 0)
                {

                    return View(company);
                }
                else
                {
                    company = _Unit.Company.GetFirstOrDefault(u => u.Id == id);
                    return View(company);
                
                }



            }
            //post
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Upsert(Company obj)
            {

                if (ModelState.IsValid)
                {
                 

                    if (obj.Id == 0)
                    {
                        _Unit.Company.Add(obj);
                    TempData["success"] = "Company created successfuly";
                }
                    else
                    {
                        _Unit.Company.Update(obj);

                    TempData["success"] = "Company Updated successfuly";
                }


                    _Unit.Save();
                  
                    return RedirectToAction("Index");
                }
                return View(obj);

            }


            #region API CALLS        
            [HttpGet]
            public IActionResult GetAll()
            {
                var CompanyList = _Unit.Company.GetAll();
                return Json(new { data = CompanyList });
            }
            [HttpDelete]

            public IActionResult Delete(int? id)

            {

                var obj = _Unit.Company.GetFirstOrDefault(u => u.Id == id);
                if (obj == null)
                {
                    return Json(new { success = false, message = "Error While Deleting" });
                }
               
                _Unit.Company.Remove(obj);
                _Unit.Save();

                return Json(new { success = true, message = "Delete Successful" });
            }


            #endregion
        }
    }

