﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog
{
    public class RequestCreateCommentDto
    {
       
        [Required]
        public string Content { get; set; } = null!;
    }
}
