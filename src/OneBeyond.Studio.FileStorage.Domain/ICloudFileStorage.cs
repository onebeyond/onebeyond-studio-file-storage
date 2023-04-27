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
    /// <param name="tags">Tags that should be associated with the file</param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        Stream fileContent,
        string fileContentType,
        IDictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="fileName">Name of the file to be uploaded</param>
    /// <param name="fileContentType">File content type</param>
    /// <param name="fileContent">File data</param>
    /// <param name="tags">Tags that should be associated with the file</param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        byte[] fileContent,
        string fileContentType,
        IDictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update file tags
    /// </summary>
    /// <param name="fileRecord">File record for the file the tags need to be added to</param>
    /// <param name="tags">Tags that should be associated with the file</param>
    Task UpdateFileTagsAsync(
        FileRecord fileRecord, 
        IDictionary<string, string> tags, 
        CancellationToken cancellationToken = default);
}
