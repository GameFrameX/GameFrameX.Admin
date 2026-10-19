namespace GameFrameX.Core.Tests.Architecture;

// C370（GFX-829）：业务凭证静态加密写入 / 读取路径守护。
// change：gfx-doc/dev/system-management/changes/C370-tenant-secret-crypto-foundation/change.md
// 三表写入路径必须经 TenantSecretCrypto / TenantObjectStorageConfigCrypto 收口（写入加密），
// 读取点必须走解密收口（发信构建 / 缓存加载 / Adapter 上下文构建）。
public sealed class TenantSecretCryptoGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    [Theory]
    [InlineData("GameFrameX.Application.Game/System/Email/SystemEmailProviderService.cs", "TenantSecretCrypto.Encrypt")]
    [InlineData("GameFrameX.Application.System/OpenAccess/SystemOpenAccessService.cs", "TenantSecretCrypto.Encrypt")]
    [InlineData("GameFrameX.Application.System/OpenAccess/SystemOpenAccessService.cs", "TenantSecretCrypto.Decrypt")]
    [InlineData("GameFrameX.Application.Game/Tenant/Integration/TenantObjectStorageConfig/TenantObjectStorageConfigEntityService.cs", "TenantObjectStorageConfigCrypto.EncryptConfigJson")]
    [InlineData("GameFrameX.Application.Game/Tenant/Integration/TenantObjectStorageConfig/TenantObjectStorageConfigEntityService.cs", "TenantObjectStorageConfigCrypto.DecryptConfigJson")]
    [InlineData("GameFrameX.Application.Game/System/Email/SystemEmailDispatcher.cs", "TenantSecretCrypto.Decrypt")]
    [InlineData("GameFrameX.Core/Service/File/TenantFileService.cs", "TenantObjectStorageConfigCrypto.DecryptConfigJson")]
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
