using System.Reflection;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GameFrameX.Foundation.Utility;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// API 端点目录启动同步（U6/C42）。
/// <para>Host 启动后：枚举 ASP.NET Core MVC 发现的全部 Admin API 端点 → 与 <see cref="GlobalApiEndpointEntity"/> 对账（匹配/孤儿/新增）
/// → 按 DB <c>global_menu.Permission</c>（routeName）一次性回填 <see cref="GlobalMenuEndpointEntity"/> 绑定
/// → 为未绑定且非公开端点自动挂隐藏按钮 → 构建并预热请求期端点权限缓存。</para>
/// <para>异常上抛让 Host fail-fast：避免目录空导致 JwtHandler 静默锁死全部租户请求。</para>
/// </summary>
public sealed class ApiEndpointCatalogSync : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApiEndpointCatalogSync> _logger;

    public ApiEndpointCatalogSync(IServiceProvider serviceProvider, ILogger<ApiEndpointCatalogSync> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var endpointRepository = provider.GetRequiredService<SqlSugarRepository<GlobalApiEndpointEntity>>();
        var bindingRepository = provider.GetRequiredService<SqlSugarRepository<GlobalMenuEndpointEntity>>();
        var menuRepository = provider.GetRequiredService<SqlSugarRepository<GlobalMenuEntity>>();
        var actionDescriptors = provider.GetRequiredService<IActionDescriptorCollectionProvider>();
        var cache = provider.GetRequiredService<SystemCacheService>();

        // 仅 Admin 主机（Web.Entry）运行：Client.Api/Hub.Api 没有 SystemCacheService 等依赖。
        // 通过是否存在 SystemCacheService 判定（上面 GetRequiredService 已会在缺失时抛异常，这里只做早退日志）。
        Dictionary<string, LiveEndpoint> liveEndpoints = new(StringComparer.OrdinalIgnoreCase);
        var now = TimerHelper.UnixTimeSeconds();

        var tran = await endpointRepository.Context.Ado.UseTranAsync(async () =>
        {
            liveEndpoints = CollectLiveEndpoints(actionDescriptors);
            await ReconcileEndpointsAsync(endpointRepository, liveEndpoints, now);
            await MigrateBindingsFromMenuPermissionAsync(menuRepository, endpointRepository, bindingRepository);
            await AutoBindUnboundEndpointsAsync(endpointRepository, bindingRepository, menuRepository, now);
        });
        if (!tran.IsSuccess)
        {
            throw new InvalidOperationException("ApiEndpointCatalogSync 事务同步失败。", tran.ErrorException);
        }

        MenuSeedCatalog.LoadFromDb(menuRepository.Context);
        InvalidateAuthorizationCaches(cache);
        await WarmEndpointPermissionCacheAsync(endpointRepository, bindingRepository, cache);

        _logger.LogInformation("ApiEndpointCatalogSync 完成：活端点 {Live}，登录态白名单 {AuthOnly}。", liveEndpoints.Count, liveEndpoints.Values.Count(e => e.IsAuthenticatedOnly));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static void InvalidateAuthorizationCaches(SystemCacheService cache)
    {
        cache.Remove(CacheConst.KeyEndpointByRoute);
        cache.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        cache.RemoveByPrefixKey(CacheConst.KeyUserButton);
    }

    /// <summary>
    /// 枚举 MVC 发现的活端点，归一化为 (HttpMethod, RoutePath) → 元数据。
    /// </summary>
    private Dictionary<string, LiveEndpoint> CollectLiveEndpoints(IActionDescriptorCollectionProvider provider)
    {
        var map = new Dictionary<string, LiveEndpoint>(StringComparer.OrdinalIgnoreCase);
        foreach (var ad in provider.ActionDescriptors.Items)
        {
            if (ad is not ControllerActionDescriptor cad)
            {
                continue;
            }

            var template = cad.AttributeRouteInfo?.Template;
            if (string.IsNullOrWhiteSpace(template))
            {
                // 约定路由（HomeController default）不作为权限点。
                continue;
            }

            var routePath = ApiEndpointIdentity.NormalizeRoutePath(template);
            if (string.IsNullOrEmpty(routePath) || !routePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (cad.MethodInfo.GetCustomAttribute<NonActionAttribute>() != null)
            {
                continue;
            }

            var isPublic = cad.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()
                           || cad.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null;
            // 「仅需登录」档与匿名档同源判定：直接扫 [AuthenticatedOnly] 特性（声明即登记，GFX-684），
            // 不再经由手写路由清单 → 中转表 → HashSet 三跳，routePath 由 ActionDescriptor 现算，登记侧与路由侧不可能漂移。
            var isAuthenticatedOnly = cad.EndpointMetadata.OfType<AuthenticatedOnlyAttribute>().Any()
                                      || cad.MethodInfo.GetCustomAttribute<AuthenticatedOnlyAttribute>() != null;
            var httpMethods = AdminApiEndpointMetadata.GetHttpMethods(cad.EndpointMetadata);

            var display = TryGetApiDescriptionSettingsName(cad.MethodInfo) ?? cad.ActionName;

            foreach (var httpMethod in httpMethods)
            {
                var method = ApiEndpointIdentity.NormalizeHttpMethod(httpMethod);
                var endpointKey = ApiEndpointIdentity.ToEndpointKey(method, routePath);
                if (string.IsNullOrEmpty(endpointKey))
                {
                    continue;
                }

                if (map.TryGetValue(endpointKey, out var existing))
                {
                    _logger.LogWarning("端点重复被多个方法占用，保留首个：{EndpointKey}（已有 {Prev}，跳过 {Skip}）",
                                       endpointKey, existing.Controller + "." + existing.Action, cad.ControllerName + "." + cad.ActionName);
                    continue;
                }

                map[endpointKey] = new LiveEndpoint
                {
                    HttpMethod = method,
                    RoutePath = routePath,
                    DisplayName = display,
                    Controller = cad.ControllerName,
                    Action = cad.ActionName,
                    IsPublic = isPublic,
                    IsAuthenticatedOnly = isAuthenticatedOnly,
                };
            }
        }

        return map;
    }

    /// <summary>
    /// 与 DB 对账：匹配更新描述、孤儿禁用、新增登记。Id 不变 → 绑定与授权零漂移。
    /// </summary>
    private async Task ReconcileEndpointsAsync(SqlSugarRepository<GlobalApiEndpointEntity> repository, Dictionary<string, LiveEndpoint> liveEndpoints, long now)
    {
        // 临时清除查询过滤器（ISafeDeletedFilter/ISafeEnabledFilter），让对账能看到被软删除/禁用的物理行。
        // 否则活端点匹配不到被遮挡的同键行 → 走插入分支 → 撞全表唯一约束 ux_global_api_endpoint_method_route。
        // 与本文件 menuRepository/bindingRepository 的 ClearAndBackup 写法、SqlSugarSetup seed upsert 的 QueryFilter.ClearAsync() 同因。
        repository.Context.QueryFilter.ClearAndBackup();
        try
        {
            var rows = await repository.AsQueryable().ToListAsync();
            var byEndpoint = rows.ToDictionary(r => ApiEndpointIdentity.ToEndpointKey(r.HttpMethod, r.RoutePath), r => r, StringComparer.OrdinalIgnoreCase);

            foreach (var live in liveEndpoints.Values)
            {
                var endpointKey = ApiEndpointIdentity.ToEndpointKey(live.HttpMethod, live.RoutePath);
                if (byEndpoint.TryGetValue(endpointKey, out var row))
                {
                    var dirty = false;
                    if (row.HttpMethod != live.HttpMethod)
                    {
                        row.HttpMethod = live.HttpMethod;
                        dirty = true;
                    }

                    if (row.DisplayName != live.DisplayName)
                    {
                        row.DisplayName = live.DisplayName;
                        dirty = true;
                    }

                    if (row.Controller != live.Controller)
                    {
                        row.Controller = live.Controller;
                        dirty = true;
                    }

                    if (row.Action != live.Action)
                    {
                        row.Action = live.Action;
                        dirty = true;
                    }

                    if (row.IsPublic != live.IsPublic)
                    {
                        row.IsPublic = live.IsPublic;
                        dirty = true;
                    }

                    if (row.IsAuthenticatedOnly != live.IsAuthenticatedOnly)
                    {
                        row.IsAuthenticatedOnly = live.IsAuthenticatedOnly;
                        dirty = true;
                    }

                    if (row.IsOrphan)
                    {
                        row.IsOrphan = false;
                        dirty = true;
                    }

                    if (row.IsEnabled != true)
                    {
                        row.IsEnabled = true;
                        dirty = true;
                    }

                    // 复活被软删除的行：否则运行期缓存查询（带过滤器）仍看不到该端点，授权漏口。
                    if (row.IsDeleted == true)
                    {
                        row.IsDeleted = false;
                        dirty = true;
                    }

                    row.LastSeenTime = now;
                    if (dirty)
                    {
                        await repository.AsUpdateable(row).ExecuteCommandAsync();
                    }
                }
                else
                {
                    var inserted = new GlobalApiEndpointEntity
                    {
                        HttpMethod = live.HttpMethod,
                        RoutePath = live.RoutePath,
                        DisplayName = live.DisplayName,
                        Controller = live.Controller,
                        Action = live.Action,
                        IsPublic = live.IsPublic,
                        IsAuthenticatedOnly = live.IsAuthenticatedOnly,
                        IsOrphan = false,
                        IsEnabled = true,
                        FirstSeenTime = now,
                        LastSeenTime = now,
                    };
                    var storageable = repository.Context.Storageable(new List<GlobalApiEndpointEntity> { inserted, });
                    storageable.WhereColumns(new[] { nameof(GlobalApiEndpointEntity.HttpMethod), nameof(GlobalApiEndpointEntity.RoutePath), });
                    var storage = storageable.ToStorage();
                    await storage.AsInsertable.ExecuteCommandAsync();
                    await storage.AsUpdateable.ExecuteCommandAsync();
                    _logger.LogInformation("新增端点权限点：{Route}（{Ctrl}.{Act}）—— 默认不授权任何租户角色。", live.RoutePath, live.Controller, live.Action);
                }
            }

            // 孤儿：DB 有、活端点无。软删除行已“死”，跳过以免清过滤器后多余 update。
            var orphanCount = 0;
            foreach (var row in rows)
            {
                if (row.IsDeleted == true)
                {
                    continue;
                }

                if (liveEndpoints.ContainsKey(ApiEndpointIdentity.ToEndpointKey(row.HttpMethod, row.RoutePath)))
                {
                    continue;
                }

                if (!row.IsOrphan || row.IsEnabled != true)
                {
                    row.IsOrphan = true;
                    row.IsEnabled = false;
                    row.LastSeenTime = now;
                    await repository.AsUpdateable(row).ExecuteCommandAsync();
                }

                orphanCount++;
            }

            if (orphanCount > 0)
            {
                _logger.LogWarning("端点孤儿 {Count} 个（DB 有行但本次扫描无活端点，已禁用并保留授权）。", orphanCount);
            }
        }
        finally
        {
            repository.Context.QueryFilter.Restore();
        }
    }

    /// <summary>
    /// 一次性迁移：读 DB global_menu 按钮 Permission（routeName）→ RoutePath → 匹配 api_endpoint → 回填 global_menu_endpoint。
    /// 幂等。**不**读 .g.cs 文件，保证 C43 删源生成器后仍可迁移。**不**动 Id/Key/tenant_role_menu。
    /// </summary>
    private async Task MigrateBindingsFromMenuPermissionAsync(
        SqlSugarRepository<GlobalMenuEntity> menuRepository,
        SqlSugarRepository<GlobalApiEndpointEntity> endpointRepository,
        SqlSugarRepository<GlobalMenuEndpointEntity> bindingRepository)
    {
        var buttons = await menuRepository.AsQueryable()
                                   .Where(u => u.Type == MenuTypeEnum.Button && u.Permission != null && u.Permission != "")
                                   .Select(u => new { u.Id, u.Permission, })
                                   .ToListAsync();

        if (buttons.Count == 0)
        {
            return;
        }

        var endpoints = await endpointRepository.AsQueryable().Where(u => u.IsEnabled == true).ToListAsync();
        var endpointsByPath = endpoints.ToLookup(e => e.RoutePath, StringComparer.OrdinalIgnoreCase);

        var existingBindings = await bindingRepository.AsQueryable()
                                               .Where(u => u.IsAuto == false)
                                               .Select(u => new { u.MenuId, u.EndpointId, })
                                               .ToListAsync();
        var existing = existingBindings.Select(b => (b.MenuId, b.EndpointId)).ToHashSet();

        var toInsert = new List<GlobalMenuEndpointEntity>();
        var orphanButtons = 0;
        foreach (var btn in buttons)
        {
            var routePath = ApiEndpointIdentity.RouteNameToRoutePath(btn.Permission);
            var matchedEndpoints = string.IsNullOrEmpty(routePath)
                                       ? new List<GlobalApiEndpointEntity>()
                                       : endpointsByPath[routePath].ToList();
            if (matchedEndpoints.Count == 0 && !string.IsNullOrEmpty(routePath))
            {
                var parameterizedPrefix = routePath + "/{";
                matchedEndpoints = endpoints
                                   .Where(u => u.RoutePath.StartsWith(parameterizedPrefix, StringComparison.OrdinalIgnoreCase))
                                   .ToList();
            }

            if (matchedEndpoints.Count == 0 && !string.IsNullOrEmpty(routePath))
            {
                // controller 改名兜底（GFX-709 等「加前缀」式改名后按钮 permission 前缀仍是旧名）：
                // 直译路由未命中时，尝试「controller 以旧名为后缀 + action 相同」的端点，
                // 如 gameAppOpenAccess:page → api/tenantGameAppOpenAccess/page。
                // 非后缀式改名（如 Dict→Dictionary）不在此列，保持孤立按钮不动。
                matchedEndpoints = MatchRenamedControllerEndpoints(routePath, endpoints);
            }

            if (matchedEndpoints.Count == 0)
            {
                // Permission 指向已不存在的路由（孤立按钮），不阻断。
                orphanButtons++;
                continue;
            }

            foreach (var endpoint in matchedEndpoints)
            {
                var key = (btn.Id, endpoint.Id);
                if (existing.Contains(key))
                {
                    continue;
                }

                toInsert.Add(new GlobalMenuEndpointEntity { MenuId = btn.Id, EndpointId = endpoint.Id, IsAuto = false, });
                existing.Add(key);
            }
        }

        if (toInsert.Count > 0)
        {
            await bindingRepository.AsInsertable(toInsert).ExecuteCommandAsync();
        }

        _logger.LogInformation("绑定迁移：按钮 {Button}，回填 {Filled}，孤立按钮 {Orphan}。", buttons.Count, toInsert.Count, orphanButtons);
    }

    /// <summary>
    /// 「加前缀改名」controller 的端点兜底匹配：期望 routePath 的 action 相同、且现 controller 以旧 controller 名为后缀（忽略大小写）。
    /// </summary>
    private static List<GlobalApiEndpointEntity> MatchRenamedControllerEndpoints(string routePath, List<GlobalApiEndpointEntity> endpoints)
    {
        var parts = routePath.Split('/');
        if (parts.Length < 3)
        {
            return new List<GlobalApiEndpointEntity>();
        }

        var legacyController = parts[1];
        var action = parts[2];
        return endpoints
               .Where(u =>
               {
                   var segments = u.RoutePath.Split('/');
                   return segments.Length >= 3
                          && string.Equals(segments[2], action, StringComparison.OrdinalIgnoreCase)
                          && segments[1].EndsWith(legacyController, StringComparison.OrdinalIgnoreCase);
               })
               .ToList();
    }

    /// <summary>
    /// 未绑定且非公开、非白名单的端点 → 自动挂隐藏按钮 + IsAuto 绑定。满足"每个接口至少一个按钮"。
    /// </summary>
    private async Task AutoBindUnboundEndpointsAsync(
        SqlSugarRepository<GlobalApiEndpointEntity> endpointRepository,
        SqlSugarRepository<GlobalMenuEndpointEntity> bindingRepository,
        SqlSugarRepository<GlobalMenuEntity> menuRepository,
        long now)
    {
        var endpoints = await endpointRepository.AsQueryable()
                                         .Where(u => u.IsEnabled == true && u.IsPublic == false && u.IsAuthenticatedOnly == false)
                                         .Select(u => new { u.Id, u.RoutePath, })
                                         .ToListAsync();
        var activeMenuIds = (await menuRepository.AsQueryable().Select(u => u.Id).ToListAsync()).ToHashSet();
        var explicitlyBoundEndpointIds = (await bindingRepository.AsQueryable()
                                                          .Where(u => u.IsAuto == false && activeMenuIds.Contains(u.MenuId))
                                                          .Select(u => u.EndpointId)
                                                          .ToListAsync()).ToHashSet();

        // 自动按钮挂载推导：同 controller 的显式按钮（permission 形如 "controller:action"）挂在哪个页面菜单，
        // 自动按钮就跟随挂哪个；无显式按钮的 controller 保持 pid=0（端点对非超管 fail-closed）。
        // ponytail: 同 controller 多页面时取首个命中页面，按「controller 独占页面」假设；出现复用需改精确路由映射。
        var pageBoundButtons = await menuRepository.AsQueryable()
                                             .Where(u => u.Type == MenuTypeEnum.Button && u.Pid > 0 && u.Permission != null)
                                             .Select(u => new { u.Permission, u.Pid })
                                             .ToListAsync();
        var controllerPidMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var button in pageBoundButtons)
        {
            controllerPidMap.TryAdd(button.Permission!.Split(':')[0], button.Pid);
        }
        if (explicitlyBoundEndpointIds.Count > 0)
        {
            await bindingRepository.AsUpdateable()
                            .SetColumns(u => new GlobalMenuEndpointEntity { IsDeleted = true, })
                            .Where(u => u.IsAuto == true && explicitlyBoundEndpointIds.Contains(u.EndpointId))
                            .ExecuteCommandAsync();
        }

        var boundEndpointIds = (await bindingRepository.AsQueryable()
                                                .Where(u => activeMenuIds.Contains(u.MenuId))
                                                .Select(u => u.EndpointId)
                                                .ToListAsync()).ToHashSet();
        var unbound = endpoints.Where(e => !boundEndpointIds.Contains(e.Id)).ToList();

        // 存量自动按钮挂载矫正（历史版本一律 pid=0 挂根，非超管调用对应端点一律 403）。
        // 这些按钮的端点已有 IsAuto 绑定、不属于 unbound，上面的绑定计算触不到，须独立修正。
        // 仅按 Key 前缀 + pid=0 识别挂根的自动按钮；软删按钮交给下方恢复分支，恢复时随 CreateAutoMenu 带上新 pid。
        var rootAutoMenus = await menuRepository.AsQueryable()
                                         .Where(u => u.Type == MenuTypeEnum.Button && u.IsHide == true && u.Pid == 0)
                                         .Where(u => u.Key != null && u.Key.StartsWith("permission:auto:"))
                                         .Select(u => new { u.Id, u.Key, })
                                         .ToListAsync();
        var pidFixes = new List<GlobalMenuEntity>();
        foreach (var auto in rootAutoMenus)
        {
            var controller = auto.Key!.Split(':')[2];
            if (controllerPidMap.TryGetValue(controller, out var pid))
            {
                pidFixes.Add(new GlobalMenuEntity { Id = auto.Id, Pid = pid, });
            }
        }

        if (pidFixes.Count > 0)
        {
            await menuRepository.AsUpdateable(pidFixes)
                          .UpdateColumns(u => new { u.Pid, })
                          .ExecuteCommandAsync();
            _logger.LogInformation("自动按钮挂载矫正：{Count} 个 pid=0 按钮已挂到所属页面菜单。", pidFixes.Count);
        }

        if (unbound.Count == 0)
        {
            return;
        }

        // 清理之前自动挂的按钮（如端点已显式绑定，移除其 IsAuto 行与隐藏按钮）。
        var autoKeys = unbound.Select(e => AutoButtonKey(e.RoutePath)).ToList();
        var autoKeyIds = autoKeys.Select(StableSeedId.Menu).ToHashSet();
        List<AutoMenuSnapshot> existingAutoMenus;
        menuRepository.Context.QueryFilter.ClearAndBackup();
        try
        {
            var rows = await menuRepository.AsQueryable()
                                    .Where(u => autoKeyIds.Contains(u.Id))
                                    .Select(u => new { u.Id, u.Key, u.Pid, IsDeleted = u.IsDeleted == true, })
                                    .ToListAsync();
            existingAutoMenus = rows.Select(u => new AutoMenuSnapshot(u.Id, u.Key, u.Pid, u.IsDeleted)).ToList();
        }
        finally
        {
            menuRepository.Context.QueryFilter.Restore();
        }

        var existingAutoById = existingAutoMenus.ToDictionary(m => m.Id);

        var menuToInsert = new List<GlobalMenuEntity>();
        var menuToRestore = new List<GlobalMenuEntity>();
        var bindingToInsert = new List<GlobalMenuEndpointEntity>();
        foreach (var ep in unbound)
        {
            var key = AutoButtonKey(ep.RoutePath);
            var menuId = StableSeedId.Menu(key);
            var autoMenu = CreateAutoMenu(menuId, key, ep.RoutePath, ResolveAutoMenuPid(ep.RoutePath, controllerPidMap));
            if (existingAutoById.TryGetValue(menuId, out var existingMenu))
            {
                if (!string.Equals(existingMenu.Key, key, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"自动权限按钮稳定 Id 冲突：Id={menuId}，existingKey={existingMenu.Key}，newKey={key}。");
                }

                if (existingMenu.IsDeleted)
                {
                    menuToRestore.Add(autoMenu);
                    existingAutoById[menuId] = existingMenu with { IsDeleted = false, };
                }
            }
            else
            {
                menuToInsert.Add(autoMenu);
                existingAutoById[menuId] = new AutoMenuSnapshot(menuId, key, autoMenu.Pid, false);
            }

            bindingToInsert.Add(new GlobalMenuEndpointEntity { MenuId = menuId, EndpointId = ep.Id, IsAuto = true, });
        }

        if (menuToInsert.Count > 0)
        {
            await menuRepository.AsInsertable(menuToInsert).ExecuteCommandAsync();
        }

        foreach (var menu in menuToRestore)
        {
            menuRepository.Context.QueryFilter.ClearAndBackup();
            try
            {
                await menuRepository.AsUpdateable()
                             .SetColumns(u => new GlobalMenuEntity
                             {
                                 Pid = menu.Pid,
                                 Type = menu.Type,
                                 Key = menu.Key,
                                 Title = menu.Title,
                                 IsHide = menu.IsHide,
                                 IsEnabled = menu.IsEnabled,
                                 Permission = menu.Permission,
                                 OrderNo = menu.OrderNo,
                                 IsDeleted = false,
                             })
                             .Where(u => u.Id == menu.Id)
                             .ExecuteCommandAsync();
            }
            finally
            {
                menuRepository.Context.QueryFilter.Restore();
            }
        }

        // 仅插入不存在的 IsAuto 绑定；软删历史绑定要恢复，避免唯一索引冲突。
        List<AutoBindingSnapshot> existingAutoBindings;
        bindingRepository.Context.QueryFilter.ClearAndBackup();
        try
        {
            var rows = await bindingRepository.AsQueryable()
                                       .Where(u => u.IsAuto == true)
                                       .Select(u => new { u.MenuId, u.EndpointId, IsDeleted = u.IsDeleted == true, })
                                       .ToListAsync();
            existingAutoBindings = rows.Select(u => new AutoBindingSnapshot(u.MenuId, u.EndpointId, u.IsDeleted)).ToList();
        }
        finally
        {
            bindingRepository.Context.QueryFilter.Restore();
        }

        var existingAutoBindingByKey = existingAutoBindings.ToDictionary(b => (b.MenuId, b.EndpointId));
        var newBindings = new List<GlobalMenuEndpointEntity>();
        var bindingsToRestore = new List<GlobalMenuEndpointEntity>();
        foreach (var binding in bindingToInsert)
        {
            if (!existingAutoBindingByKey.TryGetValue((binding.MenuId, binding.EndpointId), out var existingBinding))
            {
                newBindings.Add(binding);
                continue;
            }

            if (existingBinding.IsDeleted)
            {
                bindingsToRestore.Add(binding);
            }
        }

        if (newBindings.Count > 0)
        {
            await bindingRepository.AsInsertable(newBindings).ExecuteCommandAsync();
        }

        foreach (var binding in bindingsToRestore)
        {
            bindingRepository.Context.QueryFilter.ClearAndBackup();
            try
            {
                await bindingRepository.AsUpdateable()
                                .SetColumns(u => new GlobalMenuEndpointEntity { IsDeleted = false, })
                                .Where(u => u.MenuId == binding.MenuId && u.EndpointId == binding.EndpointId)
                                .ExecuteCommandAsync();
            }
            finally
            {
                bindingRepository.Context.QueryFilter.Restore();
            }
        }

        _logger.LogInformation("自动隐藏按钮：覆盖未绑定端点 {Count} 个（按钮新增 {Menu}，绑定新增 {Bind}）。",
                               unbound.Count, menuToInsert.Count + menuToRestore.Count, newBindings.Count + bindingsToRestore.Count);
    }

    /// <summary>
    /// 构建并预热请求期端点权限缓存：(HttpMethod, RoutePath) → (EndpointId, IsPublic, IsAuthenticatedOnly, 绑定按钮 MenuId 集)。
    /// </summary>
    private async Task WarmEndpointPermissionCacheAsync(
        SqlSugarRepository<GlobalApiEndpointEntity> endpointRepository,
        SqlSugarRepository<GlobalMenuEndpointEntity> bindingRepository,
        SystemCacheService cache)
    {
        var endpoints = await endpointRepository.AsQueryable().Where(u => u.IsEnabled == true).ToListAsync();
        var bindings = await bindingRepository.AsQueryable().ToListAsync();
        var byEndpoint = bindings.GroupBy(b => b.EndpointId).ToDictionary(g => g.Key, g => g.Select(b => b.MenuId).ToHashSet());

        var map = new Dictionary<string, EndpointPermissionEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var ep in endpoints)
        {
            byEndpoint.TryGetValue(ep.Id, out var bound);
            map[ApiEndpointIdentity.ToEndpointKey(ep.HttpMethod, ep.RoutePath)] = new EndpointPermissionEntry
            {
                EndpointId = ep.Id,
                HttpMethod = ApiEndpointIdentity.NormalizeHttpMethod(ep.HttpMethod),
                RoutePath = ep.RoutePath,
                IsPublic = ep.IsPublic,
                IsAuthenticatedOnly = ep.IsAuthenticatedOnly,
                BoundMenuIds = bound ?? new HashSet<long>(),
            };
        }

        cache.Set(CacheConst.KeyEndpointByRoute, map);
        cache.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        cache.RemoveByPrefixKey(CacheConst.KeyUserButton);
    }

    private static string AutoButtonKey(string routePath)
    {
        // routePath 形如 api/systemUser/page → permission:auto:systemUser:page（稳定、可读、唯一）。
        var core = routePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ? routePath.Substring(4) : routePath;
        return "permission:auto:" + core.Replace("/", ":");
    }

    private static long ResolveAutoMenuPid(string routePath, Dictionary<string, long> controllerPidMap)
    {
        // routePath 形如 api/tenantGameAppPackage/detail（controller 段全小写），与 permission 前缀忽略大小写对齐。
        var segments = routePath.Split('/');
        var controller = segments.Length >= 2 ? segments[1] : string.Empty;
        return controllerPidMap.TryGetValue(controller, out var pid) ? pid : 0;
    }

    private static GlobalMenuEntity CreateAutoMenu(long menuId, string key, string routePath, long pid)
    {
        var title = "自动权限:" + routePath;
        if (title.Length > 64)
        {
            title = title.Substring(0, 64);
        }

        return new GlobalMenuEntity
        {
            Id = menuId,
            Pid = pid,
            Type = MenuTypeEnum.Button,
            Key = key,
            Title = title,
            IsHide = true,
            IsEnabled = true,
            Permission = null,
            OrderNo = 9999,
        };
    }

    private static string? TryGetApiDescriptionSettingsName(MethodInfo method)
    {
        foreach (var a in method.GetCustomAttributes(true))
        {
            var t = a.GetType();
            if (t.Name != "ApiDescriptionSettingsAttribute")
            {
                continue;
            }

            var name = t.GetProperty("Name")?.GetValue(a) as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return null;
    }

    private sealed class LiveEndpoint
    {
        public string HttpMethod { get; set; } = string.Empty;
        public string RoutePath { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public bool IsPublic { get; set; }
        public bool IsAuthenticatedOnly { get; set; }
    }

    private sealed record AutoMenuSnapshot(long Id, string? Key, long Pid, bool IsDeleted);

    private sealed record AutoBindingSnapshot(long MenuId, long EndpointId, bool IsDeleted);
}