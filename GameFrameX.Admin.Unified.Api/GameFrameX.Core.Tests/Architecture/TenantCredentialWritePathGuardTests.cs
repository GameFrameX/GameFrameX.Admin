namespace GameFrameX.Core.Tests.Architecture;

// C371（GFX-838）T7：租户域 10 实体凭证写入路径源码级守护——每个落库写入点必须出现 TenantSecretCrypto.Encrypt
// （密钥环类）或 *ConfigCrypto.EncryptConfigJson（JSON 字段级类）收口调用。
// change：gfx-doc/dev/game-business/changes/C371-tenant-credential-encryption-application/change.md
// 新增凭证实体 / 新增写入方法时若绕过收口，本测试失败，防止明文落库回潮。
public sealed class TenantCredentialWritePathGuardTests
{
    /// <summary>
    /// 源文件相对路径 → 必须包含的收口标记（出现次数下限 = 写入点数）。
    /// </summary>
    private static readonly (string File, string Marker, int MinOccurrences)[] GuardedWritePaths =
    {
        // 密钥环 / 凭证列直加密（Application.Game）
        ("GameFrameX.Application.Game/Tenant/Security/TenantAppSignatureKey/TenantAppSignatureKeyEntityService.cs", "TenantSecretCrypto.Encrypt", 2),
        ("GameFrameX.Application.Game/Tenant/Security/TenantSessionKey/TenantSessionKeyEntityService.cs", "TenantSecretCrypto.Encrypt", 2),
        ("GameFrameX.Application.Game/Tenant/LoginTicket/TenantGameLoginTicketKey/TenantGameLoginTicketKeyEntityService.cs", "TenantSecretCrypto.Encrypt", 1),
        ("GameFrameX.Application.Game/Tenant/ClientApiPolicy/TenantClientApiPolicy/TenantClientApiPolicyEntityService.cs", "TenantSecretCrypto.Encrypt", 2),
        ("GameFrameX.Application.Game/Tenant/OpenAccess/TenantGameAppOpenAccess/TenantGameAppOpenAccessService.cs", "TenantSecretCrypto.Encrypt", 2),
        ("GameFrameX.Application.Game/Tenant/Integration/TenantEmailConfig/TenantEmailConfigEntityService.cs", "TenantSecretCrypto.Encrypt", 4),
        ("GameFrameX.Application.Game/Tenant/Integration/TenantSmsConfig/TenantSmsConfigEntityService.cs", "TenantSecretCrypto.Encrypt", 4),
        ("GameFrameX.Application.Game/Tenant/Integration/TenantWechatConfig/TenantWechatConfigEntityService.cs", "TenantSecretCrypto.Encrypt", 4),
        ("GameFrameX.Application.Game/Tenant/Integration/TenantAdvertisementCallbackConfig/TenantAdvertisementCallbackConfigEntityService.cs", "TenantSecretCrypto.Encrypt", 4),
        ("GameFrameX.Application.Game/Tenant/Integration/TenantAdvertisementCallbackConfig/TenantAdvertisementCallbackConfigEntityService.cs", "AdvertisementCallbackConfigCrypto.EncryptConfigJson", 2),
        ("GameFrameX.Application.Game/Order/GamePaymentConfig/TenantGamePaymentConfigEntityService.cs", "PaymentConfigCrypto.EncryptConfigJson", 2),
        // 运行时签发路径（Client.Api）
        ("GameFrameX.Client.Api/Services/Auth/ClientApiSessionKeyIssuer.cs", "TenantSecretCrypto.Encrypt", 1),
        ("GameFrameX.Client.Api/Services/GameLoginTicket/GameLoginTicketKeyResolver.cs", "TenantSecretCrypto.Encrypt", 1),
    };

    [Fact]
    public void CredentialWritePaths_AllPassCryptoFacade()
    {
        var repositoryRoot = GetRepositoryRoot();
        var violations = new List<string>();
        foreach (var (relativePath, marker, minOccurrences) in GuardedWritePaths)
        {
            var fullPath = Path.Combine(repositoryRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                violations.Add($"文件缺失：{relativePath}");
                continue;
            }

            var source = File.ReadAllText(fullPath);
            var occurrences = CountOccurrences(source, marker);
            if (occurrences < minOccurrences)
            {
                violations.Add($"{relativePath} 收口标记 {marker} 出现 {occurrences} 次，要求 ≥ {minOccurrences}（凭证写入必须经加密收口）");
            }
        }

        Assert.True(violations.Count == 0, "凭证写入路径存在未加密落库点：\n" + string.Join("\n", violations));
    }

    private static int CountOccurrences(string source, string marker)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(marker, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += marker.Length;
        }

        return count;
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
