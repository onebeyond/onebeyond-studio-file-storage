using System;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.FileStorage.Domain.Options;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the file storage using the given builder action
    /// </summary>
    /// <param name="services"></param>
    /// <param name="fileStorageBuilderAction"></param>
    public static void AddFileStorage(
        this IServiceCollection services,
        Action<IFileStorageBuilder> fileStorageBuilderAction)
    {
        services.AddFileStorage(
            new MimeTypeValidationOptions
            {
                ValidationMode = MimeTypeValidationMode.Blacklist,
                MimeTypeSignatures = Array.Empty<MimeTypeSignatureOptions>()
            },
            fileStorageBuilderAction);
    }

    /// <summary>
    /// Registers the file storage using the given builder action and validation options
    /// </summary>
    /// <param name="services"></param>
    /// <param name="validationOptions"></param>
    /// <param name="fileStorageBuilderAction"></param>
    public static void AddFileStorage(
        this IServiceCollection services,
        MimeTypeValidationOptions validationOptions,
        Action<IFileStorageBuilder> fileStorageBuilderAction)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(validationOptions, nameof(validationOptions));
        EnsureArg.IsNotNull(fileStorageBuilderAction, nameof(fileStorageBuilderAction));

        services.AddSingleton(validationOptions);

        var fileStorageBuilder = new FileStorageBuilder(services);

        fileStorageBuilderAction(fileStorageBuilder);
    }


    /// <summary>
    /// Registers the file storage using the given cloud builder action
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cloudStorageBuilderAction"></param>
    public static void AddCloudStorage(
        this IServiceCollection services,
        Action<ICloudStorageBuilder> cloudStorageBuilderAction)
    {
        services.AddCloudStorage(
            new MimeTypeValidationOptions
            {
                ValidationMode = MimeTypeValidationMode.Blacklist,
                MimeTypeSignatures = Array.Empty<MimeTypeSignatureOptions>()
            },
            cloudStorageBuilderAction);
    }

    /// <summary>
    /// Registers the file storage using the given cloud builder action and validation options
    /// </summary>
    /// <param name="services"></param>
    /// <param name="validationOptions"></param>
    /// <param name="cloudStorageBuilderAction"></param>
    public static void AddCloudStorage(
        this IServiceCollection services,
        MimeTypeValidationOptions validationOptions,
        Action<ICloudStorageBuilder> cloudStorageBuilderAction)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(validationOptions, nameof(validationOptions));
        EnsureArg.IsNotNull(cloudStorageBuilderAction, nameof(cloudStorageBuilderAction));

        services.AddSingleton(validationOptions);

        var cloudStorageBuilder = new CloudStorageBuilder(services);
        cloudStorageBuilderAction(cloudStorageBuilder);
    }

}
