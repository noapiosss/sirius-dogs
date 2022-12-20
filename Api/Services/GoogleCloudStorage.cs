using Api.Configuration;
using Api.Services.Interfaces;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services
{
    public class GoogleCloudStorage : ICloudStorage
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GoogleCloudStorage(IOptionsMonitor<AppConfiguration> configuration)
        {
            _storageClient = StorageClient.Create(GoogleCredential.FromFile(configuration.CurrentValue.CredentialFile));
            _bucketName = configuration.CurrentValue.BucketName;
        }

        public async Task<string> UploadFileAsync(IFormFile imageFile, string folderName, string fileNameForStorage)
        {
            if (!FileExists(folderName))
            {
                await CreateFolderAsync(folderName);
            }

            using MemoryStream memoryStream = new();
            await imageFile.CopyToAsync(memoryStream);
            Google.Apis.Storage.v1.Data.Object dataObject = await _storageClient.UploadObjectAsync(_bucketName, $"{folderName}/{fileNameForStorage}", "image/jpeg", memoryStream);
            return dataObject.Name.Split("/")[1];
        }

        public async Task DeleteFileAsync(string folderName, string fileNameForStorage)
        {
            await _storageClient.DeleteObjectAsync(_bucketName, $"{folderName}/{fileNameForStorage}");
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            Google.Apis.Storage.v1.Data.Object[] objects = _storageClient.ListObjects(_bucketName, Path.GetDirectoryName(folderName))
                .Where(o => o.Name.StartsWith($"{folderName}/"))
                .ToArray();

            foreach (Google.Apis.Storage.v1.Data.Object file in objects)
            {
                await _storageClient.DeleteObjectAsync(_bucketName, file.Name);
            }
        }

        private async Task CreateFolderAsync(string folderName)
        {
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            byte[] content = Encoding.UTF8.GetBytes("");

            _ = await _storageClient.UploadObjectAsync(_bucketName, folderName, "application/x-directory", new MemoryStream(content));
        }

        private bool FileExists(string fileName)
        {
            Google.Apis.Storage.v1.Data.Object[] objects = _storageClient.ListObjects(_bucketName, Path.GetDirectoryName(fileName)).ToArray();
            return objects.Any(o => o.Name == fileName);
        }
    }
}