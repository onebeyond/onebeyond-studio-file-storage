using System.Linq;
using OneBeyond.Studio.FileStorage.AWS.Exceptions;

namespace OneBeyond.Studio.FileStorage.AWS.Helpers;

internal static class BlobHelper
{
    //This is designed to improve information about what is and is not valid for a given azure blob name.
    internal static void ValidateBlobName(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
        {
            throw new S3StorageException("Blob name cannot be empty.");
        }

        if (blobName.Length < 1 || blobName.Length > 1024)
        {
            throw new S3StorageException("Blob name must be between 1 and 1024 characters in length.");
        }

        if (blobName.EndsWith(".") || blobName.EndsWith("/"))
        {
            throw new S3StorageException("Blob name must not end with '.' or '/'");
        }

        var paths = blobName.Split('/');

        if (paths.Length > 254)
        {
            throw new S3StorageException("The number of '/' delimited segments cannot exceed 254.");
        }

        if (paths.Any(x => x.EndsWith(".")))
        {
            throw new S3StorageException("No path segment ('/' delimited segment) can end in a '.'");
        }
    }
}
