using BulkyBookweb.Data;
using BulkyBookweb.Models;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BulkyBookweb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _Unit;
        public CategoryController(IUnitOfWork Unit)
        {
            _Unit = Unit;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> CategoryList = _Unit.Category.GetAll();
            return View(CategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder)
            {
                ModelState.AddModelError("name", "the display order can not exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                _Unit.Category.Add(obj);
                _Unit.Save();

                return RedirectToAction("Index");
            }
            return View(obj);

        }
        //get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var CategoryFromDb = _Unit.Category.GetFirstOrDefault(u => u.Id == id);
            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            return View(CategoryFromDb);
        }
        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder)
            {
                ModelState.AddModelError("name", "the display order can not exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                _Unit.Category.Update(obj);
                _Unit.Save();
                TempData["success"] = "Category edited successfuly";
                return RedirectToAction("Index");
            }
            return View(obj);

        }
        //get
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var CategoryFromDb = _Unit.Category.GetFirstOrDefault(u => u.Id == id);
            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            _Unit.Category.Remove(CategoryFromDb);
            _Unit.Save();
            TempData["success"] = "Category deleted successfuly";
            return RedirectToAction("Index");
        }
        #region API CALLS        
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _Unit.Category.GetAll();
            return Json(new { data = productList });
        }
        #endregion
    }
}
