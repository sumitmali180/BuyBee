using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class UnitsController : Controller
    {
        private readonly IUnitsRepository _unitsRepository;

        public UnitsController(IUnitsRepository unitsRepository)
        {
            _unitsRepository = unitsRepository;
        }

        #region Index

        public IActionResult Index()
        {
            var units = _unitsRepository.GetAll();

            return View(units);
        }

        #endregion

        #region Details

        public IActionResult Details(int id)
        {
            var unit = _unitsRepository.GetById(id);

            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        #endregion

        #region Create

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Units unit)
        {
            if (ModelState.IsValid)
            {
                _unitsRepository.Insert(unit);

                TempData["Success"] = "Unit created successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(unit);
        }

        #endregion

        #region Edit

        public IActionResult Edit(int id)
        {
            var unit = _unitsRepository.GetById(id);

            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Units unit)
        {
            if (id != unit.UnitId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _unitsRepository.Update(unit);

                TempData["Success"] = "Unit updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(unit);
        }

        #endregion

        #region Delete

        public IActionResult Delete(int id)
        {
            var unit = _unitsRepository.GetById(id);

            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _unitsRepository.Delete(id);

            TempData["Success"] = "Unit deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}