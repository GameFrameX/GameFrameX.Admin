using GameFrameX.Core.Authorization.Diagnostics;

namespace GameFrameX.Core.Tests.Authorization;

/// <summary>
/// C128 菜单权限/endpoint 绑定漂移诊断引擎行为契约测试。
/// <para>每个漂移类别一个最小用例 + 全一致绿路径。引擎为纯函数，直接构造快照调用，无 DB 依赖。</para>
/// </summary>
public sealed class MenuEndpointBindingDiagnosticsEngineTests
{
    // ---------- 绿路径 ----------

    [Fact]
    public void Diagnose_FullyConsistentSnapshot_IsClean()
    {
        var input = FullyConsistentInput();

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.True(report.IsClean);
        Assert.Empty(report.Findings);
        Assert.Empty(report.Summary);
    }

    [Fact]
    public void Diagnose_IsDeterministic_SameInputSameOutput()
    {
        var input = FullyConsistentInput();
        // 注入若干不同类别漂移以验证排序稳定性。
        input.EndpointRows.Add(Row(99, "POST", "api/missing/orphan", isOrphan: true, isEnabled: true));

        var r1 = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);
        var r2 = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Equal(r1.Findings.Count, r2.Findings.Count);
        for (var i = 0; i < r1.Findings.Count; i++)
        {
            Assert.Equal(r1.Findings[i].Category, r2.Findings[i].Category);
            Assert.Equal(r1.Findings[i].HttpMethod, r2.Findings[i].HttpMethod);
            Assert.Equal(r1.Findings[i].RoutePath, r2.Findings[i].RoutePath);
            Assert.Equal(r1.Findings[i].Detail, r2.Findings[i].Detail);
        }
    }

    // ---------- 1. LiveEndpointMissingFromDb ----------

    [Fact]
    public void Diagnose_LiveEndpointNotInDb_ReportsWarning()
    {
        var input = new BindingDiagnosticsRequest
        {
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                Live("POST", "api/systemUser/page"),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        var f = Assert.Single(report.Findings);
        Assert.Equal(BindingDriftCategory.LiveEndpointMissingFromDb, f.Category);
        Assert.Equal(BindingDriftSeverity.Warning, f.Severity);
        Assert.Equal("POST", f.HttpMethod);
        Assert.Equal("api/systemUser/page", f.RoutePath);
    }

    // ---------- 2. DbOrphanStillEnabled ----------

    [Fact]
    public void Diagnose_OrphanRowStillEnabled_ReportsError()
    {
        var input = new BindingDiagnosticsRequest
        {
            EndpointRows = new List<EndpointRowSnapshot>
            {
                Row(1, "POST", "api/systemUser/page", isOrphan: true, isEnabled: true),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.DbOrphanStillEnabled
                                              && f.Severity == BindingDriftSeverity.Error);
    }

    // ---------- 3. NonPublicEndpointWithoutBinding ----------

    [Fact]
    public void Diagnose_NonPublicEndpointNoBinding_ReportsError()
    {
        var input = new BindingDiagnosticsRequest
        {
            EndpointRows = new List<EndpointRowSnapshot>
            {
                Row(1, "POST", "api/systemUser/page", false, false, isEnabled: true),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.NonPublicEndpointWithoutBinding
                                              && f.Severity == BindingDriftSeverity.Error);
    }

    [Fact]
    public void Diagnose_PublicOrAuthOnlyEndpointNoBinding_NotFlagged()
    {
        var input = new BindingDiagnosticsRequest
        {
            EndpointRows = new List<EndpointRowSnapshot>
            {
                Row(1, "GET", "api/public/health", true, isEnabled: true),
                Row(2, "POST", "api/systemAuth/userInfo", isAuthenticatedOnly: true, isEnabled: true),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.DoesNotContain(report.Findings, f => f.Category == BindingDriftCategory.NonPublicEndpointWithoutBinding);
    }

    // ---------- 4. BindingToMissingMenu ----------

    [Fact]
    public void Diagnose_BindingToDeletedMenu_ReportsError()
    {
        var input = new BindingDiagnosticsRequest
        {
            EndpointRows = new List<EndpointRowSnapshot> { Row(1, "POST", "api/systemUser/page", isEnabled: true), },
            Bindings = new List<BindingSnapshot> { new() { MenuId = 500, EndpointId = 1, }, },
            Menus = new List<MenuSnapshot> { new() { Id = 500, IsDeleted = true, }, },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.BindingToMissingMenu
                                              && f.Severity == BindingDriftSeverity.Error);
    }

    // ---------- 5. BindingToMissingEndpoint ----------

    [Fact]
    public void Diagnose_BindingToDisabledEndpoint_ReportsError()
    {
        var input = new BindingDiagnosticsRequest
        {
            EndpointRows = new List<EndpointRowSnapshot> { Row(1, "POST", "api/systemUser/page", isEnabled: false), },
            Bindings = new List<BindingSnapshot> { new() { MenuId = 10, EndpointId = 1, }, },
            Menus = new List<MenuSnapshot> { new() { Id = 10, }, },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.BindingToMissingEndpoint
                                              && f.Severity == BindingDriftSeverity.Error);
    }

    // ---------- 6. MenuButtonPermissionOrphan ----------

    [Fact]
    public void Diagnose_ButtonPermissionMapsToNoLiveEndpoint_ReportsWarning()
    {
        var input = new BindingDiagnosticsRequest
        {
            Menus = new List<MenuSnapshot>
            {
                new() { Id = 10, Type = 3, Permission = "systemUser:ghost", },
            },
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                Live("POST", "api/systemUser/page"),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.MenuButtonPermissionOrphan
                                              && f.Severity == BindingDriftSeverity.Warning
                                              && f.RoutePath == "api/systemuser/ghost");
    }

    [Fact]
    public void Diagnose_ButtonPermissionMatchesParameterizedLiveRoute_NotFlagged()
    {
        var input = new BindingDiagnosticsRequest
        {
            Menus = new List<MenuSnapshot>
            {
                new() { Id = 10, Type = 3, Permission = "systemUser:detail", },
            },
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                Live("GET", "api/systemUser/detail/{id}"),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.DoesNotContain(report.Findings, f => f.Category == BindingDriftCategory.MenuButtonPermissionOrphan);
    }

    // ---------- 8. DuplicateEndpointKey ----------
    // （历史注记：段落 7 AuthenticatedOnlyRouteMissingFromLive 已随登录态白名单 [AuthenticatedOnly]
    // 特性化退役删除——特性长在方法上，方法删除则登记自动消失，该漂移结构性不可再发生，GFX-684/C277。）

    [Fact]
    public void Diagnose_DuplicateLiveEndpointKey_ReportsWarning()
    {
        var input = new BindingDiagnosticsRequest
        {
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                new() { HttpMethod = "POST", RoutePath = "api/systemUser/page", Controller = "A", Action = "Page1", },
                new() { HttpMethod = "POST", RoutePath = "api/systemUser/page", Controller = "B", Action = "Page2", },
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        Assert.Contains(report.Findings, f => f.Category == BindingDriftCategory.DuplicateEndpointKey
                                              && f.Severity == BindingDriftSeverity.Warning);
    }

    // ---------- SummaryAsync ----------

    [Fact]
    public void Diagnose_Summary_AggregatesByCategory()
    {
        var input = new BindingDiagnosticsRequest
        {
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                Live("POST", "api/x/one"),
                Live("POST", "api/x/two"),
            },
        };

        var report = MenuEndpointBindingDiagnosticsEngine.Diagnose(input);

        var missingCount = Assert.Single(report.Summary);
        Assert.Equal(BindingDriftCategory.LiveEndpointMissingFromDb, missingCount.Category);
        Assert.Equal(2, missingCount.Count);
    }

    // ---------- helpers ----------

    private static BindingDiagnosticsRequest FullyConsistentInput()
    {
        // 一个完全一致的快照：活端点 = DB 行；非公开端点有有效绑定；按钮 Permission 命中活端点。
        return new BindingDiagnosticsRequest
        {
            LiveEndpoints = new List<LiveEndpointSnapshot>
            {
                Live("POST", "api/systemUser/page"),
                Live("GET", "api/systemAuth/userInfo"),
            },
            EndpointRows = new List<EndpointRowSnapshot>
            {
                Row(1, "POST", "api/systemUser/page", isEnabled: true),
                Row(2, "GET", "api/systemAuth/userInfo", isAuthenticatedOnly: true, isEnabled: true),
            },
            Bindings = new List<BindingSnapshot>
            {
                new() { MenuId = 10, EndpointId = 1, IsAuto = false, },
            },
            Menus = new List<MenuSnapshot>
            {
                new() { Id = 10, Type = 3, Permission = "systemUser:page", IsDeleted = false, },
            },
        };
    }

    private static LiveEndpointSnapshot Live(string method, string path)
    {
        return new LiveEndpointSnapshot { HttpMethod = method, RoutePath = path, };
    }

    private static EndpointRowSnapshot Row(long id, string method, string path, bool isPublic = false, bool isAuthenticatedOnly = false, bool isOrphan = false, bool isEnabled = false)
    {
        return new EndpointRowSnapshot
        {
            Id = id,
            HttpMethod = method,
            RoutePath = path,
            IsPublic = isPublic,
            IsAuthenticatedOnly = isAuthenticatedOnly,
            IsOrphan = isOrphan,
            IsEnabled = isEnabled,
        };
    }
}