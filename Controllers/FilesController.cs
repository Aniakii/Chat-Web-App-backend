using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormulaOne.ChatService.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public FilesController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
            _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "chatappbucket266875";
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, int id)
        {
            string idString = id.ToString();
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (id <= 0)
                return BadRequest("Room id is required.");

            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
            if (!bucketExists) return NotFound($"Bucket {_bucketName} does not exist.");

            var fileExtension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(fileExtension))
                return BadRequest("File must have an extension.");

            var newFileName = $"{id}{fileExtension}";

            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = newFileName,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };

            request.Metadata.Add("Content-Type", file.ContentType);

            await _s3Client.PutObjectAsync(request);
            var url = $"{_s3Client.Config.DetermineServiceURL()}/{_bucketName}/{newFileName}";
            return Ok(new { url });
        }

        [HttpGet("get-room-image/{id}")]
        public async Task<IActionResult> GetFileByRoomIdAsync(int id)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
            if (!bucketExists) return NotFound($"Bucket {_bucketName} does not exist.");

            var listObjectsRequest = new ListObjectsV2Request()
            {
                BucketName = _bucketName,
                Prefix = id.ToString(),
            };

            var listObjectsResponse = await _s3Client.ListObjectsV2Async(listObjectsRequest);
            var s3Object = listObjectsResponse.S3Objects.FirstOrDefault();

            if (s3Object == null) return NotFound($"No file found for chat room {id}");

            var getObjectRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Object.Key
            };

            var getObjectResponse = await _s3Client.GetObjectAsync(getObjectRequest);

            return File(getObjectResponse.ResponseStream, getObjectResponse.Headers.ContentType);
        }
    }
}
