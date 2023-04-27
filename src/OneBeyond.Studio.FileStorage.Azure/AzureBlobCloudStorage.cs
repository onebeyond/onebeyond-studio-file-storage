using Azure.Storage.Blobs;
using EnsureThat;
using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using OneBeyond.Studio.FileStorage.Azure.Helpers;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Domain.Entities;
using OneBeyond.Studio.FileStorage.Domain.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.FileStorage.Azure;

public class AzureBlobCloudStorage : AzureBlobFileStorage, ICloudFileStorage
{
    protected readonly AzureBlobCloudStorageOptions _cloudFileStorageOptions;

    public AzureBlobCloudStorage(
        MimeTypeValidationOptions mimeTypeValidationOptions,
        AzureBlobCloudStorageOptions fileStorageOptions)
        : base(mimeTypeValidationOptions, fileStorageOptions)
    {
        EnsureArg.IsNotNull(fileStorageOptions, nameof(fileStorageOptions));

        if (fileStorageOptions.SharedAccessDuration is null
            || fileStorageOptions.SharedAccessDuration.Value <= TimeSpan.Zero)
        {
            throw new AzureStorageException("SharedAccessDuration is not set, the cloud storage provider will not be able to generate file urls.");
        }

        _cloudFileStorageOptions = fileStorageOptions;
    }

    public Task<Uri> GetDownloadUrlAsync(string fileId, CancellationToken cancellationToken = default)
        => GetSasUriAsync(fileId, CloudStorageAction.Download, cancellationToken);

    public Task<Uri> GetUploadUrlAsync(string fileId, CancellationToken cancellationToken = default)
        => GetSasUriAsync(fileId, CloudStorageAction.Upload, cancellationToken);

    public Task<Uri> GetDeleteUrlAsync(string fileId, CancellationToken cancellationToken = default)
        => GetSasUriAsync(fileId, CloudStorageAction.Delete, cancellationToken);

    public async Task<FileRecord> UploadFileAsync(
        string fileName, 
        Stream fileContent, 
        string fileContentType, 
        Dictionary<string, string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        var fileRecord = await UploadFileAsync(fileName, fileContent, fileContentType, cancellationToken);

        if (tags is { })
        {
            await UpdateFileTagsAsync(fileRecord, tags, cancellationToken);
        }

        return fileRecord;
    }

    public async Task<FileRecord> UploadFileAsync(
        string fileName, 
        byte[] fileContent, 
        string fileContentType, 
        Dictionary<string, string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        var fileRecord = await UploadFileAsync(fileName, fileContent, fileContentType, cancellationToken);

        if (tags is { })
        {
            await UpdateFileTagsAsync(fileRecord, tags, cancellationToken);
        }

        return fileRecord;
    }

    public async Task UpdateFileTagsAsync(FileRecord fileRecord, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(tags, nameof(tags));

        BlobClient blobClient = await GetBlobClientAsync(fileRecord.Id).ConfigureAwait(continueOnCapturedContext: false);

        await blobClient.SetTagsAsync(tags, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    protected async Task<Uri> GetSasUriAsync(string blobFileId, CloudStorageAction action, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNullOrWhiteSpace(blobFileId, nameof(blobFileId));

        cancellationToken.ThrowIfCancellationRequested();

        var containerClient = await _defaultBlobContainerClient.Task.ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        return ContainerHelper.GetSharedAccessUriFromContainer(
            blobFileId,
            action,
            containerClient,
            _cloudFileStorageOptions.SharedAccessDuration!.Value);
    }
}
