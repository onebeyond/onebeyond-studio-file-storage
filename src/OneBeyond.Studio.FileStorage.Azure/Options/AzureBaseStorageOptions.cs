using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace OneBeyond.Studio.FileStorage.Azure.Options;

public abstract record AzureBaseStorageOptions
{
    private static readonly Regex _containerCharacterRegex = new(@"^[a-z0-9-]*$", RegexOptions.Compiled);
    private static readonly Regex _containerFullValidityRegex = new(@"^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Compiled);

    /// <summary>
    /// The name of the Azure Storage Account to use for Azure Identity authentication.
    /// NOTE: If specified, a connection string must not be provided.
    /// </summary>
    public string? AccountName { get; init; }

    /// <summary>
    /// The connection string to use for authentication.
    /// NOTE: if specified, a resource name must not be provided.
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

    public virtual void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString) && string.IsNullOrWhiteSpace(AccountName))
        {
            throw new ArgumentException("At least one connection must be provided, " +
                "either the connection string or the account name (for Azure Identity usage).");
        }

        if (!string.IsNullOrWhiteSpace(ConnectionString) && !string.IsNullOrWhiteSpace(AccountName))
        {
            throw new ArgumentException("Only one connection can be provided, " +
                "either the connection string or the account name (for Azure Identity usage).");
        }

        ValidateContainerName(ContainerName);
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

        if (!_containerCharacterRegex.IsMatch(containerName))
        {
            throw new AzureStorageException("Container name can only contain lowercase letters, numbers or hyphens.");
        }

        if (!_containerFullValidityRegex.IsMatch(containerName))
        {
            throw new AzureStorageException("Container name must start and end with a number or letter and cannot contain multiple hyphens in sequence.");
        }
    }
}
