namespace GameFrameX.Core.Tests.Architecture;

// C370（GFX-829）：业务凭证静态加密写入 / 读取路径守护。
// change：gfx-doc/dev/system-management/changes/C370-secret-crypto-foundation/change.md
// 三表写入路径必须经 SecretCrypto / ObjectStorageConfigCrypto 收口（写入加密），
// 读取点必须走解密收口（发信构建 / 缓存加载 / Adapter 上下文构建）。
public sealed class SecretCryptoGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    [Theory]
    [InlineData("GameFrameX.Application/Platform/Email/SystemEmailProviderService.cs", "SecretCrypto.Encrypt")]
    [InlineData("GameFrameX.Application/Platform/OpenAccess/SystemOpenAccessService.cs", "SecretCrypto.Encrypt")]
    [InlineData("GameFrameX.Application/Platform/OpenAccess/SystemOpenAccessService.cs", "SecretCrypto.Decrypt")]
    [InlineData("GameFrameX.Application/ClientDelivery/ObjectStorageConfig/ObjectStorageConfigService.cs", "ObjectStorageConfigCrypto.EncryptConfigJson")]
    [InlineData("GameFrameX.Application/ClientDelivery/ObjectStorageConfig/ObjectStorageConfigService.cs", "ObjectStorageConfigCrypto.DecryptConfigJson")]
    [InlineData("GameFrameX.Application/Platform/Email/SystemEmailDispatcher.cs", "SecretCrypto.Decrypt")]
    [InlineData("GameFrameX.Core/Service/File/FileService.cs", "ObjectStorageConfigCrypto.DecryptConfigJson")]
    public void CredentialPath_MustRouteThroughSecretCrypto(string relativePath, string requiredCall)
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot, relativePath));

        Assert.Contains(requiredCall, source, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }
}
