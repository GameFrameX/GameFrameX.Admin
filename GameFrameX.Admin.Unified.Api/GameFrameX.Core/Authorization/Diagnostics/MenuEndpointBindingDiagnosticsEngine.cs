namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 菜单权限/endpoint 绑定漂移诊断纯函数引擎（C128）。
/// <para>输入只读快照，输出按稳定 <see cref="BindingDriftCategory"/> 分类的结构化报告。
/// 不依赖 SqlSugar / HttpContext / 任何有状态服务，可被单元测试直接调用。
/// 引擎 <b>不写 DB、不触发</b> ApiEndpointCatalogSync。</para>
/// <para>确定性保证：相同输入 → 相同输出（含排序：Category → HttpMethod → RoutePath → DetailAsync，Ordinal 比较）。</para>
/// </summary>
public static class MenuEndpointBindingDiagnosticsEngine
{
    /// <summary>
    /// 菜单类型：按钮（与 <c>MenuTypeEnum.Button</c> 的整数值一致）。
    /// </summary>
    public const int MenuTypeButton = 3;

    /// <summary>
    /// 执行诊断。
    /// </summary>
    public static BindingDiagnosticsReport Diagnose(BindingDiagnosticsRequest input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var live = input.LiveEndpoints ?? new List<LiveEndpointSnapshot>();
        var rows = input.EndpointRows ?? new List<EndpointRowSnapshot>();
        var bindings = input.Bindings ?? new List<BindingSnapshot>();
        var menus = input.Menus ?? new List<MenuSnapshot>();

        var findings = new List<BindingDriftFinding>();

        // 活端点 routePath 集合（用于按钮 Permission 孤立判定与白名单缺失判定）。
        var liveRoutePaths = new HashSet<string>(live.Select(e => e.RoutePath), StringComparer.OrdinalIgnoreCase);

        // DB 端点键索引：(HttpMethod, RoutePath) → row（对账键，与 ApiEndpointCatalogSync 一致）。
        var rowByKey = new Dictionary<string, EndpointRowSnapshot>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var key = ApiEndpointIdentity.ToEndpointKey(row.HttpMethod, row.RoutePath);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            // 同键多行（理论不应发生，唯一索引保护）：保留首个，避免误判 LiveEndpointMissingFromDb。
            if (!rowByKey.ContainsKey(key))
            {
                rowByKey[key] = row;
            }
        }

        var rowById = rows.Where(r => r.Id > 0).ToDictionary(r => r.Id);

        // 活跃菜单集合：存在且未软删。绑定有效性以此为准。
        var activeMenuIds = new HashSet<long>(menus.Where(m => !m.IsDeleted).Select(m => m.Id));

        // 1) LiveEndpointMissingFromDb + DuplicateEndpointKey（先按 key 分组活端点以同时覆盖重复键）。
        var liveByKey = new Dictionary<string, List<LiveEndpointSnapshot>>(StringComparer.OrdinalIgnoreCase);
        foreach (var ep in live)
        {
            var key = ApiEndpointIdentity.ToEndpointKey(ep.HttpMethod, ep.RoutePath);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!liveByKey.TryGetValue(key, out var bucket))
            {
                bucket = new List<LiveEndpointSnapshot>();
                liveByKey[key] = bucket;
            }

            bucket.Add(ep);
        }

        foreach (var pair in liveByKey)
        {
            var key = pair.Key;
            var bucket = pair.Value;
            if (bucket.Count > 1)
            {
                findings.Add(Build(BindingDriftCategory.DuplicateEndpointKey, bucket[0].HttpMethod, bucket[0].RoutePath,
                                   $"端点键 {key} 被 {bucket.Count} 个活方法占用，采集仅保留首个（控制器/方法：{JoinControllers(bucket)}）。"));
            }

            if (!rowByKey.ContainsKey(key))
            {
                var first = bucket[0];
                findings.Add(Build(BindingDriftCategory.LiveEndpointMissingFromDb, first.HttpMethod, first.RoutePath,
                                   $"活端点 {key}（{first.Controller}.{first.Action}）未在 global_api_endpoint 登记，JwtHandler 将 fail-closed。"));
            }
        }

        // 2) DbOrphanStillEnabled + 3) NonPublicEndpointWithoutBinding（遍历 DB 行）。
        var endpointHasActiveBinding = new HashSet<long>();
        foreach (var b in bindings)
        {
            if (activeMenuIds.Contains(b.MenuId))
            {
                endpointHasActiveBinding.Add(b.EndpointId);
            }
        }

        foreach (var row in rows)
        {
            if (row.IsOrphan && row.IsEnabled)
            {
                findings.Add(Build(BindingDriftCategory.DbOrphanStillEnabled, row.HttpMethod, row.RoutePath,
                                   $"DB 端点 Id={row.Id} 标记孤儿但 IsEnabled=true（应在对账后自动禁用）。"));
            }

            if (row.IsEnabled && !row.IsPublic && !row.IsAuthenticatedOnly && !endpointHasActiveBinding.Contains(row.Id))
            {
                findings.Add(Build(BindingDriftCategory.NonPublicEndpointWithoutBinding, row.HttpMethod, row.RoutePath,
                                   $"端点 Id={row.Id} 非公开/非白名单且无有效绑定，JwtHandler 空绑定将 fail-closed（任何非超管请求必 403）。"));
            }
        }

        // 4) BindingToMissingMenu + 5) BindingToMissingEndpoint。
        foreach (var b in bindings)
        {
            if (!activeMenuIds.Contains(b.MenuId))
            {
                findings.Add(Build(BindingDriftCategory.BindingToMissingMenu, string.Empty, string.Empty,
                                   $"绑定 (MenuId={b.MenuId}, EndpointId={b.EndpointId}) 指向不存在或已软删菜单。"));
            }

            if (!rowById.TryGetValue(b.EndpointId, out var boundRow) || !boundRow.IsEnabled)
            {
                findings.Add(Build(BindingDriftCategory.BindingToMissingEndpoint, string.Empty, string.Empty,
                                   $"绑定 (MenuId={b.MenuId}, EndpointId={b.EndpointId}) 指向不存在或已禁用端点。"));
            }
        }

        // 6) MenuButtonPermissionOrphan。
        foreach (var menu in menus)
        {
            if (menu.IsDeleted || menu.Type != MenuTypeButton || string.IsNullOrWhiteSpace(menu.Permission))
            {
                continue;
            }

            var routePath = ApiEndpointIdentity.RouteNameToRoutePath(menu.Permission);
            if (string.IsNullOrEmpty(routePath))
            {
                continue;
            }

            if (!LiveRouteMatches(liveRoutePaths, routePath))
            {
                findings.Add(Build(BindingDriftCategory.MenuButtonPermissionOrphan, string.Empty, routePath,
                                   $"按钮 MenuId={menu.Id} Permission='{menu.Permission}' → routePath='{routePath}' 命中零活端点（孤立按钮）。"));
            }
        }

        // 7) 登录态白名单漂移类别已随 [AuthenticatedOnly] 特性化退役（GFX-684/C277）：
        // 特性长在方法上，方法删除/改名则登记自动消失，该漂移结构性不可再发生。

        // 确定性排序：Category → HttpMethod → RoutePath → DetailAsync。
        findings.Sort(CompareFindings);

        var summary = findings.GroupBy(f => f.Category)
                              .Select(g => new BindingDriftCategoryCount
                              {
                                  Category = g.Key,
                                  Severity = BindingDriftSeverityMap.Of(g.Key),
                                  Count = g.Count(),
                              })
                              .OrderBy(c => (int)c.Category)
                              .ToList();

        return new BindingDiagnosticsReport
        {
            GeneratedTime = TimerHelper.UnixTimeSeconds(),
            Findings = findings,
            Summary = summary,
        };
    }

    private static bool LiveRouteMatches(HashSet<string> liveRoutePaths, string routePath)
    {
        if (liveRoutePaths.Contains(routePath))
        {
            return true;
        }

        // 参数化路由回退（与 ApiEndpointCatalogSync.MigrateBindingsFromMenuPermissionAsync 一致）：
        // 按钮 routePath 命中以自身为前缀的参数化端点（如 api/systemUser/{id}）也算匹配。
        var parameterizedPrefix = routePath + "/{";
        foreach (var live in liveRoutePaths)
        {
            if (live.StartsWith(parameterizedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static BindingDriftFinding Build(BindingDriftCategory category, string httpMethod, string routePath, string detail)
    {
        return new BindingDriftFinding
        {
            Category = category,
            Severity = BindingDriftSeverityMap.Of(category),
            HttpMethod = httpMethod ?? string.Empty,
            RoutePath = routePath ?? string.Empty,
            Detail = detail ?? string.Empty,
        };
    }

    private static string JoinControllers(List<LiveEndpointSnapshot> bucket)
    {
        var parts = bucket.Select(e => (e.Controller ?? "?") + "." + (e.Action ?? "?"));
        return string.Join("; ", parts);
    }

    private static int CompareFindings(BindingDriftFinding x, BindingDriftFinding y)
    {
        var c = ((int)x.Category).CompareTo((int)y.Category);
        if (c != 0)
        {
            return c;
        }

        c = string.CompareOrdinal(x.HttpMethod, y.HttpMethod);
        if (c != 0)
        {
            return c;
        }

        c = string.CompareOrdinal(x.RoutePath, y.RoutePath);
        if (c != 0)
        {
            return c;
        }

        return string.CompareOrdinal(x.Detail, y.Detail);
    }
}