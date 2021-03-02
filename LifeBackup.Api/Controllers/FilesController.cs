using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeBackup.Core.Communication.Files;
using LifeBackup.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeBackup.Api.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;

        #region Constructor

        public FilesController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        #endregion

        #region Class Methods

        [HttpPost]
        [Route("{bucketName}/add")]
        public async Task<ActionResult<AddFileResponse>> AddFiles(string bucketName, IList<IFormFile> formFiles)
        {
            if (formFiles == null)
            {
                return BadRequest("The request doesn't contain any files to be uploaded.");
            }

            var response = await _fileRepository.UploadFile(bucketName, formFiles);
            if (response == null)
            {
                return BadRequest();
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("{bucketName}/list")]
        public async Task<ActionResult<IEnumerable<ListFilesResponse>>> ListFiles(string bucketName)
        {
            var response = await _fileRepository.ListFiles(bucketName);
            return Ok(response);
        }

        [HttpGet]
        [Route("{bucketName}/download/{filename}")]
        public async Task<IActionResult> DownloadFile(string bucketName, string fileName)
        {
            await _fileRepository.DownloadFile(bucketName, fileName);
            return Ok();
        }

        [HttpDelete]
        [Route("{bucketName}/delete/{fileName}")]
        public async Task<ActionResult<DeleteFileResponse>> DeleteFile(string bucketName, string fileName)
        {
            var response = await _fileRepository.DeleteFile(bucketName, fileName);
            return Ok(response);
        }

        [HttpPost]
        [Route("{bucketName}/addjsonobject")]
        public async Task<IActionResult> AddJsonObject(string bucketName, [FromBody] AddJsonObjectRequest request)
        {
            await _fileRepository.AddJsonObject(bucketName, request);
            return Ok();
        }

        [HttpGet]
        [Route("{bucketName}/getjsonobject")]
        public async Task<ActionResult<GetJsonObjectResponse>> GetJsonObject(string bucketName, [FromQuery] string fileName)
        {
            var result = await _fileRepository.GetJsonObject(bucketName, fileName);
            return Ok(result);
        }

        #endregion
    }
}