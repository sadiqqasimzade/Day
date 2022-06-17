using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Day.DAL;
using Day.Models;
using Day.Utilities;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace Day.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Products.Include(p => p.Category);
            return View(await appDbContext.ToListAsync());
        }


        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.File == null)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                ModelState.AddModelError("File", "File must exit");
                return View(product);
            }
            if (product.File.Length / 1024 > Consts.ProudctImgMaxSizeKb)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                ModelState.AddModelError("File", "Too big must be smaller than:" + Consts.ProudctImgMaxSizeKb + "Kb");
                return View(product);
            }
            if (!product.File.ContentType.Contains("image"))
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                ModelState.AddModelError("File", "Invalid type");
                return View(product);
            }
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                return View(product);
            }

            string filename = Guid.NewGuid().ToString() + product.File.FileName;
            if (filename.Length > Consts.ProductImgNameLength)
                filename = filename.Substring(filename.Length - Consts.ProductImgNameLength, Consts.ProductImgNameLength);

            using (FileStream fs = new FileStream(Path.Combine(Consts.ProductImgPath, filename), FileMode.Create))
                await product.File.CopyToAsync(fs);

            product.Img = filename;
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update( Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                return View(product);
            }
            Product dbproduc = await _context.Products.FindAsync(product.Id);
            dbproduc.Title = product.Title.Trim();
            dbproduc.Title2 = product.Title2.Trim();
            dbproduc.CategoryId = product.CategoryId;
            if (product.File != null)
            {
                if (product.File.Length / 1024 < Consts.ProudctImgMaxSizeKb || product.File.ContentType.Contains("image"))
                {
                    if (System.IO.File.Exists(Path.Combine(Consts.ProductImgPath, dbproduc.Img)))
                        System.IO.File.Delete(Path.Combine(Consts.ProductImgPath, dbproduc.Img));

                    string filename = Guid.NewGuid().ToString() + product.File.FileName;
                    if (filename.Length > Consts.ProductImgNameLength)
                        filename = filename.Substring(filename.Length - Consts.ProductImgNameLength, Consts.ProductImgNameLength);

                    using (FileStream fs = new FileStream(Path.Combine(Consts.ProductImgPath, filename), FileMode.Create))
                        await product.File.CopyToAsync(fs);

                    dbproduc.Img = filename;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (System.IO.File.Exists(Path.Combine(Consts.ProductImgPath, product.Img)))
                System.IO.File.Delete(Path.Combine(Consts.ProductImgPath, product.Img));
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
