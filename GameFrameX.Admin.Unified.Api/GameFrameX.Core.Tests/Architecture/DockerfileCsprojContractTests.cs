using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// Dockerfile restore 层 csproj 清单守护测试。
// 三个 Dockerfile（web-api / client-api / hub-api）的 restore 层逐个 COPY csproj 以利用构建缓存，
// 清单必须与对应 host 的 ProjectReference 传递闭包完全一致，否则：
//   - 清单缺项（新增项目未同步）→ dotnet restore 找不到引用目标，构建失败；
//   - 清单多项（项目已删除/合并未同步）→ COPY 不存在的文件，构建直接报错。
// 2026-08-22 曾因 Application 重组（Application.Client/Tenant/Core.Enum 归位删除、Advertisement 引入）
// 三清单集体漂移，下一次 main push 构建必挂，由本测试族防止回潮。
public sealed class DockerfileCsprojContractTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    public static TheoryData<string, string> HostDockerfiles => new()
    {
        { "Dockerfile.web-api", "GameFrameX.Web.Api/GameFrameX.Web.Api.csproj" },
        { "Dockerfile.client-api", "GameFrameX.Client.Api/GameFrameX.Client.Api.csproj" },
        { "Dockerfile.hub-api", "GameFrameX.Hub.Api/GameFrameX.Hub.Api.csproj" },
    };

    [Theory]
    [MemberData(nameof(HostDockerfiles))]
    public void Dockerfile_CopyList_MatchesProjectReferenceClosure(string dockerfile, string hostCsproj)
    {
        var listed = ExtractDockerfileCopyList(dockerfile);
        var closure = ComputeProjectClosure(hostCsproj);

        var missing = closure.Except(listed, StringComparer.Ordinal).OrderBy(p => p).ToList();
        Assert.True(missing.Count == 0,
            $"{dockerfile} 的 COPY 清单缺少 host 传递闭包内的项目（restore 会失败）：" + string.Join(", ", missing));

        var stale = listed.Except(closure, StringComparer.Ordinal).OrderBy(p => p).ToList();
        Assert.True(stale.Count == 0,
            $"{dockerfile} 的 COPY 清单含闭包外的多余项目（项目已删除则 COPY 直接报错）：" + string.Join(", ", stale));
    }

    [Theory]
    [MemberData(nameof(HostDockerfiles))]
    public void Dockerfile_CopyList_AllFilesExist(string dockerfile, string hostCsproj)
    {
        var listed = ExtractDockerfileCopyList(dockerfile);

        var nonexistent = listed.Where(p => !File.Exists(Path.Combine(RepositoryRoot, p))).ToList();
        Assert.True(nonexistent.Count == 0,
            $"{dockerfile} 的 COPY 清单引用了不存在的文件：" + string.Join(", ", nonexistent));
    }

    /// <summary>提取 Dockerfile restore 层 COPY 的 csproj 相对路径清单。</summary>
    private static HashSet<string> ExtractDockerfileCopyList(string dockerfile)
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot, dockerfile));
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(source, @"COPY \[""([^""\\]+\.csproj)"""))
        {
            result.Add(match.Groups[1].Value);
        }

        return result;
    }

    /// <summary>从 host csproj 出发解析 ProjectReference 传递闭包（统一路径分隔符，返回仓库相对路径）。</summary>
    private static HashSet<string> ComputeProjectClosure(string hostCsproj)
    {
        var references = LoadAllProjectReferences();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        stack.Push(hostCsproj);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!seen.Add(current))
            {
                continue;
            }

            foreach (var dep in references(current))
            {
                stack.Push(dep);
            }
        }

        // 清单含 host 自身 csproj（restore 入口），闭包保留 host 不移除
        return seen;
    }

    private static Func<string, IEnumerable<string>> LoadAllProjectReferences()
    {
        var cache = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var csproj in Directory.EnumerateFiles(RepositoryRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var normalized = Path.GetRelativePath(RepositoryRoot, csproj).Replace('\\', '/');
            if (normalized.Contains("/obj/", StringComparison.Ordinal) || normalized.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(csproj);
            var baseDir = Path.GetDirectoryName(csproj)!;
            var deps = new List<string>();
            foreach (Match match in Regex.Matches(source, @"ProjectReference Include=""([^""]+)"""))
            {
                var relative = match.Groups[1].Value.Replace('\\', '/');
                var absolute = Path.GetFullPath(Path.Combine(baseDir, relative));
                deps.Add(Path.GetRelativePath(RepositoryRoot, absolute).Replace('\\', '/'));
            }

            cache[normalized] = deps;
        }

        IEnumerable<string> Lookup(string project)
        {
            return cache.TryGetValue(project, out var deps) ? deps : Array.Empty<string>();
        }

        return Lookup;
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
