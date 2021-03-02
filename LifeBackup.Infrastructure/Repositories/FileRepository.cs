using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using LifeBackup.Core.Communication.Files;
using LifeBackup.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LifeBackup.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IAmazonS3 _s3Client;

        public FileRepository(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<AddFileResponse> UploadFile(string bucketName, IList<IFormFile> formFiles)
        {
            var response = new List<string>();
            foreach (var file in formFiles)
            {
                var uploadRequest = new TransferUtilityUploadRequest()
                {
                    InputStream = file.OpenReadStream(),
                    Key = file.FileName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.NoACL
                };

                using (var fileTransferUtility = new TransferUtility(_s3Client))
                {
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }

                var expiryUrlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = file.FileName,
                    Expires = DateTime.Now.AddDays(1)
                };

                var url = _s3Client.GetPreSignedURL(expiryUrlRequest);
                response.Add(url);
            }

            return new AddFileResponse
            {
                PreSignedUrl = response
            };
        }

        public async Task<IEnumerable<ListFilesResponse>> ListFiles(string bucketName)
        {
            var responses = await _s3Client.ListObjectsAsync(bucketName);
            return responses.S3Objects.Select(x => new ListFilesResponse
            {
                Key = x.Key, 
                BucketName = x.BucketName, 
                Owner = x.Owner.DisplayName, 
                Size = x.Size.ToString()
            });
        }

        public async Task DownloadFile(string bucketName, string fileName)
        {
            var pathAndFileName = $"/Users/sophoap/tmp/s3temp/{fileName}";
            var downloadRequest = new TransferUtilityDownloadRequest
            {
                BucketName = bucketName,
                Key = fileName,
                FilePath = pathAndFileName
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.DownloadAsync(downloadRequest);
        }

        public async Task<DeleteFileResponse> DeleteFile(string bucketName, string fileName)
        {
            var multiObjectDeleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName
            };
            multiObjectDeleteRequest.AddKey(fileName);
            
            var response = await _s3Client.DeleteObjectsAsync(multiObjectDeleteRequest);
            return new DeleteFileResponse
            {
                NumberOfDeletedObjects = response.DeletedObjects.Count
            };
        }

        public async Task AddJsonObject(string bucketName, AddJsonObjectRequest request)
        {
            var createdOnUtc = DateTime.UtcNow;
            var s3Key = $"{createdOnUtc:yyyy}/{createdOnUtc:MM}/{createdOnUtc:dd}/{request.Id}";
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = s3Key,
                ContentBody = JsonConvert.SerializeObject(request)
            };

            await _s3Client.PutObjectAsync(putObjectRequest);
        }

        public async Task<GetJsonObjectResponse> GetJsonObject(string bucketName, string fileName)
        {
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName, 
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(getObjectRequest);
            using var stream = new StreamReader(response.ResponseStream);
            var content = await stream.ReadToEndAsync();
            return JsonConvert.DeserializeObject<GetJsonObjectResponse>(content);
        }
    }
}