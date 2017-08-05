using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LineBotFunctions.CloudStorage
{
    public class BingoBotBlobStorage
    {
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public BingoBotBlobStorage(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = strageAccount.CreateCloudBlobClient();
        }

        public static async Task<BingoBotBlobStorage> CreateAsync(string connectionString)
        {
            var instance = new BingoBotBlobStorage(connectionString);
            await instance.InitializeAsync();
            return instance;
        }

        public async Task InitializeAsync()
        {
            _blobContainer = _blobClient.GetContainerReference("bingocard");
            await _blobContainer.CreateIfNotExistsAsync();
            _blobContainer.SetPermissions(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public async Task<Uri> UploadImageAsync(System.Drawing.Image image,string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Position = 0;
                await blob.UploadFromStreamAsync(stream);
                return blob.Uri;
            }
        }

        public async Task DeleteImageAsync(string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task DeleteDirectoryAsync(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            foreach(CloudBlob blob in directory.ListBlobs())
            {
                await blob.DeleteIfExistsAsync();
            }
        }
    }
}

