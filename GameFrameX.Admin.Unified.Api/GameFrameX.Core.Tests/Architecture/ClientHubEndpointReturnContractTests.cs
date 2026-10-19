using System.Reflection;
using GameFrameX.Foundation.Http.Normalization;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.Architecture;

/// <summary>
/// C227 防回潮契约：Client.Api / Hub.Api 的 API action 返回签名禁止 HttpJsonResultData&lt;string&gt; / &lt;object&gt;
/// 与裸 HttpJsonResultData（U9 统一 DTO 决策：对外 data 一律强类型对象，成功确认由信封 code==0 承载）。
/// 与 AdminEndpointReturnContractTests 同机制：反射扫描测试输出目录全部 GameFrameX 程序集，天然无视源码注释。
/// </summary>
public sealed class ClientHubEndpointReturnContractTests
{
    // C268 迁移豁免：GameUserPayController.Report 随 Game.Api 物理并入，保留 HttpJsonResultData<object> 历史 wire 形态
    // （合并零漂移前提；Game 控制器语义收敛/强类型 DTO 化由后续 change 处理，届时删除本条目）
    private static readonly HashSet<string> MigratedLegacyEnvelopes = new()
    {
        "GameFrameX.Client.Api.GameUserPay.GameUserPayController.Report",
    };

    [Fact]
    public void ClientHubApiActions_DoNotReturnStringDataEnvelope()
    {
        var assemblies = LoadClientHubAssemblies();
        // 防 vacuous pass：host dll 未进测试输出目录时本测试会静默通过，先断言扫描面真实存在
        Assert.True(assemblies.Count() == 2, $"必须加载 Client.Api 与 Hub.Api 两个 host 程序集，实际 {assemblies.Count()}（dll 未随测试项目输出时契约扫描为空转）");
        var controllerCount = assemblies.SelectMany(SafeGetTypes).Where(IsClientHubController).Count();
        Assert.True(controllerCount >= 10, $"host 控制器扫描面异常，实际仅 {controllerCount} 个（预期 Client.Api + Hub.Api 共 12+ 个控制器）");

        var violations = assemblies
                         .SelectMany(SafeGetTypes)
                         .Where(IsClientHubController)
                         .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                         .Where(method => !IsExcluded(method))
                         .Select(method => new { Method = method, ReturnType = UnwrapTask(method.ReturnType), })
                         .Where(item => IsForbiddenEnvelope(item.ReturnType))
                         .Select(item => $"{item.Method.DeclaringType!.FullName}.{item.Method.Name} -> {item.ReturnType.Name}")
                         .Order(StringComparer.Ordinal)
                         .Distinct(StringComparer.Ordinal)
                         .ToList();

        Assert.True(
            violations.Count == 0,
            "Client.Api / Hub.Api action 不得返回 HttpJsonResultData<string>/<object>/裸信封（C227：data 一律强类型 DTO）。违规方法："
            + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// 加载测试输出目录下 Client.Api / Hub.Api 程序集（host 本体，不含其引用的共享层）。
    /// </summary>
    private static IEnumerable<Assembly> LoadClientHubAssemblies()
    {
        return Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll")
                        .Where(path =>
                        {
                            var name = Path.GetFileNameWithoutExtension(path);
                            return name == "GameFrameX.Client.Api" || name == "GameFrameX.Hub.Api";
                        })
                        .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
                        .ToList();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>();
        }
    }

    /// <summary>
    /// Client/Hub host 控制器判据：ControllerBase 派生（含各自 BaseController&lt;T&gt; 基类链）。
    /// 仅扫两个 host 程序集本身，共享层服务不在此列。
    /// </summary>
    private static bool IsClientHubController(Type type)
    {
        return typeof(ControllerBase).IsAssignableFrom(type)
               && type.IsClass
               && type.IsPublic
               && !type.IsAbstract
               && !type.ContainsGenericParameters;
    }

    /// <summary>
    /// 违规信封判据：HttpJsonResultData 泛型参数为 string / object，或非泛型裸 HttpJsonResultData。
    /// </summary>
    private static bool IsForbiddenEnvelope(Type returnType)
    {
        if (returnType == typeof(HttpJsonResultData<string>) || returnType == typeof(HttpJsonResultData<object>))
        {
            return true;
        }

        if (returnType.IsGenericType
            && returnType.GetGenericTypeDefinition() == typeof(HttpJsonResultData<>)
            && returnType.GetGenericArguments()[0] == typeof(object))
        {
            return true;
        }

        // 裸 HttpJsonResultData（若存在非泛型形态）按类型名兜底，不引入对可能不存在类型的编译依赖
        return returnType.Name == "HttpJsonResultData" && !returnType.IsGenericType;
    }

    private static bool IsExcluded(MethodInfo method)
    {
        return method.GetCustomAttributes(true).OfType<NonActionAttribute>().Any()
               || MigratedLegacyEnvelopes.Contains(method.DeclaringType!.FullName + "." + method.Name);
    }

    private static Type UnwrapTask(Type returnType)
    {
        if (returnType == typeof(Task) || returnType == typeof(void))
        {
            return typeof(void);
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        return returnType;
    }
}