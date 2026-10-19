using System.Reflection;
using GameFrameX.Core.Api;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GameFrameX.Core.Tests.Architecture;

// C289 全量端点 golden 冻结（GFX-696 硬验收）：对全部 IAdminApiController 实现的 public action
// 纯内存套用 AdminApiControllerConvention（与运行时同一条路由推导代码路径，含 GET 单参数路径化），
// 冻结 {类型|路由模板|谓词} 全量快照。方法名是内部实现不入快照（C289 批量加 Async 后缀正是靠此设计
// 证明路由零漂移）；任何端点漂移（路由 / 谓词 / 参数路径化变化）必令本快照变红，此后作为路由零漂移的永久守护。
// 与 ThreeHostApiRoutingContractTests（5 端点样本断言）互补：本测试是全量枚举。
// 基线重生成：GFX_ADMIN_ENDPOINT_GOLDEN_UPDATE=1 dotnet test（仅限合法端点调整，须 diff 核对后重跑）。
public sealed class AdminEndpointGoldenSnapshotTests
{
    private static readonly string BaselinePath = Path.Combine(
        GetRepositoryRoot(),
        Path.Combine("GameFrameX.Core.Tests", "Architecture", "AdminEndpointGolden.baseline.txt"));

    [Fact]
    public void AdminEndpoints_MatchGoldenSnapshot()
    {
        var actualLines = BuildSnapshotLines().ToList();
        var actualText = string.Join(Environment.NewLine, actualLines);

        if (Environment.GetEnvironmentVariable("GFX_ADMIN_ENDPOINT_GOLDEN_UPDATE") == "1")
        {
            File.WriteAllText(BaselinePath, actualText + Environment.NewLine);
            Assert.Fail("基线已重写（GFX_ADMIN_ENDPOINT_GOLDEN_UPDATE=1），当前行数 " + actualLines.Count + "；请 diff 核对变化仅限合法端点调整后重跑。");
        }

        Assert.True(File.Exists(BaselinePath), "基线文件缺失：先以 GFX_ADMIN_ENDPOINT_GOLDEN_UPDATE=1 生成。");
        var expectedText = File.ReadAllText(BaselinePath).TrimEnd();
        Assert.Equal(expectedText, actualText);
    }

    private static IEnumerable<string> BuildSnapshotLines()
    {
        var applicationModel = new ApplicationModel();
        foreach (var type in LoadAdminControllerTypes())
        {
            var controllerModel = new ControllerModel(type.GetTypeInfo(), type.GetCustomAttributes(true));
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsAction(method))
                {
                    continue;
                }

                var actionModel = new ActionModel(method, method.GetCustomAttributes(true));
                foreach (var parameter in method.GetParameters())
                {
                    actionModel.Parameters.Add(new ParameterModel(parameter, parameter.GetCustomAttributes(true))
                    {
                        // MVC 框架由 ControllerActionApplicationModelProvider 回填；手工组装需补上，
                        // 否则 GET 单参数路径化（GetRouteParameterName 读 Parameters）判定失效
                        ParameterName = parameter.Name!,
                    });
                }

                actionModel.Controller = controllerModel;
                controllerModel.Actions.Add(actionModel);
            }

            if (controllerModel.Actions.Count > 0)
            {
                applicationModel.Controllers.Add(controllerModel);
            }
        }

        new AdminApiControllerConvention().Apply(applicationModel);

        var lines = new List<string>();
        foreach (var controller in applicationModel.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                foreach (var selector in action.Selectors)
                {
                    var verbs = string.Join(
                        ",",
                        selector.ActionConstraints.OfType<HttpMethodActionConstraint>()
                                .SelectMany(constraint => constraint.HttpMethods)
                                .Order(StringComparer.Ordinal));
                    lines.Add(controller.ControllerType.FullName + "|" + selector.AttributeRouteModel?.Template + "|" + verbs);
                }
            }
        }

        return lines.Order(StringComparer.Ordinal);
    }

    private static IEnumerable<Type> LoadAdminControllerTypes()
    {
        var types = new List<Type>();
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll"))
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            Type[] loadedTypes;
            try
            {
                loadedTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                loadedTypes = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
            }

            types.AddRange(loadedTypes.Where(IsAdminController));
        }

        return types.Distinct();
    }

    // 与 AdminApiControllerFeatureProvider.IsController 的 IAdminApiController 分支同一口径
    private static bool IsAdminController(Type type)
    {
        return typeof(IAdminApiController).IsAssignableFrom(type)
               && type.IsClass
               && type.IsPublic
               && !type.IsAbstract
               && !type.ContainsGenericParameters;
    }

    // 复刻 MVC ControllerActionDescriptorProvider 的 action 判定核心语义
    private static bool IsAction(MethodInfo method)
    {
        if (method.IsStatic || method.IsAbstract || method.IsGenericMethod || method.IsSpecialName)
        {
            return false;
        }

        if (method.DeclaringType == typeof(object))
        {
            return false;
        }

        if (method.Name == "Dispose" && method.GetParameters().Length == 0)
        {
            return false;
        }

        return method.GetCustomAttribute<NonActionAttribute>(true) == null;
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
