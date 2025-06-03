namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    public class BaseResposeDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
