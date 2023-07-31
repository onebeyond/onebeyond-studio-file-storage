using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace OneBeyond.Studio.FileStorage.Azure.Options.Validators
{
    public partial class AzureStorageOptionsValidator<T> : IValidateOptions<T>
        where T : AzureBaseStorageOptions
    {
        public ValidateOptionsResult Validate(string? name, T options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.AccountName))
            {
                return ValidateOptionsResult.Fail("At least one connection must be provided, " +
                    "either the connection string or the account name (for Azure Identity usage).");
            }

            if (!string.IsNullOrWhiteSpace(options.ConnectionString) && !string.IsNullOrWhiteSpace(options.AccountName))
            {
                return ValidateOptionsResult.Fail("Only one connection can be provided, " +
                    "either the connection string or the account name (for Azure Identity usage).");
            }

            return ValidateContainerName(options.ContainerName);
        }


        /// <summary>
        /// This is designed to improve information about what is and is not valid for a given azure container name.
        /// </summary>
        private static ValidateOptionsResult ValidateContainerName(string? containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                return ValidateOptionsResult.Fail("Container name cannot be empty.");
            }

            if (containerName.Length < 3 || containerName.Length > 63)
            {
                return ValidateOptionsResult.Fail("Container name must be between 3 and 63 characters in length.");
            }

            if (!ContainerCharacterRegex().IsMatch(containerName))
            {
                return ValidateOptionsResult.Fail("Container name can only contain lowercase letters, numbers or hyphens.");
            }

            if (!ContainerFullValidityRegex().IsMatch(containerName))
            {
                return ValidateOptionsResult.Fail("Container name must start and end with a number or letter and cannot contain multiple hyphens in sequence.");
            }

            return ValidateOptionsResult.Success;
        }

        [GeneratedRegex("^[a-z0-9-]*$", RegexOptions.Compiled)]
        private static partial Regex ContainerCharacterRegex();

        [GeneratedRegex("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Compiled)]
        private static partial Regex ContainerFullValidityRegex();
    }
} 
}
