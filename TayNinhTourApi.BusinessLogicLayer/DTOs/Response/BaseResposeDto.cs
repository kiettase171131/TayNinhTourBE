namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    public class BaseResposeDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public bool success { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new Dictionary<string, List<string>>();
    }
}
