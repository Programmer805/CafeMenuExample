using CafeMenu.Infrastructure;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// Admin Panel - Kategori yönetimi Controller'ı
    /// </summary>
    [TenantAuthorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category
        public async Task<ActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync(CurrentTenantId);
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id, CurrentTenantId);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            ViewBag.ParentCategories = GetParentCategoriesSelectList();
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryCreateDto categoryCreateDto)
        {
            if (ModelState.IsValid)
            {
                categoryCreateDto.TenantID = CurrentTenantId;
                categoryCreateDto.CreatorUserID = GetCurrentUserId(); // Session'dan alınacak
                
                var result = await _categoryService.CreateAsync(categoryCreateDto);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Kategori oluşturulurken bir hata oluştu.");
            }

            ViewBag.ParentCategories = GetParentCategoriesSelectList();
            return View(categoryCreateDto);
        }

        // GET: Category/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id, CurrentTenantId);
            if (category == null)
            {
                return HttpNotFound();
            }

            var updateDto = new CategoryUpdateDto
            {
                ID = category.ID,
                CategoryName = category.CategoryName,
                ParentCategoryID = category.ParentCategoryID
            };

            ViewBag.ParentCategories = GetParentCategoriesSelectList(category.ID);
            return View(updateDto);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CategoryUpdateDto categoryUpdateDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateAsync(categoryUpdateDto, CurrentTenantId);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu.");
            }

            ViewBag.ParentCategories = GetParentCategoriesSelectList(categoryUpdateDto.ID);
            return View(categoryUpdateDto);
        }

        // GET: Category/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id, CurrentTenantId);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteAsync(id, CurrentTenantId);
            if (result)
            {
                TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kategori silinemedi. Bu kategorinin alt kategorileri veya ürünleri mevcut olabilir.";
            }
            
            return RedirectToAction("Index");
        }

        // AJAX: Category/GetHierarchy
        public async Task<JsonResult> GetHierarchy()
        {
            var categories = await _categoryService.GetCategoryHierarchyAsync(CurrentTenantId);
            return TenantJsonResult(categories);
        }

        // AJAX: Category/GetByParent
        public async Task<JsonResult> GetByParent(int? parentId)
        {
            var categories = await _categoryService.GetByParentCategoryAsync(parentId, CurrentTenantId);
            return TenantJsonResult(categories);
        }

        // AJAX: Category/CheckCanDelete
        public async Task<JsonResult> CheckCanDelete(int id)
        {
            var hasSubCategories = await _categoryService.HasSubCategoriesAsync(id, CurrentTenantId);
            var hasProducts = await _categoryService.HasProductsAsync(id, CurrentTenantId);
            
            var canDelete = !hasSubCategories && !hasProducts;
            var message = "";
            
            if (hasSubCategories)
                message = "Bu kategorinin alt kategorileri mevcut.";
            else if (hasProducts)
                message = "Bu kategoride ürünler mevcut.";
            
            return TenantJsonResult(new { CanDelete = canDelete, Message = message });
        }

        #region Helper Methods

        private SelectList GetParentCategoriesSelectList(int? excludeCategoryId = null)
        {
            var categories = _categoryService.GetAll(CurrentTenantId)
                .Where(c => !excludeCategoryId.HasValue || c.ID != excludeCategoryId.Value)
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.CategoryName
                });

            var list = categories.ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = "-- Ana Kategori --" });
            
            return new SelectList(list, "Value", "Text");
        }

        #endregion
    }
}