using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.FileStorage.AWS.Options;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.AWS.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IFileStorageBuilder UseAzureBlobs(
        this IFileStorageBuilder fileRepositoryBuilder,
        S3BlobFileStorageOptions options)
    {
        EnsureArg.IsNotNull(fileRepositoryBuilder, nameof(fileRepositoryBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        fileRepositoryBuilder.Services.AddSingleton(options);

        fileRepositoryBuilder.Services.AddTransient<IFileStorage, S3BlobFileStorage>();

        return fileRepositoryBuilder;
    }

    public static ICloudStorageBuilder UseAzureBlobs(
        this ICloudStorageBuilder cloudStorageBuilder,
        S3BlobCloudStorageOptions cloudStorageOptions)
    {
        EnsureArg.IsNotNull(cloudStorageBuilder, nameof(cloudStorageBuilder));
        EnsureArg.IsNotNull(cloudStorageOptions, nameof(cloudStorageOptions));

        cloudStorageBuilder.Services.AddSingleton(cloudStorageOptions);

        cloudStorageBuilder.Services.AddTransient<ICloudFileStorage, S3BlobCloudStorage>();

        return cloudStorageBuilder;
    }
}
