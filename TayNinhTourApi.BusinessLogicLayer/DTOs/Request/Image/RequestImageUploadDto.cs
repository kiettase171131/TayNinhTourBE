namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Image
{
    public class RequestImageUploadDto
    {
        public string FileName { get; set; } = null!;
        public byte[] FileContent { get; set; } = null!;
        public string FileExtension { get; set; } = null!;
    }
}
