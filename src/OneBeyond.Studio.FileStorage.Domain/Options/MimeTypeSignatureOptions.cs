using System.Collections.Generic;

namespace OneBeyond.Studio.FileStorage.Domain.Options;

public sealed record MimeTypeSignatureOptions
{
    /// <summary>
    /// </summary>
    public string MimeType { get; init; } = default!;

    /// <summary>
    /// </summary>
    public IReadOnlyCollection<string> Signatures { get; init; } = default!;
}
