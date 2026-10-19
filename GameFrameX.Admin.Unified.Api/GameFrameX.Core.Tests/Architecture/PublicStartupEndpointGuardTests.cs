using System.Reflection;
using GameFrameX.Client.Api.RemoteConfig;
using GameFrameX.Core.Middleware;

namespace GameFrameX.Core.Tests.Architecture;

// C379：Tier 0 公开只读端点标记守护。规范正文：gfx-doc/dev/client-api/spec.md C377 分层口径节（Tier 0 规则）。
// 扫描三个 host 程序集的 [PublicStartupEndpoint] 集合，断言 == 预期三端点：
//   - 防未评审扩散：向任意未评审端点打标即红（标记 = 豁免传输签名强制，属安全敏感操作）；
//   - 防漏打：三端点任一丢标即红（丢标则 FailClosed 一开打断存量客户端启动流程）。
// 扩围纪律：先过分层判断口诀（响应无"这个人"的数据、请求无副作用），再打标并同步本测试预期集合。
public sealed class PublicStartupEndpointGuardTests
{
    private static readonly string[] ExpectedMarkedEndpoints =
    {
        "GameFrameX.Client.Api.RemoteConfig.GameRemoteConfigController.GetInfo",
        "GameFrameX.Client.Api.GameAppVersion.GameAppVersionController.GetInfo",
        "GameFrameX.Client.Api.GameAssetPackageVersion.GameAssetPackageVersionController.GetInfo",
    };

    [Fact]
    public void PublicStartupEndpointMarkers_ExactlyThreeStartupEndpoints()
    {
        // SignatureValidationMiddleware 仅注册于 Client.Api，但 Hub.Api / Web.Api 打标属误导性死代码，一并守护
        var assemblies = new[]
        {
            typeof(GameRemoteConfigController).Assembly,                      // GameFrameX.Client.Api
            typeof(GameFrameX.Hub.Api.Controllers.GameAssetPackageVersionController).Assembly, // GameFrameX.Hub.Api
            typeof(GameFrameX.Web.Api.ApiRouting.AdminApiControllerConvention).Assembly,       // GameFrameX.Web.Api
        };

        var marked = assemblies
                     .SelectMany(a => a.GetTypes())
                     .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                     .Select(m => (Method: m, Attr: m.GetCustomAttribute<PublicStartupEndpointAttribute>(inherit: false)))
                     .Where(x => x.Attr != null)
                     .Select(x => x.Method.DeclaringType!.FullName + "." + x.Method.Name)
                     .OrderBy(x => x, StringComparer.Ordinal)
                     .ToList();

        var expected = ExpectedMarkedEndpoints.OrderBy(x => x, StringComparer.Ordinal).ToList();
        Assert.True(marked.SequenceEqual(expected),
                    $"[PublicStartupEndpoint] 集合漂移。实际：[{string.Join(", ", marked)}]；预期：[{string.Join(", ", expected)}]");
    }
}
