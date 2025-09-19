using Domain.DTOs;
using System.Collections.Generic;

namespace CafeMenu.Models
{
    public class MenuViewModel
    {
        public IEnumerable<ProductDto> Products { get; set; }
        public IEnumerable<CategoryDto> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}