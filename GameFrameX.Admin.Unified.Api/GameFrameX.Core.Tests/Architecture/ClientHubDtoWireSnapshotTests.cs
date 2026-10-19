using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.Architecture;

// C233 wire 冻结守护（U10 硬验收）：Client.Api / Hub.Api / Core.Extension 的 Dto 命名空间与
// Client.Api Integrations 命名空间下全部 DTO 的「wire 属性名集合多重集」快照。
// 依据决策文档 gfx-doc/decisions/2026-08-17-gameframex-dto-naming-convention.md §三：两 host MVC 序列化
// NamingPolicy=null，DTO 属性名即 wire 字段名，已发布端点属性名零变化是硬边界；类名不进 wire（仅 Swagger
// schema 名），类名级 rename 必须令本快照零变化。
// 快照行不含类型名（多重集设计）：类名 rename 不影响比对；任何属性增删改名、enum 值变化、DTO 集合增删必令快照变化。
// 基线重生成：GFX_WIRE_SNAPSHOT_UPDATE=1 dotnet test（仅限快照范围本身调整，须 diff 核对后重跑）。
public sealed class ClientHubDtoWireSnapshotTests
{
    private static readonly string BaselinePath = Path.Combine(
        GetRepositoryRoot(),
        Path.Combine("GameFrameX.Core.Tests", "Architecture", "ClientHubDtoWireSnapshot.baseline.txt"));

    [Fact]
    public void DtoWirePropertyNames_MatchFrozenSnapshot()
    {
        var actualLines = BuildSnapshotLines().ToList();
        var actualText = string.Join(Environment.NewLine, actualLines);

        if (Environment.GetEnvironmentVariable("GFX_WIRE_SNAPSHOT_UPDATE") == "1")
        {
            File.WriteAllText(BaselinePath, actualText + Environment.NewLine);
            Assert.Fail("基线已重写（GFX_WIRE_SNAPSHOT_UPDATE=1），当前行数 " + actualLines.Count + "；请 diff 核对变化仅限快照范围调整后重跑。");
        }

        Assert.True(File.Exists(BaselinePath), "基线文件缺失：先以 GFX_WIRE_SNAPSHOT_UPDATE=1 生成。");
        var expectedText = File.ReadAllText(BaselinePath).TrimEnd();
        Assert.Equal(expectedText, actualText);
    }

    private static IEnumerable<string> BuildSnapshotLines()
    {
        var lines = new List<string>();
        foreach (var assembly in LoadSnapshotAssemblies())
        {
            foreach (var type in SafeGetTypes(assembly))
            {
                if (!IsSnapshotType(type))
                {
                    continue;
                }

                var kind = type.IsEnum ? "enum" : "class";
                var names = type.IsEnum
                                ? type.GetEnumNames().Order(StringComparer.Ordinal)
                                : type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(property => property.CanRead && property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                                      .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
                                      .Order(StringComparer.Ordinal);
                lines.Add(kind + "|" + string.Join(",", names));
            }
        }

        return lines.Order(StringComparer.Ordinal);
    }

    private static IEnumerable<Assembly> LoadSnapshotAssemblies()
    {
        var snapshotAssemblyNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "GameFrameX.Client.Api",
            "GameFrameX.Hub.Api",
            "GameFrameX.Core.Extension",
        };

        return Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll")
                        .Where(path => snapshotAssemblyNames.Contains(Path.GetFileNameWithoutExtension(path)))
                        .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
                        .ToList();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>();
        }
    }

    private static bool IsSnapshotType(Type type)
    {
        if (type.IsInterface || type.IsGenericTypeDefinition || type.ContainsGenericParameters)
        {
            return false;
        }

        // Integrations 命名空间同时住着 webhook controller 与其 DTO，controller 本体属性（HttpContext 等）不是 wire
        if (typeof(ControllerBase).IsAssignableFrom(type))
        {
            return false;
        }

        if (!type.IsClass && !type.IsEnum)
        {
            return false;
        }

        if (type.Name.Contains('<') || type.Name.StartsWith("DisplayClass", StringComparison.Ordinal))
        {
            return false;
        }

        var ns = type.Namespace ?? string.Empty;
        if (type.Assembly.GetName().Name == "GameFrameX.Client.Api")
        {
            return ns.Contains(".Dto", StringComparison.Ordinal) || ns.Contains(".Integrations", StringComparison.Ordinal);
        }

        return ns.Contains(".Dto", StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find GameFrameX.Admin.Unified.slnx.");
    }
}