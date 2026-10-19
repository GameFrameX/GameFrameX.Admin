using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;

namespace GameFrameX.Core.Job;

/// <summary>
/// 动态 C# 代码编译器，集中承接运行时作业和动态插件编译。
/// </summary>
public static class DynamicCSharpCompiler
{
    /// <summary>
    /// 编译完整 C# 源码并加载为内存程序集。
    /// </summary>
    /// <param name="csharpCode">完整 C# 源码。</param>
    /// <param name="assemblyName">可选程序集名称。</param>
    /// <param name="additionalAssemblies">额外引用程序集。</param>
    /// <returns>已加载的动态程序集。</returns>
    public static Assembly Compile(string csharpCode, string? assemblyName = null, params Assembly[] additionalAssemblies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(csharpCode);

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
        var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode, parseOptions);
        var references = GetMetadataReferences(additionalAssemblies);
        var compilation = CSharpCompilation.Create(
            string.IsNullOrWhiteSpace(assemblyName) ? $"DynamicAssembly_{SnowFlakeIdHelper.Instance.NextId()}" : assemblyName,
            new[] { syntaxTree, },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var assemblyStream = new MemoryStream();
        var result = compilation.Emit(assemblyStream);
        if (!result.Success)
        {
            var errors = result.Diagnostics
                               .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                               .Select(diagnostic => diagnostic.ToString());
            throw BusinessError.Oh($"动态 C# 编译失败:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        assemblyStream.Position = 0;
        return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
    }

    private static IReadOnlyCollection<MetadataReference> GetMetadataReferences(IEnumerable<Assembly> additionalAssemblies)
    {
        // ponytail: 引用当前 AppDomain 已加载程序集；若后续需要隔离插件依赖，升级为显式引用白名单。
        return AppDomain.CurrentDomain.GetAssemblies()
                        .Concat(additionalAssemblies)
                        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                        .Select(assembly => assembly.Location)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Select(location => MetadataReference.CreateFromFile(location))
                        .ToArray();
    }
}