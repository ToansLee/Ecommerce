using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.Helpers;

namespace ECommerceMVC.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly FoodOrderingContext _context;

        public MenuItemsController(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .ToListAsync();
            return View(menuItems);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return NotFound();

            return View(menuItem);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = _context.MenuCategories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    menuItem.Image = MyUtil.UploadHinh(image, "MenuItem");
                }

                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.MenuCategories.ToList();
            return View(menuItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return NotFound();

            ViewBag.Categories = _context.MenuCategories.ToList();
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem, IFormFile image)
        {
            if (id != menuItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        menuItem.Image = MyUtil.UploadHinh(image, "MenuItem");
                    }

                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MenuItems.Any(e => e.Id == menuItem.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.MenuCategories.ToList();
            return View(menuItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return NotFound();

            return View(menuItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
