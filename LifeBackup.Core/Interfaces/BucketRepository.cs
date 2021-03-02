using System.Collections.Generic;
using System.Threading.Tasks;
using LifeBackup.Core.Communication.Bucket;

namespace LifeBackup.Core.Interfaces
{
    public interface IBucketRepository
    {
        Task<bool> DoesS3BucketExist(string bucketName);
        Task<CreateBucketResponse> CreateBucket(string bucketName);
        Task<IEnumerable<ListS3BucketsResponse>> ListBuckets();
        Task DeleteBucket(string bucketName);
    }
}