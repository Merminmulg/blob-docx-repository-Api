using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace blob_docx_repository_Api
{
    public class FileService
    {
        private readonly string _storageAccount = "blobstoragefortesttasks";
        private readonly string _key = Environment.GetEnvironmentVariable("BlobKey");
        private readonly BlobContainerClient _filesContainer;

        public FileService()
        {
            var credential = new StorageSharedKeyCredential(_storageAccount, _key);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            _filesContainer = blobServiceClient.GetBlobContainerClient("docx");
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob, string email)
        {
            BlobResponseDto response = new();
            BlobClient client = _filesContainer.GetBlobClient(blob.FileName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _filesContainer.Name,
                BlobName = client.Name,
                Resource = "b", // "b" означает файл
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            };

            sasBuilder.SetPermissions(BlobSasPermissions.All);

            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_storageAccount, _key)).ToString();

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = blob.ContentType,
            };

            var metadata = new Dictionary<string, string>
            {
                { "email", email },
                { "sasUri", client.Uri + "?" + sasToken }
            };

            BlobUploadOptions uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata
            };

            await using (Stream? data = blob.OpenReadStream()) 
            {
                await client.UploadAsync(data, uploadOptions);
            }

            response.Status = $"File {blob.FileName} uploaded successfully";
            response.Error = false;
            response.Blob.Uri = new Uri(client.Uri, sasToken).AbsoluteUri;
            Console.WriteLine(response.Blob.Uri);
            response.Blob.Name = client.Name;
            return response;
        }
    }
}
