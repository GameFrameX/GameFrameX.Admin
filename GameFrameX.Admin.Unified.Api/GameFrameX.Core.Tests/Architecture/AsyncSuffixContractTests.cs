using System.Reflection;

namespace GameFrameX.Core.Tests.Architecture;

// C289（GFX-696）异步命名规范守护：非豁免程序集中 public/internal 方法返回 Task/ValueTask（含泛型）
// 必须以 Async 结尾。豁免清单与 gfx-doc C289 change.md 同源：
//   - 程序集级：Core.Tests（测试方法）、Client.Api / Hub.Api（Controller action + SignalR Hub 是对外协议面，
//     [Route("api/[controller]/[action]")] 下方法名即路由段，改名即变路由）；
//   - 类型级：Payment.Apple.AppStoreServer.AppStoreServerApiClient（Apple 官方移植代码，最小 diff 原则）。
// 接口方法经接口声明本身检查（接口方法 IsPublic）；显式接口实现（private 可见性）由接口侧覆盖。
// 新增豁免必须先在本测试登记并在 change 文档记录理由。
public sealed class AsyncSuffixContractTests
{
    private static readonly HashSet<string> ExemptAssemblyNames = new(StringComparer.Ordinal)
    {
        "GameFrameX.Core.Tests",
        "GameFrameX.Client.Api",
        "GameFrameX.Hub.Api",
    };

    private static readonly HashSet<string> ExemptTypeMetadataNames = new(StringComparer.Ordinal)
    {
        // Apple 官方移植代码，最小 diff 移植原则（C289 豁免清单）
        "GameFrameX.Payment.Apple.AppStoreServer.AppStoreServerApiClient",
    };

    [Fact]
    public void PublicInternalTaskReturningMethods_HaveAsyncSuffix()
    {
        var offenders = new List<string>();

        foreach (var assemblyPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (ExemptAssemblyNames.Contains(assemblyName))
            {
                continue;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath));
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (type.Name.Contains('<') || ExemptTypeMetadataNames.Contains(type.FullName))
                {
                    continue;
                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    // 守护口径与 C289 规范一致：public / internal / protected internal
                    if (!method.IsPublic && !method.IsAssembly && !method.IsFamilyOrAssembly)
                    {
                        continue;
                    }

                    if (!IsTaskLike(method.ReturnType))
                    {
                        continue;
                    }

                    if (!method.Name.EndsWith("Async", StringComparison.Ordinal))
                    {
                        offenders.Add(type.FullName + "." + method.Name + " : " + method.ReturnType.Name);
                    }
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "以下 public/internal Task 方法缺 Async 后缀（新增豁免须在本测试与 C289 change 文档登记）：\n" + string.Join("\n", offenders.Order(StringComparer.Ordinal)));
    }

    private static bool IsTaskLike(Type returnType)
    {
        var target = Nullable.GetUnderlyingType(returnType) ?? returnType;
        if (target == typeof(Task) || target == typeof(ValueTask))
        {
            return true;
        }

        return target.IsGenericType
               && target.GetGenericTypeDefinition() is var definition
               && (definition == typeof(Task<>) || definition == typeof(ValueTask<>));
    }
}
