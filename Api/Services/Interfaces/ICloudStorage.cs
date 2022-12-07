using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;

namespace Api.Services.Interfaces
{
    public interface ICloudStorage
    {
        Task<string> UploadFileAsync(IFormFile imageFile, string folderName, string fileNameForStorage);
        Task DeleteFileAsync(string folderName, string fileNameForStorage);
        Task DeleteFolderAsync(string folderName);
    }
}