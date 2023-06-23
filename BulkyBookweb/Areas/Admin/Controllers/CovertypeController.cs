using BulkyBookweb.Models;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookweb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CovertypeController : Controller
    {
        private readonly IUnitOfWork _unit;
        public CovertypeController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> CoverList = _unit.CoverType.GetAll();
            return View(CoverList);
        }
        //get
        public IActionResult Create()
        {
            return View();
        }
        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (obj.Name == obj.Id.ToString()) 
            {
                ModelState.AddModelError("name", "the display order can not exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                _unit.CoverType.Add(obj);
                _unit.Save();

                return RedirectToAction("Index");
            }
            return View(obj);

        }
      
        public IActionResult Edit(int?id) {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var GetId = _unit.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (GetId== null)
            {
                return NotFound();
            }
          
           
            return View(GetId);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (obj.Name == obj.Id.ToString())
            {
                ModelState.AddModelError("name", "the display order can not exactly match the name.");
            }

            if (ModelState.IsValid)
            {
                _unit.CoverType.Update(obj);
                _unit.Save();
                TempData["success"] = "Category edited successfuly";
                return RedirectToAction("Index");
            }
            return View(obj);

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var CoverDelete = _unit.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (CoverDelete == null)
            {
                return NotFound();
            }
            _unit.CoverType.Remove(CoverDelete);
            _unit.Save();
            TempData["success"] = "Category deleted successfuly";
            return RedirectToAction("Index");
        }



    }
}
