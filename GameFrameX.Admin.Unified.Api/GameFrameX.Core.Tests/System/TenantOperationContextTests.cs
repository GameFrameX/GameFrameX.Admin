using System.Security.Claims;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.System;

public class TenantOperationContextTests
{
    [Fact]
    public void OrdinaryTenant_UsesJwtTenantIdAndIgnoresTargetHeader()
    {
        var user = CreateUser(AccountTypeEnum.SystemAdmin, "1001");

        var tenantId = TenantOperationContext.ResolveEffectiveTenantId(user, "2002");

        Assert.Equal(1001, tenantId);
    }

    [Fact]
    public void SuperAdmin_RequiresExplicitTargetTenant()
    {
        var user = CreateUser(AccountTypeEnum.SuperAdmin, "0");

        var tenantId = TenantOperationContext.ResolveEffectiveTenantId(user, null);

        Assert.Null(tenantId);
    }

    [Fact]
    public void SuperAdmin_UsesTargetTenantHeader()
    {
        var user = CreateUser(AccountTypeEnum.SuperAdmin, "0");

        var tenantId = TenantOperationContext.ResolveEffectiveTenantId(user, "2002");

        Assert.Equal(2002, tenantId);
    }

    [Fact]
    public void AnonymousUser_HasNoTenantContext()
    {
        var tenantId = TenantOperationContext.ResolveEffectiveTenantId(new ClaimsPrincipal(), "2002");

        Assert.Null(tenantId);
    }

    [Fact]
    public void AmbientTenantContext_OverridesAnonymousRequestAndRestoresAfterDispose()
    {
        using (TenantOperationContext.UseTenant(3003))
        {
            Assert.Equal(3003, TenantOperationContext.GetEffectiveTenantId());
        }

        Assert.Null(TenantOperationContext.ResolveEffectiveTenantId(new ClaimsPrincipal(), null));
    }

    [Fact]
    public void AmbientTenantContext_RejectsInvalidTenantId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TenantOperationContext.UseTenant(0));
    }

    [Fact]
    public void ConfiguredAccessorWithoutHttpContext_HasNoCurrentUserOrTenant()
    {
        TenantOperationContext.Configure(new HttpContextAccessor());

        Assert.Null(TenantOperationContext.CurrentHttpContext);
        Assert.Null(TenantOperationContext.CurrentUser);
        Assert.Null(TenantOperationContext.GetEffectiveTenantId());
    }

    private static ClaimsPrincipal CreateUser(AccountTypeEnum accountType, string tenantId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimConst.AccountType, ((int)accountType).ToString()),
            new Claim(ClaimConst.TenantId, tenantId),
        }, "test");

        return new ClaimsPrincipal(identity);
    }
}