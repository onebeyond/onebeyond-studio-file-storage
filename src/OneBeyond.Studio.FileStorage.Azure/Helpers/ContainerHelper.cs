using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using EnsureThat;
using Nito.AsyncEx;
using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;

namespace OneBeyond.Studio.FileStorage.Azure.Helpers;

internal static class ContainerHelper
{
    public static AsyncLazy<BlobContainerClient> CreateBlobContainerClient(
        AzureBaseStorageOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        var blobServiceClient = string.IsNullOrWhiteSpace(options.AccountName)
            ? new BlobServiceClient(options.ConnectionString)
            : new BlobServiceClient(new Uri($"https://{options.AccountName}.blob.core.windows.net"), new DefaultAzureCredential());
        
        return new AsyncLazy<BlobContainerClient>(
            async () =>
            {
                var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName!);
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                return containerClient;
            },
            AsyncLazyFlags.RetryOnFailure);
    }

    public static Uri GetSharedAccessUriFromContainer(
        string blobFileId,
        CloudStorageAction action,
        BlobContainerClient containerClient,
        TimeSpan sharedAccessDuration)
    {
        BlobHelper.ValidateBlobName(blobFileId);

        var escapedBlobName = Uri.EscapeDataString(blobFileId);
        var blobClient = containerClient.GetBlobClient(escapedBlobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new AzureStorageException("BlobClient must be authorized with Shared Key credentials to create a service SAS.");
        }

        var startsOn = DateTime.UtcNow;

        var endsOn = startsOn.Add(sharedAccessDuration);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            StartsOn = startsOn,
            ExpiresOn = endsOn,
            Resource = "b",
        };

        switch (action)
        {
            case CloudStorageAction.Download:
                sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);
                break;
            case CloudStorageAction.Upload:
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Add | BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Create);
                break;
            case CloudStorageAction.Delete:
                sasBuilder.SetPermissions(BlobAccountSasPermissions.Delete);
                break;
        }

        return blobClient.GenerateSasUri(sasBuilder);
    }
}
