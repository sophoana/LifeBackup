using System.Collections.Generic;
using System.Threading.Tasks;
using LifeBackup.Core.Communication.Files;
using Microsoft.AspNetCore.Http;

namespace LifeBackup.Core.Interfaces
{
    public interface IFileRepository
    {
        Task<AddFileResponse> UploadFile(string bucketName, IList<IFormFile> formFiles);
        Task<IEnumerable<ListFilesResponse>> ListFiles(string bucketName);
        Task DownloadFile(string bucketName, string fileName);
        Task<DeleteFileResponse> DeleteFile(string bucketName, string fileName);
        Task AddJsonObject(string bucketName, AddJsonObjectRequest request);
        Task<GetJsonObjectResponse> GetJsonObject(string bucketName, string fileName);
    }
}