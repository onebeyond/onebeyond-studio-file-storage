using System;
using System.Text.RegularExpressions;
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

internal static partial class ContainerHelper
{
    public static AsyncLazy<BlobContainerClient> CreateBlobContainerClient(
        AzureBaseStorageOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        ValidateOptions(options);

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

    private static void ValidateOptions(AzureBaseStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.AccountName))
        {
            throw new ArgumentException("At least one connection must be provided, " +
                "either the connection string or the account name (for Azure Identity usage).");
        }

        if (!string.IsNullOrWhiteSpace(options.ConnectionString) && !string.IsNullOrWhiteSpace(options.AccountName))
        {
            throw new ArgumentException("Only one connection can be provided, " +
                "either the connection string or the account name (for Azure Identity usage).");
        }

        ValidateContainerName(options.ContainerName);
    }

    /// <summary>
    /// This is designed to improve information about what is and is not valid for a given azure container name.
    /// </summary>
    private static void ValidateContainerName(string? containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new AzureStorageException("Container name cannot be empty.");
        }

        if (containerName.Length < 3 || containerName.Length > 63)
        {
            throw new AzureStorageException("Container name must be between 3 and 63 characters in length.");
        }

        if (!ContainerCharacterRegex().IsMatch(containerName))
        {
            throw new AzureStorageException("Container name can only contain lowercase letters, numbers or hyphens.");
        }

        if (!ContainerFullValidityRegex().IsMatch(containerName))
        {
            throw new AzureStorageException("Container name must start and end with a number or letter and cannot contain multiple hyphens in sequence.");
        }
    }

    [GeneratedRegex("^[a-z0-9-]*$", RegexOptions.Compiled)]
    private static partial Regex ContainerCharacterRegex();

    [GeneratedRegex("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Compiled)]
    private static partial Regex ContainerFullValidityRegex();
}
