﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết sản phẩm
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Giá sản phẩm (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng còn lại trong kho
        /// </summary>
        public int QuantityInStock { get; set; }

        /// <summary>
        /// Đường dẫn ảnh sản phẩm
        /// </summary>
        [StringLength(500)]
        public string? ImageUrl { get; set; }

       
     
        /// <summary>
        /// Có đang giảm giá không
        /// </summary>
        public bool IsSale { get; set; } = false;

        /// <summary>
        /// Giảm giá bao nhiêu phần trăm (0–100)
        /// </summary>
        [Range(0, 100)]
        public int? SalePercent { get; set; }

        [Required]
        public ProductCategory Category { get; set; }
        public int SoldCount { get; set; } = 0;


        /// <summary>
        /// Người bán sản phẩm (có thể liên kết với Shop nếu cần)
        /// </summary>
        public Guid ShopId { get; set; }

        [ForeignKey(nameof(ShopId))]
        public virtual User Shop { get; set; } = null!;
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductRating> ProductRatings { get; set; } = new List<ProductRating>();

    }
}
