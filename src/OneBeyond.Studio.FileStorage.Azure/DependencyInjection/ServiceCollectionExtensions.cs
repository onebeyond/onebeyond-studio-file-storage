using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Azure.Options.Validators;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.Azure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IFileStorageBuilder UseAzureBlobs(
        this IFileStorageBuilder fileRepositoryBuilder,
        AzureBlobFileStorageOptions options)
    {
        EnsureArg.IsNotNull(fileRepositoryBuilder, nameof(fileRepositoryBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        fileRepositoryBuilder.Services.AddSingleton(options);
        fileRepositoryBuilder.Services.AddSingleton<IValidateOptions
                              <AzureBlobFileStorageOptions>, AzureStorageOptionsValidator<AzureBlobFileStorageOptions>>();

        fileRepositoryBuilder.Services.AddTransient<IFileStorage, AzureBlobFileStorage>();

        return fileRepositoryBuilder;
    }

    public static ICloudStorageBuilder UseAzureBlobs(
        this ICloudStorageBuilder cloudStorageBuilder,
        AzureBlobCloudStorageOptions cloudStorageOptions)
    {
        EnsureArg.IsNotNull(cloudStorageBuilder, nameof(cloudStorageBuilder));
        EnsureArg.IsNotNull(cloudStorageOptions, nameof(cloudStorageOptions));

        cloudStorageBuilder.Services.AddSingleton(cloudStorageOptions);
        cloudStorageBuilder.Services.AddSingleton<IValidateOptions
                             <AzureBlobCloudStorageOptions>, AzureStorageOptionsValidator<AzureBlobCloudStorageOptions>>();

        cloudStorageBuilder.Services.AddTransient<ICloudFileStorage, AzureBlobCloudStorage>();

        return cloudStorageBuilder;
    }
}
