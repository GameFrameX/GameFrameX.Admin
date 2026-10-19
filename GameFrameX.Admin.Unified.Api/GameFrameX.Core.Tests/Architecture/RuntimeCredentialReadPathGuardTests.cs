using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C372（GFX-838）T4：运行时凭证读取点源码级守护——清单见
// gfx-doc/dev/client-api/changes/C372-runtime-credential-decrypt-closure/notes.md（T1 盘点表）。
// 1）每个读取点必须出现对应解密收口调用（新增读取点 / 删除收口调用即失败，防止明文兼容读回潮成永久态）；
// 2）明文零泄漏：读取点源码中日志 / 异常语句不得携带敏感凭证字段名（解密结果禁止进入日志与异常参数）。
public sealed partial class RuntimeCredentialReadPathGuardTests
{
    /// <summary>
    /// 源文件相对路径 → 必须包含的解密收口标记（出现次数下限）。
    /// </summary>
    private static readonly (string File, string Marker, int MinOccurrences)[] GuardedReadPaths =
    {
        // Client.Api 六链路
        ("GameFrameX.Core/Middleware/SignatureKeyResolver.cs", "TenantSecretCrypto.DecryptCached", 1),
        ("GameFrameX.Core/Middleware/SessionKeyResolver.cs", "TenantSecretCrypto.DecryptCached", 1),
        ("GameFrameX.Client.Api/Services/GameLoginTicket/GameLoginTicketKeyResolver.cs", "TenantSecretCrypto.DecryptCached", 1),
        ("GameFrameX.Client.Api/Services/ClientApiTenantOptions/ClientApiTenantOptionsProvider.cs", "TenantSecretCrypto.DecryptCached", 3),
        ("GameFrameX.Client.Api/Services/ClientApiTenantOptions/ClientApiTenantOptionsProvider.cs", "AdvertisementCallbackConfigCrypto.DecryptConfigJson", 1),
        ("GameFrameX.Client.Api/Services/Email/TenantEmailSender.cs", "TenantSecretCrypto.DecryptCached", 2),
        ("GameFrameX.Client.Api/Integrations/Payment/PaymentOptionSettingProvider.cs", "PaymentConfigCrypto.DecryptConfigJson", 1),
        // Hub.Api 链路
        ("GameFrameX.Hub.Api/Middleware/TenantGameAppOpenAccessAuthMiddleware.cs", "TenantSecretCrypto.DecryptCached", 1),
        // C370 已收口的主库域读取（防回退）
        ("GameFrameX.Application.System/OpenAccess/SystemOpenAccessService.cs", "TenantSecretCrypto.Decrypt", 1),
    };

    /// <summary>
    /// 明文零泄漏扫描的敏感字段名（读取点内解密操作的目标列）。
    /// </summary>
    private static readonly string[] SensitiveFieldNames =
    {
        "FromEmailSecretId", "FromEmailSecretKey", "AccessSecret", "SigningSecret", "VerificationKeys", "PrivateKeyPem", "SigningKey",
    };

    /// <summary>
    /// 日志 / 异常语句捕获（粗粒度）：LogHelper.Xxx(...) / Console.WriteLine(...) / throw new XxxException(...) 的参数区段。
    /// </summary>
    [GeneratedRegex(@"(?:LogHelper\.\w+|Console\.WriteLine|throw new \w+Exception)\s*\(([^;]*?)\)", RegexOptions.ExplicitCapture)]
    private static partial Regex LogOrThrowStatementRegex();

    [Fact]
    public void CredentialReadPaths_AllPassDecryptClosure()
    {
        var repositoryRoot = GetRepositoryRoot();
        var violations = new List<string>();
        foreach (var (relativePath, marker, minOccurrences) in GuardedReadPaths)
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
                violations.Add($"{relativePath} 解密收口标记 {marker} 出现 {occurrences} 次，要求 ≥ {minOccurrences}（凭证读取必须经解密收口）");
            }
        }

        Assert.True(violations.Count == 0, "凭证读取路径存在未解密读取点：\n" + string.Join("\n", violations));
    }

    [Fact]
    public void CredentialReadPaths_DecryptedFieldsNeverReachLogOrThrowStatements()
    {
        var repositoryRoot = GetRepositoryRoot();
        var violations = new List<string>();
        foreach (var (relativePath, _, _) in GuardedReadPaths)
        {
            var source = File.ReadAllText(Path.Combine(repositoryRoot, relativePath));
            foreach (Match statement in LogOrThrowStatementRegex().Matches(source))
            {
                foreach (var fieldName in SensitiveFieldNames)
                {
                    if (statement.Groups[1].Value.Contains(fieldName, StringComparison.Ordinal))
                    {
                        violations.Add($"{relativePath} 日志/异常语句携带敏感字段 {fieldName}：{statement.Value}");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0, "解密后的敏感凭证泄漏进日志/异常语句：\n" + string.Join("\n", violations));
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
