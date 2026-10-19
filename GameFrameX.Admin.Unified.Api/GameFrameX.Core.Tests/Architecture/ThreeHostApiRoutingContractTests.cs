using GameFrameX.Client.Api.GameLoginTicketKey;
using GameFrameX.Client.Api.GameWebShop;
using GameFrameX.Client.Api.GameAnnouncement;
using GameFrameX.Client.Api.GameUser;
using System.Reflection;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.System.Dictionary;
using GameFrameX.Core.Provider;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Hub.Api.Controllers;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
// Client / Hub 存在同名控制器（GameArea / GameRedeemCode / GameGoods / GameAssetPackageVersion），Hub 侧用别名消歧
using HubGameAreaController = GameFrameX.Hub.Api.Controllers.GameAreaController;
using HubGameAssetPackageVersionController = GameFrameX.Hub.Api.Controllers.GameAssetPackageVersionController;
using HubGameGoodsController = GameFrameX.Hub.Api.Controllers.GameGoodsController;
using HubGameRedeemCodeController = GameFrameX.Hub.Api.Controllers.GameRedeemCodeController;

namespace GameFrameX.Core.Tests.Architecture;

/// <summary>
/// C235 三 Host API 路由契约防回潮守护测试。
/// 契约文档：gfx-doc/agent-handbook/api-routing-contract.md（单一事实来源，附录 A 采样表与本测试同源）。
/// 与 Authorization/AdminApiRouteRulesTests（规则函数层）互补：本测试走 AdminApiControllerConvention.Apply
/// 端到端推导（与运行时同一条代码路径，含 GET 单参数路径化），并锁定 Client/Hub 显式 token 路由声明
/// 与三 host 信封成功语义（code == 0）。样本断言 URL/谓词契约而非实现细节。
/// </summary>
public sealed class ThreeHostApiRoutingContractTests
{
    // ============ Admin host：动态约定端到端（附录 A 表 A1-A5） ============

    [Theory]
    [InlineData(typeof(GlobalDictionaryTypeService), nameof(GlobalDictionaryTypeService.PageAsync), "api/globalDictionaryType/page", "POST")]
    [InlineData(typeof(GlobalDictionaryTypeService), nameof(GlobalDictionaryTypeService.GetListAsync), "api/globalDictionaryType/list", "GET")]
    [InlineData(typeof(SystemCacheService), nameof(SystemCacheService.GetKeysByPrefixKey), "api/systemCache/keysByPrefixKey/{prefixKey}", "GET")]
    [InlineData(typeof(GlobalDictionaryTypeService), nameof(GlobalDictionaryTypeService.AddDictionaryTypeAsync), "api/globalDictionaryType/add", "POST")]
    [InlineData(typeof(TenantGamePenaltyEntityService), nameof(TenantGamePenaltyEntityService.RevokeAsync), "api/tenantGamePenaltyEntity/revoke", "POST")]
    public void AdminDynamicConvention_SampleEndpoints_MatchContractedRouteAndVerb(Type controllerType, string methodName, string expectedTemplate, string expectedVerb)
    {
        var applicationModel = BuildApplicationModel(controllerType, methodName);

        new AdminApiControllerConvention().Apply(applicationModel);

        var controller = Assert.Single(applicationModel.Controllers);
        var action = Assert.Single(controller.Actions);
        var selector = Assert.Single(action.Selectors);
        Assert.Equal(expectedTemplate, selector.AttributeRouteModel!.Template);
        var constraint = Assert.Single(selector.ActionConstraints.OfType<HttpMethodActionConstraint>());
        Assert.Contains(expectedVerb, constraint.HttpMethods);
    }

    // ============ Client / Hub host：显式 token 路由声明（附录 A 表 C1-C5 / H1-H5） ============

    [Fact]
    public void ClientHub_BaseControllers_DeclareTokenRouteTemplate()
    {
        // C236：Hub 本地 BaseController 副本已回收，token 路由模板唯一来源为 Core 共享版，Client/Hub 控制器经继承链取用
        AssertTokenRoute(typeof(Service.BaseController));
        AssertTokenRoute(typeof(HubGameAreaController));
        AssertTokenRoute(typeof(HubGameRedeemCodeController));
    }

    [Fact]
    public void Client_GameAnnouncementController_RenamesClassButLocksLegacyRoute()
    {
        // C300（GFX-708）：类名自 TenantGameAnnouncementEntityServiceController 清理而来；调用方（游戏客户端）
        // 依赖既有路径，显式 [Route] 锁定旧 token 解析结果。MVC 路由模板收集与 GetCustomAttributes(inherit: true)
        // 同源，此处断言 controller 级路由唯一且等于旧值，防止基类 token 路由叠加出 api/GameAnnouncement 别名端点。
        var routes = typeof(GameAnnouncementController)
                     .GetCustomAttributes(typeof(RouteAttribute), true)
                     .OfType<RouteAttribute>()
                     .Select(route => route.Template)
                     .ToArray();

        Assert.Equal(new[] { "api/TenantGameAnnouncementEntityService/[action]" }, routes);
    }

    [Fact]
    public void Hub_Controllers_RequireAuthorizationByDefault()
    {
        // C236：Hub BaseController 副本回收后，JWT 默认鉴权显式化到各非匿名控制器类；共享基类不带 [Authorize]（Client 无 auth handler）
        Assert.False(typeof(Service.BaseController).IsDefined(typeof(AuthorizeAttribute), true));
        Assert.True(typeof(HubGameAreaController).IsDefined(typeof(AuthorizeAttribute), true));
        Assert.True(typeof(HubGameRedeemCodeController).IsDefined(typeof(AuthorizeAttribute), true));
        Assert.True(typeof(HubGameAssetPackageVersionController).IsDefined(typeof(AuthorizeAttribute), true));
        Assert.True(typeof(HubGameGoodsController).IsDefined(typeof(AuthorizeAttribute), true));
        // TapdWebHook：类级 [Authorize] fail-closed + 唯一 action 带 [AllowAnonymous]；PackageBuildReport：类级 [AllowAnonymous] + OpenAccess 中间件鉴权
        Assert.True(typeof(TapdWebHookController).IsDefined(typeof(AuthorizeAttribute), true));
        Assert.True(typeof(PackageBuildReportController).IsDefined(typeof(AllowAnonymousAttribute), false));
    }

    [Theory]
    [InlineData(typeof(GameUserController), nameof(GameUserController.WeChatLogin), "POST", null)]
    [InlineData(typeof(GameAnnouncementController), nameof(GameAnnouncementController.List), "GET", null)]
    [InlineData(typeof(GameWebShopController), nameof(GameWebShopController.NotifyWebShop), "GET", "{tenantId:long}/{appId:long}/{packageId:long?}")]
    [InlineData(typeof(GameOrderController), nameof(GameOrderController.Create), "POST", null)]
    [InlineData(typeof(GameLoginTicketKeyController), nameof(GameLoginTicketKeyController.PublicKeys), "GET", null)]
    [InlineData(typeof(GameGoodsController), nameof(GameGoodsController.AddOrUpdate), "POST", null)]
    [InlineData(typeof(HubGameAreaController), nameof(HubGameAreaController.ServerVersionAdd), "POST", null)]
    [InlineData(typeof(TapdWebHookController), nameof(TapdWebHookController.WebHookTaskFinish), "POST", null)]
    [InlineData(typeof(PackageBuildReportController), nameof(PackageBuildReportController.Report), "POST", null)]
    [InlineData(typeof(HubGameRedeemCodeController), nameof(HubGameRedeemCodeController.Add), "POST", null)]
    public void ClientHub_SampleEndpoints_DeclareExplicitVerbAndTemplate(Type controllerType, string methodName, string expectedVerb, string? expectedTemplate)
    {
        var method = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);

        var attribute = method.GetCustomAttributes(typeof(HttpMethodAttribute), true).OfType<HttpMethodAttribute>().Single();
        Assert.Contains(expectedVerb, attribute.HttpMethods);
        Assert.Equal(expectedTemplate, attribute.Template);
    }

    // ============ 信封成功语义三边一致（契约 §4） ============

    [Fact]
    public void EnvelopeSuccessCode_IsZeroAcrossThreeHosts()
    {
        // Foundation 层：三 host 共用同一信封类型
        Assert.Equal(0, HttpJsonResultData<object>.Success(new object()).Code);
        // Admin host 成功包装链路：AdminResultFilter -> AdminResultFactory.Create -> Success
        Assert.Equal(0, AdminResultFactory.Create(new object()).Code);
        // Client / Hub / Game：成功信封直接走 Foundation Success 出口（旧三 host ResultCode 的 Ok 成员已随 C264 统一错误码退役）
        Assert.Equal(0, HttpJsonResultData<object>.Success().Code);
        Assert.Equal(0, HttpJsonResultData<string>.Success().Code);
    }

    /// <summary>
    /// 手工组装只含单个 action 的 ApplicationModel，复现 MVC 模型构建产物，
    /// 让 AdminApiControllerConvention.Apply 走与运行时一致的完整推导（含参数绑定决策）。
    /// </summary>
    private static ApplicationModel BuildApplicationModel(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     ?? throw new InvalidOperationException($"Method not found: {controllerType.Name}.{methodName}");

        var actionModel = new ActionModel(method, method.GetCustomAttributes(true));
        foreach (var parameter in method.GetParameters())
        {
            actionModel.Parameters.Add(new ParameterModel(parameter, parameter.GetCustomAttributes(true))
            {
                // 框架由 ControllerActionApplicationModelProvider 赋值；ctor 不推导，漏掉则路由参数匹配失效
                ParameterName = parameter.Name!,
            });
        }

        var controllerModel = new ControllerModel(controllerType.GetTypeInfo(), controllerType.GetCustomAttributes(true));
        controllerModel.Actions.Add(actionModel);
        // MVC 框架由 DefaultApplicationModelProvider 回填 Controller 引用；手工组装需补上，AddSelector 会读取它
        actionModel.Controller = controllerModel;

        var applicationModel = new ApplicationModel();
        applicationModel.Controllers.Add(controllerModel);
        return applicationModel;
    }

    private static void AssertTokenRoute(Type baseControllerType)
    {
        var route = baseControllerType.GetCustomAttributes(typeof(RouteAttribute), true).OfType<RouteAttribute>().Single();
        Assert.Equal("api/[controller]/[action]", route.Template);
    }
}