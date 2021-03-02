using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using LifeBackup.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LifeBackup.Integration.Tests.Scenarios
{
    [Collection("api")]
    public class FilesControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public FilesControllerTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAWSService<IAmazonS3>(new AWSOptions
                    {
                        DefaultClientConfig =
                        {
                            ServiceURL = "http://localhost:9003"
                        },
                        Credentials = new BasicAWSCredentials("FAKE", "FAKE")
                    });
                });
            }).CreateClient();

            Task.Run(CreateBucket).Wait();
        }

        private async Task CreateBucket()
        {
            await _client.PostAsJsonAsync($"api/bucket/create/testS3Bucket", "testS3Bucket");
        }

        private async Task<HttpResponseMessage> UploadFileToS3Bucket()
        {
            const string path = "/Users/sophoap/tmp/delete-me/secret-1";
            var file = File.Create(path);
            HttpContent fileStreamContext = new StreamContent(file);
            var formData = new MultipartFormDataContent
            {
                {fileStreamContext, "formFiles", "secret-1"}
            };

            var response = await _client.PostAsync("api/file/testS3Bucket/add", formData);
            fileStreamContext.Dispose();
            formData.Dispose();

            return response;
        }

        [Fact]
        public async Task When_AddFile_return_OK_status()
        {
            var response = await UploadFileToS3Bucket();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task When_list_bucket_return_OK_status()
        {
            var response = await _client.GetAsync($"api/bucket/list");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}