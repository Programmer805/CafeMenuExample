using CafeMenu.Infrastructure;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// Admin Panel - Ürün yönetimi Controller'ı
    /// </summary>
    [TenantAuthorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Product
        public async Task<ActionResult> Index(int page = 1, int pageSize = 20, string search = "", int? categoryId = null)
        {
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = await GetCategoriesSelectListAsync();

            IEnumerable<ProductDto> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _productService.SearchProductsAsync(search, CurrentTenantId);
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetByCategoryAsync(categoryId.Value, CurrentTenantId);
            }
            else
            {
                products = await _productService.GetPagedAsync(page, pageSize, CurrentTenantId);
            }

            ViewBag.TotalCount = await _productService.GetTotalCountAsync(CurrentTenantId);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(products);
        }

        // GET: Product/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var product = await _productService.GetProductWithPropertiesAsync(id, CurrentTenantId);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Product/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.Categories = await GetCategoriesSelectListAsync();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductCreateDto productCreateDto)
        {
            if (ModelState.IsValid)
            {
                // ImagePath artık URL olarak gelecek, doğrulama yap
                if (!string.IsNullOrEmpty(productCreateDto.ImagePath))
                {
                    if (!IsValidImageUrl(productCreateDto.ImagePath))
                    {
                        ModelState.AddModelError("ImagePath", "Geçerli bir resim URL'si giriniz.");
                    }
                }

                if (ModelState.IsValid)
                {
                    productCreateDto.TenantID = CurrentTenantId;
                    productCreateDto.CreatorUserID = GetCurrentUserId();
                    
                    var result = await _productService.CreateAsync(productCreateDto);
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                        return RedirectToAction("Index");
                    }
                    
                    ModelState.AddModelError("", "Ürün oluşturulurken bir hata oluştu.");
                }
            }

            ViewBag.Categories = await GetCategoriesSelectListAsync();
            return View(productCreateDto);
        }

        // GET: Product/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id, CurrentTenantId);
            if (product == null)
            {
                return HttpNotFound();
            }

            var updateDto = new ProductUpdateDto
            {
                ID = product.ID,
                ProductName = product.ProductName,
                CategoryID = product.CategoryID,
                Price = product.Price,
                ImagePath = product.ImagePath
            };

            ViewBag.Categories = await GetCategoriesSelectListAsync();
            return View(updateDto);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProductUpdateDto productUpdateDto)
        {
            if (ModelState.IsValid)
            {
                // ImagePath artık URL olarak gelecek, doğrulama yap
                if (!string.IsNullOrEmpty(productUpdateDto.ImagePath))
                {
                    if (!IsValidImageUrl(productUpdateDto.ImagePath))
                    {
                        ModelState.AddModelError("ImagePath", "Geçerli bir resim URL'si giriniz.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var result = await _productService.UpdateAsync(productUpdateDto, CurrentTenantId);
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                        return RedirectToAction("Index");
                    }
                    
                    ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu.");
                }
            }

            ViewBag.Categories = await GetCategoriesSelectListAsync();
            return View(productUpdateDto);
        }

        // GET: Product/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id, CurrentTenantId);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteAsync(id, CurrentTenantId);
            if (result)
            {
                TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Ürün silinemedi.";
            }
            
            return RedirectToAction("Index");
        }

        // AJAX: Product/GetByCategory
        public async Task<JsonResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId, CurrentTenantId);
            return TenantJsonResult(products);
        }

        // AJAX: Product/GetRecentProducts
        public async Task<JsonResult> GetRecentProducts(int count = 10)
        {
            var products = await _productService.GetRecentProductsAsync(count, CurrentTenantId);
            return TenantJsonResult(products);
        }

        // AJAX: Product/SearchProducts
        public async Task<JsonResult> SearchProducts(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 3)
            {
                return TenantJsonResult(new List<ProductDto>());
            }

            var products = await _productService.SearchProductsAsync(term, CurrentTenantId);
            return TenantJsonResult(products.Take(20)); // İlk 20 sonucu döndür
        }

        // AJAX: Product/CheckCanDelete
        public async Task<JsonResult> CheckCanDelete(int id)
        {
            // Ürünün silinip silinemeyeceğini kontrol et
            // Örneğin: menü öğelerinde kullanılıyor mu, sipariş geçmişinde var mı vs.
            
            // Şimdilik basit kontrol - gerçek implementasyonda business rule'lar olacak
            var product = await _productService.GetByIdAsync(id, CurrentTenantId);
            if (product == null)
            {
                return ErrorJsonResult("Ürün bulunamadı.");
            }

            // Gerçek implementasyonda:
            // - Menu items kontrol edilecek
            // - Order history kontrol edilecek
            // - Other dependencies kontrol edilecek
            
            var canDelete = true;
            var message = "";
            
            // Örnek business rule (gerçek implementasyonda repository'den gelecek)
            // var hasMenuItems = await _menuService.HasProductAsync(id, CurrentTenantId);
            // var hasOrderHistory = await _orderService.HasProductAsync(id, CurrentTenantId);
            
            // if (hasMenuItems)
            // {
            //     canDelete = false;
            //     message = "Bu ürün menü öğelerinde kullanılıyor.";
            // }
            // else if (hasOrderHistory)
            // {
            //     canDelete = false;
            //     message = "Bu ürünün sipariş geçmişi mevcut.";
            // }
            
            return TenantJsonResult(new { CanDelete = canDelete, Message = message });
        }
        
        // AJAX: Product/DebugCache - Cache durumunu kontrol et
        public async Task<JsonResult> DebugCache()
        {
            try
            {
                // Cache servisine erişmek için reflection kullanabiliriz
                // Veya cache service'i public hale getirebiliriz
                var debugInfo = new
                {
                    TenantId = CurrentTenantId,
                    Message = "Cache debug özelliği aktif. Visual Studio Output penceresini kontrol edin.",
                    Timestamp = DateTime.Now
                };
                
                System.Diagnostics.Debug.WriteLine($"[CACHE DEBUG REQUEST] Tenant: {CurrentTenantId} at {DateTime.Now}");
                
                return TenantJsonResult(debugInfo);
            }
            catch (Exception ex)
            {
                return ErrorJsonResult($"Debug cache error: {ex.Message}");
            }
        }
        
        // AJAX: Product/ClearCache - Cache'i manuel temizle
        public async Task<JsonResult> ClearCache()
        {
            try
            {
                // ProductService üzerinden cache invalidation çağır
                // Bu için ProductService'e public metod eklemek gerekiyor
                
                // Geçici çözüm: Yeni bir ürün oluşturur gibi cache invalidation tetikle
                // Reflection ile private metoda eriş
                var productServiceType = _productService.GetType();
                var invalidateMethod = productServiceType.GetMethod("InvalidateProductCacheAsync", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (invalidateMethod != null)
                {
                    await (Task)invalidateMethod.Invoke(_productService, new object[] { CurrentTenantId });
                    return TenantJsonResult(new { Message = "Cache başarıyla temizlendi.", TenantId = CurrentTenantId });
                }
                else
                {
                    return ErrorJsonResult("Cache temizleme metodu bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                return ErrorJsonResult($"Cache temizleme hatası: {ex.Message}");
            }
        }

        #region Helper Methods

        private async Task<SelectList> GetCategoriesSelectListAsync()
        {
            var categories = await _categoryService.GetAllAsync(CurrentTenantId);
            return new SelectList(categories, "ID", "CategoryName");
        }

        /// <summary>
        /// Resim URL'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        private bool IsValidImageUrl(string imageUrl)
        {
            try
            {
                // Boş veya null kontrolü
                if (string.IsNullOrWhiteSpace(imageUrl))
                    return false;

                // URL formatı kontrolü
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uriResult))
                    return false;

                // HTTP/HTTPS kontrolü
                if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                    return false;

                // Resim uzantısı kontrolü (opsiyonel)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var extension = Path.GetExtension(uriResult.AbsolutePath).ToLower();
                
                // Eğer uzantı belirtilmişse kontrol et, yoksa geçerli say
                if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}