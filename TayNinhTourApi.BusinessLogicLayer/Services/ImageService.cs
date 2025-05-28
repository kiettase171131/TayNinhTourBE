using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Image;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Image;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class ImageService : BaseService, IImageService
    {
        private readonly HashSet<string> _allowedExtensions = new HashSet<string> { ".png", ".jpg", ".jpeg", ".webp" };

        public ImageService(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        {
        }

        public async Task<ResponseImageUploadDto> UploadImage(List<RequestImageUploadDto> imageDtos, string localRootPath, string urlPath)
        {
            if (imageDtos == null || imageDtos.Count == 0)
            {
                return new ResponseImageUploadDto()
                {
                    StatusCode = 400,
                    Message = "No files were uploaded."
                };
            }

            var filePaths = new List<string>();

            foreach (var imageDto in imageDtos)
            {
                var validateDto = ValidateImage(imageDto);
                if (validateDto == null)
                {
                    string fileName = $"{Guid.NewGuid()}{imageDto.FileExtension}";
                    await SaveImageAsync(imageDto.FileContent, fileName, localRootPath);

                    var filePath = $"{urlPath}/{fileName}";

                    var imageEntity = new Image
                    {
                        Url = filePath,
                    };

                    await _unitOfWork.ImageRepository!.AddAsync(imageEntity);
                    await _unitOfWork.SaveChangesAsync();
                    filePaths.Add(filePath);
                }
                else
                {
                    return validateDto;
                }
            }

            return new ResponseImageUploadDto()
            {
                StatusCode = 200,
                Urls = filePaths
            };
        }

        private ResponseImageUploadDto? ValidateImage(RequestImageUploadDto imageDto)
        {
            string fileExtension = Path.GetExtension(imageDto.FileName).ToLower();

            if (imageDto == null || imageDto.FileContent == null || imageDto.FileContent.Length == 0)
            {
                return new ResponseImageUploadDto()
                {
                    Message = "File content is empty or invalid."
                };
            }
            else if (!_allowedExtensions.Contains(fileExtension))
            {
                return new ResponseImageUploadDto()
                {
                    Message = $"Invalid file extension for {imageDto.FileName}. Only {string.Join(", ", _allowedExtensions)} are allowed."
                };
            }

            return null;
        }

        private async Task SaveImageAsync(byte[] fileContent, string fileName, string localRootPath)
        {
            if (!Directory.Exists(localRootPath))
            {
                Directory.CreateDirectory(localRootPath);
            }

            string localFilePath = Path.Combine(localRootPath, fileName);
            await File.WriteAllBytesAsync(localFilePath, fileContent);
        }
    }
}
