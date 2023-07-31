using System;

namespace OneBeyond.Studio.FileStorage.Azure.Options;

public abstract partial record AzureBaseStorageOptions
{
    /// <summary>
    /// The name of the Azure Storage Account to use for Azure Identity authentication.
    /// NOTE: If specified, the <ref>ConnectionString</ref> must not be provided.
    /// </summary>
    public string? AccountName { get; init; }

    /// <summary>
    /// The connection string to use for authentication.
    /// NOTE: if specified, the <ref>AccountName</ref> must not be provided.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// The name of the container to use for file storage.
    /// </summary>
    public string? ContainerName { get; init; }

    /// <summary>
    /// The duration of time, for which a shared access link to the file will be generated (if needed).
    /// </summary>
    public TimeSpan? SharedAccessDuration { get; init; }
}