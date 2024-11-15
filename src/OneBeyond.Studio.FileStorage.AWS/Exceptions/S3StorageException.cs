using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.FileStorage.Domain.Exceptions;

namespace OneBeyond.Studio.FileStorage.AWS.Exceptions;

[Serializable]
public sealed class S3StorageException : FileStorageException
{
    public S3StorageException(string message) : base(message)
    {
    }

    public S3StorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
