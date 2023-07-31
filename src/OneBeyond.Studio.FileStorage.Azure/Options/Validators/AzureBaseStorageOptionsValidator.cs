using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using System.Text.RegularExpressions;
using System;

namespace OneBeyond.Studio.FileStorage.Azure.Options.Validators
{
    internal static partial class AzureBaseStorageOptionsValidator
    {
        public static void EnsureIsValid(this AzureBaseStorageOptions options)
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
}
