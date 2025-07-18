﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class ProductReviewRepository : GenericRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }
    }
}
