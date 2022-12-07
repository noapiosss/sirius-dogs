using Api.Services.Interfaces;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services
{
    public class GoogleCloudStorage : ICloudStorage
    {
        private readonly GoogleCredential _googleCredential;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GoogleCloudStorage(IConfiguration configuration)
        {
            _googleCredential = GoogleCredential.FromFile(Environment.GetEnvironmentVariable("SIRIUS_DOGS_GOOGLE_CREDENTIAL_FILE", EnvironmentVariableTarget.Machine));
            _storageClient = StorageClient.Create(_googleCredential);
            _bucketName = Environment.GetEnvironmentVariable("SIRIUS_DOGS_GOOGLE_BUCKET", EnvironmentVariableTarget.Machine);
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