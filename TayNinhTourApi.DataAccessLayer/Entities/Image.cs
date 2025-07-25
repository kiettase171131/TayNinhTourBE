﻿namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Image : BaseEntity
    {
        public string Url { get; set; } = null!;
        public ICollection<Tour> Tour { get; set; } = new List<Tour>();
        public ICollection<TourTemplate> TourTemplates { get; set; } = new List<TourTemplate>();

    }
}
