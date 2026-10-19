using System.Reflection;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Authorization.Diagnostics;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Web.Api.Handlers.Diagnostics;

/// <summary>
/// 菜单权限/endpoint 绑定漂移诊断薄壳服务（C128）。
/// <para>把 DB（<see cref="GlobalApiEndpointEntity"/> / <see cref="GlobalMenuEndpointEntity"/> / <see cref="GlobalMenuEntity"/>）
/// 与 ASP.NET Core MVC 活端点（<see cref="IActionDescriptorCollectionProvider"/>）
/// 投影成 <see cref="BindingDiagnosticsRequest"/> 快照，委托 <see cref="MenuEndpointBindingDiagnosticsEngine"/> 产出报告。</para>
/// <para><b>只读、不写 DB、不触发</b><see cref="ApiEndpointCatalogSync"/>。采集口径与启动同步器保持一致。</para>
/// </summary>
public sealed class MenuEndpointBindingDiagnosticsService
{
    private readonly SqlSugarRepository<GlobalApiEndpointEntity> _endpointRepository;
    private readonly SqlSugarRepository<GlobalMenuEndpointEntity> _bindingRepository;
    private readonly SqlSugarRepository<GlobalMenuEntity> _menuRepository;
    private readonly IActionDescriptorCollectionProvider _actionDescriptors;

    public MenuEndpointBindingDiagnosticsService(
        SqlSugarRepository<GlobalApiEndpointEntity> endpointRepository,
        SqlSugarRepository<GlobalMenuEndpointEntity> bindingRepository,
        SqlSugarRepository<GlobalMenuEntity> menuRepository,
        IActionDescriptorCollectionProvider actionDescriptors)
    {
        _endpointRepository = endpointRepository;
        _bindingRepository = bindingRepository;
        _menuRepository = menuRepository;
        _actionDescriptors = actionDescriptors;
    }

    /// <summary>
    /// 采集当前 DB + 活端点快照并执行诊断。
    /// </summary>
    public async Task<BindingDiagnosticsReport> DiagnoseAsync()
    {
        var input = new BindingDiagnosticsRequest
        {
            LiveEndpoints = CollectLiveEndpoints(),
            EndpointRows = await _endpointRepository.AsQueryable()
                                             .Select(u => new EndpointRowSnapshot
                                             {
                                                 Id = u.Id,
                                                 HttpMethod = u.HttpMethod,
                                                 RoutePath = u.RoutePath,
                                                 IsPublic = u.IsPublic,
                                                 IsAuthenticatedOnly = u.IsAuthenticatedOnly,
                                                 IsOrphan = u.IsOrphan,
                                                 IsEnabled = u.IsEnabled == true,
                                             })
                                             .ToListAsync(),
            Bindings = await _bindingRepository.AsQueryable()
                                        .Select(u => new BindingSnapshot
                                        {
                                            MenuId = u.MenuId,
                                            EndpointId = u.EndpointId,
                                            IsAuto = u.IsAuto,
                                        })
                                        .ToListAsync(),
            Menus = await _menuRepository.AsQueryable()
                                  .Select(u => new MenuSnapshot
                                  {
                                      Id = u.Id,
                                      Type = (int)u.Type,
                                      Permission = u.Permission,
                                      IsEnabled = u.IsEnabled == true,
                                      IsDeleted = u.IsDeleted == true,
                                  })
                                  .ToListAsync(),
        };

        return MenuEndpointBindingDiagnosticsEngine.Diagnose(input);
    }

    // 与 ApiEndpointCatalogSync.CollectLiveEndpoints 同口径：仅 [ApiDescriptionSettings]/约定模板、api/ 前缀、非 NonAction。
    // 不在此处理登录态白名单（那是 DB 行的 IsAuthenticatedOnly 标志，已由 EndpointRows 携带）。
    private List<LiveEndpointSnapshot> CollectLiveEndpoints()
    {
        var map = new Dictionary<string, LiveEndpointSnapshot>(StringComparer.OrdinalIgnoreCase);
        foreach (var ad in _actionDescriptors.ActionDescriptors.Items)
        {
            if (ad is not ControllerActionDescriptor cad)
            {
                continue;
            }

            var template = cad.AttributeRouteInfo?.Template;
            if (string.IsNullOrWhiteSpace(template))
            {
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
            var httpMethods = AdminApiEndpointMetadata.GetHttpMethods(cad.EndpointMetadata);

            foreach (var httpMethod in httpMethods)
            {
                var method = ApiEndpointIdentity.NormalizeHttpMethod(httpMethod);
                var key = ApiEndpointIdentity.ToEndpointKey(method, routePath);
                if (string.IsNullOrEmpty(key) || map.ContainsKey(key))
                {
                    continue;
                }

                map[key] = new LiveEndpointSnapshot
                {
                    HttpMethod = method,
                    RoutePath = routePath,
                    IsPublic = isPublic,
                    Controller = cad.ControllerName,
                    Action = cad.ActionName,
                };
            }
        }

        return map.Values.ToList();
    }
}