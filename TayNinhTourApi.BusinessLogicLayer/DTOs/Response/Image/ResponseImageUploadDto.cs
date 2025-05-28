namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Image
{
    public class ResponseImageUploadDto : BaseResposeDto
    {
        public List<string> Urls { get; set; } = null!;
    }
}
