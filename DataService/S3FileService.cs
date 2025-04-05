using Amazon.S3;
using Amazon.S3.Model;

namespace FormulaOne.ChatService.DataService
{
    public class S3FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3FileService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
            _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "chatappbucket266875";
        }

        public async Task<string?> UploadRoomImageAsync(IFormFile file, int roomId)
        {
            var ext = Path.GetExtension(file.FileName);
            var key = $"{roomId}{ext}";

            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };
            request.Metadata.Add("Content-Type", file.ContentType);

            await _s3Client.PutObjectAsync(request);

            // Możesz zwrócić pełny URL lub tylko klucz, zależnie od potrzeby
            return key;
        }

        public string GetImageUrl(int roomId, string extension)
        {
            return $"https://{_bucketName}.s3.amazonaws.com/{roomId}{extension}";
        }
    }

}
