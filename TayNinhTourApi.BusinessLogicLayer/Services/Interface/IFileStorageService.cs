using Microsoft.AspNetCore.Http;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface for file storage operations
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Stores a CV file and returns file information
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="userId">User ID for organizing files</param>
        /// <returns>File storage result</returns>
        Task<FileStorageResult> StoreCvFileAsync(IFormFile file, Guid userId);

        /// <summary>
        /// Deletes a CV file from storage
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteCvFileAsync(string filePath);

        /// <summary>
        /// Gets the full URL for accessing a stored file
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>Full access URL</returns>
        string GetFileAccessUrl(string filePath);

        /// <summary>
        /// Checks if a file exists in storage
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>True if file exists</returns>
        bool FileExists(string filePath);
    }

    /// <summary>
    /// Result of file storage operation
    /// </summary>
    public class FileStorageResult
    {
        public bool success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public string? AccessUrl { get; set; }
        public string? OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
    }
}
