using Business.Services;
using CafeMenu.Models;
using CafeMenu.Helpers;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// Customer Panel - Müşteri menü görüntüleme Controller'ı
    /// Tenant ID URL'den alınır, login gerekmez
    /// </summary>
    [AllowAnonymous]
    public class MenuController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public MenuController(
            IProductService productService, 
            ICategoryService categoryService,
            ITenantService tenantService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Menu (Ana menü sayfası)
        public async Task<ActionResult> Index(int? categoryId = null, string search = "", decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Tenant kontrolü
            var currentTenantId = CurrentTenantId;
            if (currentTenantId <= 0)
            {
                return RedirectToAction("TenantNotFound", "Auth");
            }
            
            // Tenant bilgisini ViewBag'e ekle
            ViewBag.TenantId = currentTenantId;
            
            // Kategorileri al (navigasyon için)
            var categories = await _categoryService.GetCategoryHierarchyAsync(currentTenantId);
            ViewBag.Categories = categories;

            // Döviz kuru bilgisini al (gerçek implementasyonda external service'den gelecek)
            var exchangeRate = GetExchangeRate();
            ViewBag.ExchangeRate = exchangeRate;

            // Ürünleri filtrele
            var products = await GetFilteredProductsAsync(categoryId, search, minPrice, maxPrice, exchangeRate);

            // View model oluştur
            var viewModel = new MenuViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SearchTerm = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                ExchangeRate = exchangeRate
            };

            return View(viewModel);
        }

        // GET: Menu/Category/5 (Kategori bazlı görüntüleme)
        public async Task<ActionResult> Category(int id, bool includeSubCategories = true)
        {
            var category = await _categoryService.GetByIdAsync(id, CurrentTenantId);
            if (category == null)
            {
                return HttpNotFound();
            }

            var exchangeRate = GetExchangeRate();
            
            var products = includeSubCategories 
                ? await _productService.GetByCategoryWithSubCategoriesAsync(id, CurrentTenantId)
                : await _productService.GetByCategoryAsync(id, CurrentTenantId);

            // Döviz kuru uygula
            products = ApplyExchangeRate(products, exchangeRate);

            ViewBag.Category = category;
            ViewBag.ExchangeRate = exchangeRate;
            ViewBag.IncludeSubCategories = includeSubCategories;

            return View(products);
        }

        // GET: Menu/Product/5 (Ürün detay sayfası)
        public async Task<ActionResult> Product(int id)
        {
            var product = await _productService.GetProductWithPropertiesAsync(id, CurrentTenantId);
            if (product == null)
            {
                return HttpNotFound();
            }

            var exchangeRate = GetExchangeRate();
            product.PriceWithExchangeRate = product.Price * exchangeRate;

            ViewBag.ExchangeRate = exchangeRate;

            return View(product);
        }

        // AJAX: Menu/Search (Canlı arama)
        public async Task<JsonResult> Search(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 3)
            {
                return TenantJsonResult(new List<object>());
            }

            // Basit arama kullan
            var products = await _productService.SearchProductsAsync(term, CurrentTenantId);
            
            var exchangeRate = GetExchangeRate();
            
            var searchResults = products.Take(10).Select(p => new
            {
                Id = p.ID,
                Name = p.ProductName,
                CategoryName = p.CategoryName,
                Price = p.Price,
                PriceWithExchangeRate = p.Price * exchangeRate,
                ImagePath = p.ImagePath
            });

            return TenantJsonResult(searchResults);
        }

        // AJAX: Menu/GetProductsByPriceRange
        public async Task<JsonResult> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice, CurrentTenantId);
            var exchangeRate = GetExchangeRate();
            
            products = ApplyExchangeRate(products, exchangeRate);

            return TenantJsonResult(products.Take(20));
        }

        // AJAX: Menu/GetPopularProducts (Popüler ürünler)
        public async Task<JsonResult> GetPopularProducts(int count = 10)
        {
            var products = await _productService.GetRecentProductsAsync(count, CurrentTenantId);
            var exchangeRate = GetExchangeRate();
            
            products = ApplyExchangeRate(products, exchangeRate);

            return TenantJsonResult(products);
        }

        // GET: Menu/Categories (Kategori listesi - AJAX için)
        public async Task<JsonResult> GetCategories()
        {
            var categories = await _categoryService.GetCategoryHierarchyAsync(CurrentTenantId);
            
            var categoryTree = categories.Select(c => new
            {
                Id = c.ID,
                Name = c.CategoryName,
                ParentId = c.ParentCategoryID,
                ProductCount = c.Products?.Count ?? 0
            });

            return TenantJsonResult(categoryTree);
        }

        #region Helper Methods

        private async Task<IEnumerable<ProductDto>> GetFilteredProductsAsync(
            int? categoryId, string search, decimal? minPrice, decimal? maxPrice, decimal exchangeRate)
        {
            IEnumerable<ProductDto> products;

            if (!string.IsNullOrEmpty(search))
            {
                // Basit arama kullan
                products = await _productService.SearchProductsAsync(search, CurrentTenantId);
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetByCategoryWithSubCategoriesAsync(categoryId.Value, CurrentTenantId);
            }
            else
            {
                // Tüm ürünleri al (cache'den)
                products = await _productService.GetProductsWithExchangeRateAsync(CurrentTenantId, exchangeRate);
            }

            // Fiyat filtresi uygula
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                products = products.Where(p =>
                {
                    var priceToCheck = p.Price * exchangeRate;
                    return (!minPrice.HasValue || priceToCheck >= minPrice.Value) &&
                           (!maxPrice.HasValue || priceToCheck <= maxPrice.Value);
                });
            }

            // Döviz kuru uygula
            return ApplyExchangeRate(products, exchangeRate);
        }

        private IEnumerable<ProductDto> ApplyExchangeRate(IEnumerable<ProductDto> products, decimal exchangeRate)
        {
            foreach (var product in products)
            {
                product.PriceWithExchangeRate = product.Price * exchangeRate;
            }
            return products;
        }

        private decimal GetExchangeRate()
        {
            // URL'den para birimi kontrolü
            var fromCurrency = Request.QueryString["from"] ?? "TRY"; // Varsayılan para birimi
            var toCurrency = Request.QueryString["to"] ?? "USD"; // Varsayılan hedef para birimi USD
            
            // Eğer aynı para birimi ise 1 döndür
            if (fromCurrency.Equals(toCurrency, System.StringComparison.OrdinalIgnoreCase))
                return 1.0m;
            
            // Query string'den manuel kur kontrolü
            if (Request.QueryString["exchangeRate"] != null && 
                decimal.TryParse(Request.QueryString["exchangeRate"], out var manualRate))
            {                
                Session["ExchangeRate"] = manualRate;
                Session["ExchangeRatePair"] = $"{fromCurrency}/{toCurrency}";
                Session["ExchangeRateSource"] = "Manual";
                return manualRate;
            }
            
            // Session'dan kontrol et (15 dakikadan eski değilse)
            var sessionPair = Session["ExchangeRatePair"] as string;
            var sessionTime = Session["ExchangeRateTime"] as System.DateTime?;
            
            if (Session["ExchangeRate"] != null && 
                sessionPair == $"{fromCurrency}/{toCurrency}" &&
                sessionTime.HasValue && 
                (System.DateTime.Now - sessionTime.Value).TotalMinutes < 15)
            {
                return (decimal)Session["ExchangeRate"];
            }

            // ExchangeRateHelper'dan güncel kuru al
            try
            {
                var currentRate = ExchangeRateHelper.GetExchangeRate(fromCurrency, toCurrency);
                Session["ExchangeRate"] = currentRate;
                Session["ExchangeRatePair"] = $"{fromCurrency}/{toCurrency}";
                Session["ExchangeRateTime"] = System.DateTime.Now;
                Session["ExchangeRateSource"] = "API";
                return currentRate;
            }
            catch
            {
                // Hata durumunda varsayılan kur
                return 1.0m;
            }
        }

        #endregion
        
        // AJAX: Menu/GetExchangeRateInfo (Döviz kuru bilgisi)
        public JsonResult GetExchangeRateInfo()
        {
            try
            {
                var fromCurrency = Request.QueryString["from"] ?? "TRY";
                var toCurrency = Request.QueryString["to"] ?? "TRY";
                
                var rate = GetExchangeRate();
                var currencyPair = ExchangeRateHelper.FormatCurrencyPair(fromCurrency, toCurrency);
                var source = Session["ExchangeRateSource"] as string ?? "Default";
                var lastUpdate = Session["ExchangeRateTime"] as System.DateTime?;
                
                return TenantJsonResult(new
                {
                    success = true,
                    rate = rate,
                    formattedRate = rate.ToString("N4"),
                    currencyPair = currencyPair,
                    fromCurrency = fromCurrency,
                    toCurrency = toCurrency,
                    source = source,
                    lastUpdate = lastUpdate?.ToString("HH:mm") ?? "Bilinmiyor",
                    supportedCurrencies = ExchangeRateHelper.GetSupportedCurrencies()
                });
            }
            catch (System.Exception ex)
            {
                return TenantJsonResult(new { success = false, message = ex.Message });
            }
        }
        
        // AJAX: Menu/RefreshExchangeRate (Döviz kuru yenileme)
        [HttpPost]
        public async Task<JsonResult> RefreshExchangeRate(string fromCurrency = "TRY", string toCurrency = "TRY")
        {
            try
            {
                // Cache'i temizle
                ExchangeRateHelper.ClearCache();
                
                // Session'dan kur bilgilerini temizle
                Session.Remove("ExchangeRate");
                Session.Remove("ExchangeRatePair");
                Session.Remove("ExchangeRateTime");
                Session.Remove("ExchangeRateSource");
                
                // Yeni kuru al
                var newRate = await ExchangeRateHelper.GetExchangeRateAsync(fromCurrency, toCurrency);
                
                // Session'a kaydet
                Session["ExchangeRate"] = newRate;
                Session["ExchangeRatePair"] = $"{fromCurrency}/{toCurrency}";
                Session["ExchangeRateTime"] = System.DateTime.Now;
                Session["ExchangeRateSource"] = "API-Refresh";
                
                return TenantJsonResult(new
                {
                    success = true,
                    rate = newRate,
                    formattedRate = newRate.ToString("N4"),
                    currencyPair = ExchangeRateHelper.FormatCurrencyPair(fromCurrency, toCurrency),
                    message = "Döviz kuru başarıyla yenilendi",
                    lastUpdate = System.DateTime.Now.ToString("HH:mm")
                });
            }
            catch (System.Exception ex)
            {
                return TenantJsonResult(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
        
        // GET: Menu/TenantInfo (Test ve bilgi sayfası)
        public ActionResult TenantInfo()
        {
            ViewBag.TenantId = CurrentTenantId;
            return View();
        }
        
        // GET: Menu/ExchangeRateTest (Döviz kuru test sayfası)
        public ActionResult ExchangeRateTest()
        {
            // Döviz kuru bilgisini al ve ViewBag'e ekle
            var exchangeRate = GetExchangeRate();
            ViewBag.ExchangeRate = exchangeRate;
            
            // Para birimi bilgileri
            var fromCurrency = Request.QueryString["from"] ?? "TRY";
            var toCurrency = Request.QueryString["to"] ?? "TRY";
            
            ViewBag.FromCurrency = fromCurrency;
            ViewBag.ToCurrency = toCurrency;
            ViewBag.CurrencyPair = $"{fromCurrency}/{toCurrency}";
            
            return View();
        }
    }
}