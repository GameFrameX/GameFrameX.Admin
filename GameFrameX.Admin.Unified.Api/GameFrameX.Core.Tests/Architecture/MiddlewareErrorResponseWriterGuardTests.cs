using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C270（GFX-676）：中间件错误响应出口统一守护。
// change：gfx-doc/dev/client-api/changes/C270-gfx-676-middleware-error-response-writer/change.md
// 两条守护（防回潮）：
//   G1 Middleware/ 目录下中间件不得私写 JSON 序列化错误响应（JsonSerializer.Serialize 仅允许共享 writer；
//      JsonSerializer.Deserialize 解析凭证 Scopes 等合法用途不受限制）
//   G2 Middleware/ 目录下中间件不得直接 Response.WriteAsync 错误响应（仅允许共享 writer；
//      CryptoEnvelopeMiddleware 响应侧加密回写用的是 originalBody.WriteAsync（局部流变量），不命中本模式）
public sealed class MiddlewareErrorResponseWriterGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    // 共享 writer 自身是唯一合法私写点
    private static readonly string WriterSelfPath = "GameFrameX.Core/Middleware/MiddlewareResponseWriter.cs";

    [Fact]
    public void G1_MiddlewareDirectory_NoPrivateJsonSerialization()
    {
        var pattern = new Regex(@"JsonSerializer\.Serialize\s*\(", RegexOptions.Compiled);
        var offenders = EnumerateMiddlewareSources()
                        .Where(f => f != WriterSelfPath)
                        .Where(f => pattern.IsMatch(File.ReadAllText(Path.Combine(RepositoryRoot, f))))
                        .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void G2_MiddlewareDirectory_NoDirectResponseWriteAsync()
    {
        var pattern = new Regex(@"Response\.WriteAsync\s*\(", RegexOptions.Compiled);
        var offenders = EnumerateMiddlewareSources()
                        .Where(f => f != WriterSelfPath)
                        .Where(f => pattern.IsMatch(File.ReadAllText(Path.Combine(RepositoryRoot, f))))
                        .ToList();
        Assert.Empty(offenders);
    }

    /// <summary>
    /// 枚举 GameFrameX.* 顶层产品目录下 Middleware/ 目录中的全部 .cs 源文件（相对路径，/ 分隔；排除 obj/bin 构建产物）。
    /// </summary>
    private static IEnumerable<string> EnumerateMiddlewareSources()
    {
        var root = new DirectoryInfo(RepositoryRoot);
        foreach (var projectDir in root.EnumerateDirectories("GameFrameX.*"))
        {
            foreach (var file in projectDir.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(RepositoryRoot, file.FullName).Replace(Path.DirectorySeparatorChar, '/');
                var segments = relative.Split('/');
                if (segments.Contains("obj") || segments.Contains("bin") || !segments.Contains("Middleware"))
                {
                    continue;
                }

                yield return relative;
            }
        }
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