using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
    public static BlobServiceClient CreateBlobServiceClient(
        AzureBaseStorageOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        ValidateOptions(options);

        var blobServiceClient = string.IsNullOrWhiteSpace(options.AccountName)
            ? new BlobServiceClient(options.ConnectionString)
            : new BlobServiceClient(new Uri($"https://{options.AccountName}.blob.core.windows.net"), new DefaultAzureCredential());

        return blobServiceClient;
    }

    public static AsyncLazy<BlobContainerClient> CreateBlobContainerClient(
        BlobServiceClient blobServiceClient,
        AzureBaseStorageOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        ValidateOptions(options);

        return new AsyncLazy<BlobContainerClient>(
            async () =>
            {
                var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName!);
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                return containerClient;
            },
            AsyncLazyFlags.RetryOnFailure);
    }

    /// <summary>
    /// Generates a Shared Access Signature (SAS) URI for a blob in Azure Blob Storage, allowing specific actions
    /// for a specified duration. This method requires Shared Key credentials.
    /// </summary>
    /// <param name="blobFileId">The unique identifier for the blob within the storage container.</param>
    /// <param name="action">The type of access (e.g., read, write) to grant with the SAS token.</param>
    /// <param name="containerClient">The BlobContainerClient for the target storage container.</param>
    /// <param name="sharedAccessDuration">The duration for which the SAS token will be valid.</param>
    /// <returns>
    /// A URI containing the SAS token for the specified blob. This URI can be used to access the blob with 
    /// the granted permissions within the specified duration.
    /// </returns>
    /// <exception cref="AzureStorageException">Thrown if the BlobClient is not authorized with Shared Key credentials.</exception>
    public static Uri GetSharedAccessUriFromContainer(
        string blobFileId,
        CloudStorageAction action,
        BlobContainerClient containerClient,
        TimeSpan sharedAccessDuration)
    {
        BlobHelper.ValidateBlobName(blobFileId);

        var blobClient = GetBlobClient(containerClient, blobFileId);
        if (!blobClient.CanGenerateSasUri)
        {
            throw new AzureStorageException("BlobClient must be authorized with Shared Key credentials to create a service SAS.");
        }

        var accessDuration = CalculateAccessDuration(sharedAccessDuration);
        var sasBuilder = CreateBlobSasBuilder(blobClient, action, accessDuration.StartsOn, accessDuration.EndsOn);

        return blobClient.GenerateSasUri(sasBuilder);
    }

    /// <summary>
    /// Generates a Shared Access Signature (SAS) URI for a blob in Azure Blob Storage with user-delegated permissions,
    /// allowing specific actions for a specified duration.
    /// </summary>
    /// <param name="blobFileId">The unique identifier for the blob within the storage container.</param>
    /// <param name="action">The type of access (e.g., read, write) to grant with the SAS token.</param>
    /// <param name="serviceClient">The BlobServiceClient used to obtain the user delegation key.</param>
    /// <param name="containerClient">The BlobContainerClient for the target storage container.</param>
    /// <param name="sharedAccessDuration">The duration for which the SAS token will be valid.</param>
    /// <returns>
    /// URI that includes the SAS token for the specified blob.
    /// This URI can be used to access the blob with the granted permissions within the specified duration.
    /// </returns>
    public static async Task<Uri> GenerateUserDelegatedBlobSasUriAsync(
        string blobFileId,
        CloudStorageAction action,
        BlobServiceClient serviceClient,
        BlobContainerClient containerClient,
        TimeSpan sharedAccessDuration,
        CancellationToken cancellationToken)
    {
        BlobHelper.ValidateBlobName(blobFileId);

        var blobClient = GetBlobClient(containerClient, blobFileId);

        var accessDuration = CalculateAccessDuration(sharedAccessDuration);

        var sasBuilder = CreateBlobSasBuilder(blobClient, action, accessDuration.StartsOn, accessDuration.EndsOn);

        var userDelegationKey = await serviceClient.GetUserDelegationKeyAsync(
            accessDuration.StartsOn,
            accessDuration.EndsOn,
            cancellationToken)
            .ConfigureAwait(false);

        var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, serviceClient.AccountName);

        var blobUriBuilder = new BlobUriBuilder(containerClient.Uri);

        blobUriBuilder.BlobName = blobClient.Name;
        blobUriBuilder.Sas = sasToken;

        return blobUriBuilder.ToUri();
    }

    private static BlobClient GetBlobClient(
        BlobContainerClient containerClient,
        string blobFileId)
    {
        var escapedBlobName = Uri.EscapeDataString(blobFileId);
        return containerClient.GetBlobClient(escapedBlobName);
    }

    private static BlobSasBuilder CreateBlobSasBuilder(
        BlobClient blobClient,
        CloudStorageAction action,
        DateTime startsOn,
        DateTime endsOn)
    {
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

        return sasBuilder;
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

    private static (DateTime StartsOn, DateTime EndsOn) CalculateAccessDuration(
        TimeSpan sharedAccessDuration)
    {
        var startsOn = DateTime.UtcNow;
        var endsOn = startsOn.Add(sharedAccessDuration);

        return (startsOn, endsOn);
    }

    [GeneratedRegex("^[a-z0-9-]*$", RegexOptions.Compiled)]
    private static partial Regex ContainerCharacterRegex();

    [GeneratedRegex("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Compiled)]
    private static partial Regex ContainerFullValidityRegex();
}
