using Azure.Storage.Blobs;
using System;
using System.Configuration;

namespace RemoteFileManager.DAO
{
    static class Repository
    {
        private const string ContainerName = "bcontainer";

        private static readonly string cs = ConfigurationManager.ConnectionStrings[nameof(cs)].ConnectionString;

        private static readonly Lazy<BlobContainerClient> container = new Lazy<BlobContainerClient>(
            ()=>new BlobServiceClient(cs).GetBlobContainerClient(ContainerName));

        public static BlobContainerClient Container => container.Value;


    }
}
