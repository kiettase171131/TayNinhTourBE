using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class RequestCreateProductDto 
    {
        [Required(ErrorMessage = "Please select Name")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please select Price")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Please select QuantityInStock")]
        public int QuantityInStock { get; set; }
        [Required(ErrorMessage = "Please select Category")]
        public ProductCategory Category { get; set; }
        
        [Required(ErrorMessage = "Please select Images")]
        public List<IFormFile>? Files { get; set; }
    }
}
