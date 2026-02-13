using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.FileStorage.Domain.Options;

namespace OneBeyond.Studio.FileStorage.Domain.Tests;

[TestClass]
public sealed class MimeTypeValidationOptionsTests
{
    [TestMethod]
    public void TestWhitelistValidationOptionsAreLoadedProperly()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var options = GetOptions<MimeTypeValidationOptions>(
            configuration,
            "FileStorage:MimeTypeValidation");

        Assert.AreEqual(MimeTypeValidationMode.Whitelist, options.ValidationMode);

        Assert.HasCount(2, options.MimeTypeSignatures);
        Assert.IsTrue(options.MimeTypeSignatures.Any((mimeTypeOptions) => mimeTypeOptions.MimeType == "image/png"));
        Assert.IsTrue(options.MimeTypeSignatures.Any((mimeTypeOptions) => mimeTypeOptions.MimeType == "image/jpeg"));

        var pngOptions = options.MimeTypeSignatures.Single((mimeTypeOptions) => mimeTypeOptions.MimeType == "image/png");
        Assert.HasCount(2, pngOptions.Signatures);

        var jpegOptions = options.MimeTypeSignatures.Single((mimeTypeOptions) => mimeTypeOptions.MimeType == "image/jpeg");
        Assert.HasCount(1, jpegOptions.Signatures);
    }

    private static TOptions GetOptions<TOptions>(IConfiguration configuration, string sectionKey)
        => configuration
            .GetSection(sectionKey)
            .Get<TOptions>()!;
}
