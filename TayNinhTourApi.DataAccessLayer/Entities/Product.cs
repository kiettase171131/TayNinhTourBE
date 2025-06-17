using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Danh mục sản phẩm (VD: Mắm, Bánh tráng, Muối tôm,...)
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Người bán sản phẩm (có thể liên kết với Shop nếu cần)
        /// </summary>
        public Guid ShopId { get; set; }

        [ForeignKey(nameof(ShopId))]
        public virtual User Shop { get; set; } = null!;
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    }
}
