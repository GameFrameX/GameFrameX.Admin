using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Tests.Tenant;

/// <summary>
/// 覆盖 <see cref="TenantAccessGate"/> 裁决矩阵（禁用/过期/过渡期/正常 × 读/写）。
/// <para>裁决逻辑经 <see cref="TenantAccessGate.Decide"/> 暴露为纯函数；到期状态经 <see cref="TenantExpirationService.GetExpirationStatus(SystemTenantEntity, GameFrameX.Entity.Global.GlobalPackageEntity)"/> 纯计算重载得出。两者组合 = 完整裁决，无需构造缓存与数据库。</para>
/// </summary>
public class TenantAccessGateTests
{
    /// <summary>
    /// TenantExpirationService.GetExpirationStatusAsync(tenant, package) 是纯计算，不访问注入的仓储，可传 null 构造。
    /// </summary>
    private static readonly TenantExpirationService ExpirationService = new(null!, null!);

    private static SystemTenantEntity Tenant(bool? enabled, long? expireTime, int? grace = null, long? packageId = null)
    {
        return new SystemTenantEntity
        {
            Id = 1,
            IsEnabled = enabled,
            ExpireTime = expireTime,
            GracePeriodDays = grace,
            PackageId = packageId,
        };
    }

    private static TenantExpirationStatus Status(SystemTenantEntity? t)
    {
        return ExpirationService.GetExpirationStatus(t, null);
    }

    [Fact]
    public void Decide_NullTenant_Allows()
    {
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(null, TenantExpirationStatus.Active, true));
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(null, TenantExpirationStatus.Expired, true));
    }

    [Fact]
    public void Decide_DisabledTenant_DeniesReadAndWrite()
    {
        var t = Tenant(false, null);
        Assert.Equal(TenantAccessVerdict.DenyDisabled, TenantAccessGate.Decide(t, TenantExpirationStatus.Active, false));
        Assert.Equal(TenantAccessVerdict.DenyDisabled, TenantAccessGate.Decide(t, TenantExpirationStatus.Active, true));
        Assert.Equal(TenantAccessVerdict.DenyDisabled, TenantAccessGate.Decide(t, TenantExpirationStatus.Expired, true));
    }

    [Fact]
    public void Decide_EnabledNull_TreatedAsEnabled()
    {
        var t = Tenant(null, null);
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, TenantExpirationStatus.Active, true));
    }

    [Fact]
    public void Decide_ExpiredWrite_DeniesExpiredWrite()
    {
        var t = Tenant(true, DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(), 0);
        var status = Status(t);
        Assert.Equal(TenantExpirationStatus.Expired, status);
        Assert.Equal(TenantAccessVerdict.DenyExpiredWrite, TenantAccessGate.Decide(t, status, true));
    }

    [Fact]
    public void Decide_ExpiredRead_Allows()
    {
        var t = Tenant(true, DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(), 0);
        var status = Status(t);
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, false));
    }

    [Fact]
    public void Decide_GracePeriod_AllowsReadWrite()
    {
        var t = Tenant(true, DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(), 5);
        var status = Status(t);
        Assert.Equal(TenantExpirationStatus.InGracePeriod, status);
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, true));
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, false));
    }

    [Fact]
    public void Decide_Active_AllowsReadWrite()
    {
        var t = Tenant(true, DateTimeOffset.UtcNow.AddDays(5).ToUnixTimeSeconds());
        var status = Status(t);
        Assert.Equal(TenantExpirationStatus.Active, status);
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, true));
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, false));
    }

    [Fact]
    public void Decide_NoExpireTime_TreatedAsActive()
    {
        var t = Tenant(true, null);
        var status = Status(t);
        Assert.Equal(TenantExpirationStatus.Active, status);
        Assert.Equal(TenantAccessVerdict.Allow, TenantAccessGate.Decide(t, status, true));
    }
}