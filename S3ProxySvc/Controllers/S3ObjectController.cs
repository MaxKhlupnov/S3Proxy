using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.Auth;
using Amazon.S3.Model;
using System.IO;
using System.Security.Cryptography;
using System.Numerics;
using System.Globalization;
using System.Text;
using S3ProxySvc.Models;

namespace S3ProxySvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3ObjectController : ControllerBase
    {

        private readonly IAmazonS3 yandexS3;
        internal const string PROPERTY_FILE_NAME = "X-Amz-Meta-FileName";
        internal const string PROPERTY_CONTENT_TYPE = "X-Amz-Meta-ContentType";

        public S3ObjectController(IAmazonS3 amazonS3)
        {

            this.yandexS3 = amazonS3;
        }

        [HttpPost]
        [Route("WriteBinary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WriteBinary([FromForm] IFormFile File, string BucketName)
        {
            if (string.IsNullOrEmpty(BucketName))
                return StatusCode(StatusCodes.Status400BadRequest, " BucketName required");

            PutObjectRequest putRequest = new PutObjectRequest
            {
                 BucketName = BucketName,
                 Key = Guid.NewGuid().ToString("N"),
                 InputStream = File.OpenReadStream()
            };

            putRequest.Metadata.Add("FileName", File.FileName);
            putRequest.Metadata.Add("ContentType", File.ContentType);
            try
            {
                PutObjectResponse res = await this.yandexS3.PutObjectAsync(putRequest);
            }catch (Exception ex)
            {             
                return StatusCode(StatusCodes.Status500InternalServerError, $"{putRequest.BucketName} {putRequest.Key} {ex.Message}");
            }

           return Ok(putRequest.FilePath);
        }

        [HttpPost]
        [Route("ReadBinary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ReadBinary([FromBody] GetObjectRequestModel request)
        {
            if (string.IsNullOrEmpty(request.BucketName))
                return StatusCode(StatusCodes.Status400BadRequest, " BucketName required");

            GetObjectRequest req = new GetObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.Key
            };
            GetObjectResponse res = await this.yandexS3.GetObjectAsync(req);
            string ContentType = res.Metadata[PROPERTY_CONTENT_TYPE];
            if (string.IsNullOrEmpty(ContentType))
            {
                ContentType = "application/octet-stream";
            }

            return File(res.ResponseStream, ContentType);
        }

        [HttpPost]
        [Route("ReadText")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> ReadText([FromBody] GetTextObjectRequestModel request)
        {

            if (string.IsNullOrEmpty(request.BucketName))
                return StatusCode(StatusCodes.Status400BadRequest, " BucketName required");

            GetObjectRequest req = new GetObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.Key
            };
            GetObjectResponse res = await this.yandexS3.GetObjectAsync(req);
            Encoding txtEncoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(request.Encoding)) {
                txtEncoding = Encoding.GetEncoding(request.Encoding);
            }
            using (StreamReader reader = new StreamReader(res.ResponseStream, txtEncoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
