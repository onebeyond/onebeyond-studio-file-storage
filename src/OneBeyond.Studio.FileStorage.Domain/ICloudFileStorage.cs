using OneBeyond.Studio.FileStorage.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.FileStorage.Domain;

/// <summary>
/// Interface for interacting directly with cloud storage asynchronously. This requests 
/// URLs with shared access tokens for upload and download, no additional checks or 
/// verifications are performed. This is currently up to the user.
/// </summary>
public interface ICloudFileStorage : IFileStorage
{
    /// <summary>
    /// Request the Download URL from a cloud storage blob, given a file ID.
    /// </summary>    
    Task<Uri> GetDownloadUrlAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request the Upload URL from a cloud storage blob, given a file ID.
    /// </summary>    
    Task<Uri> GetUploadUrlAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request the Delete URL from a cloud storage blob, given a file ID.
    /// </summary>
    Task<Uri> GetDeleteUrlAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="fileName">Name of the file to be uploaded</param>
    /// <param name="fileContentType">File content type</param>
    /// <param name="fileContent">File data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        Stream fileContent,
        string fileContentType,
        Dictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="fileName">Name of the file to be uploaded</param>
    /// <param name="fileContentType">File content type</param>
    /// <param name="fileContent">File data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        byte[] fileContent,
        string fileContentType,
        Dictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update file tags
    /// </summary>
    Task UpdateFileTagsAsync(FileRecord fileRecord, Dictionary<string, string> tags, CancellationToken cancellationToken = default);
}
