using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Const;
using GameFrameX.Core.Middleware;
using GameFrameX.Hub.Api.Middleware;

namespace GameFrameX.Core.Tests.Architecture;

/// <summary>
/// 租户上下文统一契约守护（C260，GFX-667）：全仓唯一 <see cref="TenantResolutionMiddleware"/> 实现且归属 GameFrameX.Core，
/// 各 host 无同名副本回潮（C268 后剩 Client/Hub 两 host，Game.Api 已并入 Client.Api）；Hub 不再持有专有 Items 键常量；统一键值与 <see cref="ClaimConst.TenantId"/> 同串（Client 公开回调双写语义保持）。
/// </summary>
public class TenantResolutionUnificationTests
{
    private static readonly global::System.Reflection.Assembly[] HostAssemblies =
    {
        typeof(TenantResolutionMiddleware).Assembly,
        typeof(AppResolutionMiddleware).Assembly,
        typeof(HubTenantMiddleware).Assembly,
    };

    [Fact]
    public void TenantResolutionMiddleware_SingleImplementation_LivesInCore()
    {
        var matches = HostAssemblies
                      .SelectMany(a => a.GetTypes())
                      .Where(t => t.Name == nameof(TenantResolutionMiddleware))
                      .ToList();

        var match = Assert.Single(matches);
        Assert.Equal("GameFrameX.Core", match.Assembly.GetName().Name);
    }

    [Fact]
    public void HubTenantMiddleware_NoPrivateItemsKeyConstant()
    {
        // 收敛前 Hub 专有键 __hub_tenant_id 以 const TenantIdKey 形式定义在 HubTenantMiddleware 上，C260 起只允许统一键
        var constFields = typeof(HubTenantMiddleware)
                          .GetFields(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static)
                          .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                          .ToList();

        Assert.DoesNotContain(constFields, f => f.Name == "TenantIdKey");
        Assert.DoesNotContain(constFields, f => (string?)f.GetRawConstantValue() == "__hub_tenant_id");
    }

    [Fact]
    public void TenantContextAccessor_UnifiedKeyMatchesClaimConstTenantId()
    {
        Assert.Equal("TenantId", TenantContextAccessor.TenantIdKey);
        Assert.Equal(ClaimConst.TenantId, TenantContextAccessor.TenantIdKey);
    }
}