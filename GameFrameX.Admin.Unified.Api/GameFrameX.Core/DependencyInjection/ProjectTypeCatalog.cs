namespace GameFrameX.Core.DependencyInjection;

/// <summary>
/// 提供项目运行期类型目录，用于替代 历史框架 全局类型集合。
/// </summary>
public static class ProjectTypeCatalog
{
    private static readonly Lazy<IReadOnlyCollection<Type>> Types = new(LoadEffectiveTypes);

    public static IReadOnlyCollection<Type> EffectiveTypes
    {
        get { return Types.Value; }
    }

    public static IReadOnlyCollection<Assembly> EffectiveAssemblies
    {
        get { return EffectiveTypes.Select(type => type.Assembly).Distinct().ToArray(); }
    }

    private static IReadOnlyCollection<Type> LoadEffectiveTypes()
    {
        LoadProjectAssembliesFromBaseDirectory();

        return AppDomain.CurrentDomain.GetAssemblies()
                        .Where(IsProjectAssembly)
                        .SelectMany(GetLoadableTypes)
                        .Where(type => !type.IsGenericTypeDefinition)
                        .ToArray();
    }

    private static void LoadProjectAssembliesFromBaseDirectory()
    {
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX.*.dll", SearchOption.TopDirectoryOnly))
        {
            var assemblyName = AssemblyName.GetAssemblyName(path);
            if (!IsProjectAssemblyName(assemblyName.Name)
                || AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == assemblyName.Name))
            {
                continue;
            }

            Assembly.Load(assemblyName);
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }

    private static bool IsProjectAssembly(Assembly assembly)
    {
        return IsProjectAssemblyName(assembly.GetName().Name);
    }

    private static bool IsProjectAssemblyName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name)
               && name.StartsWith("GameFrameX.", StringComparison.Ordinal)
               && !name.StartsWith("GameFrameX.Plugin.", StringComparison.Ordinal);
    }
}