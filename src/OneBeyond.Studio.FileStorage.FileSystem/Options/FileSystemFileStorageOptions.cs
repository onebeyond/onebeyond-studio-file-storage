namespace OneBeyond.Studio.FileStorage.FileSystem.Options;

public sealed record FileSystemFileStorageOptions
{
    public string StorageRootPath { get; init; } = default!;
    public bool AllowDownloadUrl { get; init; }
}
