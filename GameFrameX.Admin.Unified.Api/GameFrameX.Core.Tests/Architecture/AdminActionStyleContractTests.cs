using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.System.Dictionary;
using GameFrameX.Core.Api;
using GameFrameX.Core.Service.Base;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Core.Tests.Architecture;

// C257：Admin API Action 写法规范契约测试。规范正文：gfx-doc/agent-handbook/admin-api-action-style.md（单一事实来源）。
// 六规则 + 试点 URL 冻结断言：
//   S1 显式模板必填（每个 declared public action 必须有非空相对模板 [HttpXxx(...)]，三种字面形态均可；override 继承基类特性视为合规）
//   S2 禁绝对路径模板（唯一长期豁免：SystemAuthService swagger 集成端点）
//   S3 禁 action 路由段 Async 后缀（C289 起方法名 Async 由 AsyncSuffixContractTests 强制，URL 段契约独立守护）
//   S4 禁类级 [AllowAnonymous]（GFX-664/C258 匿名写可达收口；C258 落地后例外清零，死条目断言强制清理）
//   S5 declared public action 必须带 [DisplayName]（Swagger 展示名；inherit:true 继承基类的算数）
//   S6 服务文件裸中文 BusinessError.Oh("…") 快照（只减不增；错误抛出唯一标准 BusinessError.Oh(XxxErrorCode.X)，见规范 §6）
//   S7 试点 GlobalDictionaryTypeService URL 冻结端到端断言（显式模板重写前后 api/globalDictionaryType/* 路由与谓词逐条一致）
// 双向断言：新增违规失败（防回潮）；例外清单出现死条目也失败（防僵尸例外，参照 DtoNamingConventionTests 模式）。
// 例外清单随按模块清零批次（notes.md 批次 A1→B3）只减不增；试点服务 GlobalDictionaryTypeService 重写完成后即从 S1 例外移除。
public sealed class AdminActionStyleContractTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    // ============ S1 显式模板必填：类粒度例外（该类全部 declared public action 豁免，整类重写批清零） ============

    private static readonly HashSet<string> ExplicitTemplateLegacyClasses = new(StringComparer.Ordinal)
    {
        "TenantGameAppAssetPackageVersionService",
        "TenantGameAppPackageService",
        "TenantGameMailCampaignEntityService",
        "GlobalDictionaryDataService",
        "GlobalMenuService",
        "GlobalPackageService",
        "LogExceptionService",
        "LogOperationService",
        "PlatformCaptchaConfigService",
        "PlatformJwtConfigService",
        "PlatformLoggingConfigService",
        "PlatformPasswordPolicyConfigService",
        "PlatformRateLimitConfigService",
        "SystemAuthService",
        "SystemCacheService",
        "SystemConfigService",
        "SystemConstService",
        "SystemDatabaseService",
        "SystemEmailProviderService",
        "SystemEnumService",
        "SystemJobService",
        "SystemMessageService",
        "SystemOpenAccessService",
        "SystemPluginService",
        "SystemRegionService",
        "SystemServerService",
        "SystemTenantService",
        "TenantAdvertisementCallbackConfigEntityService",
        "TenantChannelDefinitionService",
        "TenantEmailConfigEntityService",
        "TenantFileService",
        "TenantGameAppEntityService",
        "TenantGameAppOpenAccessService",
        "TenantGameAreaEntityService",
        "TenantGameMailEntityService",
        "TenantGameOrderEntityService",
        "TenantGameOrderFulfillmentEntityService",
        "TenantGamePaymentConfigEntityService",
        "TenantGameRedeemCodeUseRecordEntityService",
        "TenantGameRoleEntityService",
        "TenantGameUserEntityService",
        "TenantGameWebShopOrderEntityService",
        "TenantNoticeService",
        "TenantObjectStorageConfigEntityService",
        "TenantOrganizationService",
        "TenantPositionService",
        "TenantPrintService",
        "TenantRoleService",
        "TenantSmsConfigEntityService",
        "TenantUserService",
        "TenantWechatConfigEntityService",
        "TenantWechatOfficialAccountService",
        "TenantWechatUserService",
    };

    // ============ S2 绝对路径模板：TypeName.MethodName 粒度 ============

    private static readonly HashSet<string> AbsoluteTemplateLegacy = new(StringComparer.Ordinal)
    {
        // SystemAuthService swagger 集成专用端点（规范 §1 唯一长期豁免）
        "SystemAuthService.SwaggerCheckUrl",
        "SystemAuthService.SwaggerSubmitUrlAsync",
    };

    // ============ S3 Async 后缀：TypeName.MethodName 粒度 ============

    private static readonly HashSet<string> AsyncRouteSegmentLegacy = new(StringComparer.Ordinal)
    {
        // 无 Name 无模板，路由段 = getEndpointPermissionMapAsync（C257 遗留）；动词收敛批消灭
        "GlobalMenuService.GetEndpointPermissionMapAsync",
    };

    // ============ S4 类级 [AllowAnonymous]：C258（GFX-664）落地后本清单必须清零 ============

    private static readonly HashSet<string> ClassLevelAllowAnonymousLegacy = new(StringComparer.Ordinal)
    {
    };

    // ============ S5 [DisplayName] 存在性：TypeName.MethodName 粒度 ============

    private static readonly HashSet<string> DisplayNameLegacy = new(StringComparer.Ordinal)
    {
        "TenantGameAppAssetPackageService.SelectAsync",
        "TenantGameAppAssetPackageVersionService.SelectAsync",
        "TenantGameAppRemoteConfigModeService.AddAsync",
        "TenantGameAppPackageService.AddAsync",
        "TenantGameAppPackageService.DeleteAsync",
        "TenantGameAppPackageService.SelectAsync",
        "TenantGameAppPackageService.UpdateAsync",
        "TenantGameAppVersionDefinitionService.SelectAsync",
        "TenantGameMailCampaignEntityService.DeleteAsync",
        "GlobalLanguageService.SelectAsync",
        "SystemEmailProviderService.AddAsync",
        "SystemEmailProviderService.GetAsync",
        "SystemEmailProviderService.PageAsync",
        "SystemEmailProviderService.UpdateAsync",
        "TenantAdvertisementCallbackConfigEntityService.AddAsync",
        "TenantAdvertisementCallbackConfigEntityService.GetAsync",
        "TenantAdvertisementCallbackConfigEntityService.PageAsync",
        "TenantAdvertisementCallbackConfigEntityService.UpdateAsync",
        "TenantAppSignatureKeyEntityService.AddAsync",
        "TenantAppSignatureKeyEntityService.DeleteAsync",
        "TenantAppSignatureKeyEntityService.DisableAsync",
        "TenantAppSignatureKeyEntityService.EnableAsync",
        "TenantAppSignatureKeyEntityService.PageAsync",
        "TenantAppSignatureKeyEntityService.RenewAsync",
        "TenantAppSignatureKeyEntityService.RotateAsync",
        "TenantDeviceGroupService.AddAsync",
        "TenantDeviceGroupService.SelectAsync",
        "TenantGameAppDeviceService.SelectAsync",
        "TenantEmailConfigEntityService.AddAsync",
        "TenantEmailConfigEntityService.GetAsync",
        "TenantEmailConfigEntityService.PageAsync",
        "TenantEmailConfigEntityService.UpdateAsync",
        "TenantGameAntiCheatEventEntityService.DetailAsync",
        "TenantGameAntiCheatEventEntityService.PageAsync",
        "TenantGameAntiCheatEventEntityService.ReviewAsync",
        "TenantGameAntiCheatEventEntityService.SubmitAsync",
        "TenantGameAppEntityService.AddAsync",
        "TenantGameAppEntityService.GetAsync",
        "TenantGameAppEntityService.PageAsync",
        "TenantGameGoodsEntityService.SelectStringAsync",
        "TenantGameLoginTicketKeyEntityService.DeleteAsync",
        "TenantGameLoginTicketKeyEntityService.DetailAsync",
        "TenantGameLoginTicketKeyEntityService.DisableAsync",
        "TenantGameLoginTicketKeyEntityService.EnableAsync",
        "TenantGameLoginTicketKeyEntityService.PageAsync",
        "TenantGameLoginTicketKeyEntityService.RotateAsync",
        "TenantGamePaymentConfigEntityService.AddAsync",
        "TenantGamePaymentConfigEntityService.GetAsync",
        "TenantGamePaymentConfigEntityService.PageAsync",
        "TenantGamePaymentConfigEntityService.UpdateAsync",
        "TenantGamePenaltyAppealEntityService.AddAsync",
        "TenantGamePenaltyAppealEntityService.ReviewAsync",
        "TenantGamePenaltyEntityService.AddAsync",
        "TenantGamePenaltyEntityService.RevokeAsync",
        "TenantGameReportEntityService.AddAsync",
        "TenantGameReportEntityService.LinkPenaltyAsync",
        "TenantGameRewardTemplateEntityService.CopySnapshotAsync",
        "TenantGameRewardTemplateEntityService.SelectAsync",
        "TenantGameRewardTemplateGroupEntityService.SelectAsync",
        "TenantGameRoleEntityService.GetAsync",
        "TenantGameRoleEntityService.PageAsync",
        "TenantGameRoleEntityService.UpdateAsync",
        "TenantObjectStorageConfigEntityService.AddAsync",
        "TenantObjectStorageConfigEntityService.GetAsync",
        "TenantObjectStorageConfigEntityService.PageAsync",
        "TenantObjectStorageConfigEntityService.UpdateAsync",
        "TenantSessionKeyEntityService.AddAsync",
        "TenantSessionKeyEntityService.DeleteAsync",
        "TenantSessionKeyEntityService.DetailAsync",
        "TenantSessionKeyEntityService.DisableAsync",
        "TenantSessionKeyEntityService.EnableAsync",
        "TenantSessionKeyEntityService.PageAsync",
        "TenantSessionKeyEntityService.RenewAsync",
        "TenantSessionKeyEntityService.RotateAsync",
        "TenantSmsConfigEntityService.AddAsync",
        "TenantSmsConfigEntityService.GetAsync",
        "TenantSmsConfigEntityService.PageAsync",
        "TenantSmsConfigEntityService.UpdateAsync",
        "TenantWechatConfigEntityService.AddAsync",
        "TenantWechatConfigEntityService.GetAsync",
        "TenantWechatConfigEntityService.PageAsync",
        "TenantWechatConfigEntityService.UpdateAsync",
    };

    // ============ S6 裸中文 BusinessError.Oh：服务文件 → 计数快照（只减不增） ============

    private static readonly Dictionary<string, int> BareChineseErrorSnapshot = new(StringComparer.Ordinal)
    {
        { "GameFrameX.Application.System/DataBase/SystemDatabaseService.cs", 3 },
        { "GameFrameX.Application.System/Tenant/Device/TenantGameAppDevice/TenantGameAppDeviceService.cs", 3 },
        { "GameFrameX.Application.System/Tenant/Device/TenantDeviceGroup/TenantDeviceGroupService.cs", 3 },
        { "GameFrameX.Application.System/ChannelDefinition/ChannelDefinitionManagementServiceBase.cs", 16 },
        { "GameFrameX.Application.System/Const/SystemConstService.cs", 1 },
        { "GameFrameX.Application.System/Region/SystemRegionService.cs", 2 },
        { "GameFrameX.Application.Game/Tenant/AntiCheat/TenantGameAntiCheatEvent/TenantGameAntiCheatEventEntityService.cs", 4 },
        { "GameFrameX.Application.Game/Tenant/Integration/TenantSmsConfig/TenantSmsConfigEntityService.cs", 13 },
        { "GameFrameX.Application.Game/Tenant/Penalty/TenantGamePenalty/TenantGamePenaltyEntityService.cs", 4 },
        { "GameFrameX.Application.Game/Tenant/Report/TenantGameReport/TenantGameReportEntityService.cs", 6 },
        { "GameFrameX.Application.Game/Tenant/Integration/TenantObjectStorageConfig/TenantObjectStorageConfigEntityService.cs", 8 },
        { "GameFrameX.Application.Game/Tenant/Penalty/TenantGamePenaltyAppeal/TenantGamePenaltyAppealEntityService.cs", 6 },
        { "GameFrameX.Application.Game/Reward/GameRewardTemplate/TenantGameRewardTemplateEntityService.cs", 9 },
        { "GameFrameX.Application.Game/Mail/GameMailCampaign/TenantGameMailCampaignEntityService.cs", 3 },
        { "GameFrameX.Application.Game/Tenant/Security/TenantSessionKey/TenantSessionKeyEntityService.cs", 6 },
        { "GameFrameX.Application.Game/App/GameApp/TenantGameAppEntityService.cs", 1 },
        { "GameFrameX.Application.Game/App/GameAppPackage/TenantGameAppPackageService.cs", 15 },
        { "GameFrameX.Application.Game/Tenant/Security/TenantAppSignatureKey/TenantAppSignatureKeyEntityService.cs", 6 },
        { "GameFrameX.Application.Game/Order/GamePaymentConfig/TenantGamePaymentConfigEntityService.cs", 8 },
        { "GameFrameX.Application.Game/Tenant/Integration/TenantWechatConfig/TenantWechatConfigEntityService.cs", 10 },
        { "GameFrameX.Application.Game/System/Email/SystemEmailProviderService.cs", 10 },
        { "GameFrameX.Application.Game/Tenant/LoginTicket/TenantGameLoginTicketKey/TenantGameLoginTicketKeyEntityService.cs", 2 },
        { "GameFrameX.Application.Game/Tenant/Integration/TenantEmailConfig/TenantEmailConfigEntityService.cs", 8 },
        { "GameFrameX.Application.Game/Tenant/Integration/TenantAdvertisementCallbackConfig/TenantAdvertisementCallbackConfigEntityService.cs", 7 },
        { "GameFrameX.Application.Game/RedeemCode/GameRedeemCodeChannel/TenantGameRedeemCodeChannelEntityService.cs", 1 },
        { "GameFrameX.Application.Game/App/GameAppRemoteConfigMode/TenantGameAppRemoteConfigModeService.cs", 2 },
        { "GameFrameX.Application.Game/App/GameAppReleaseVersion/TenantGameAppReleaseVersionService.cs", 1 },
        { "GameFrameX.Application.Game/App/GameAppAssetPackageVersion/TenantGameAppAssetPackageVersionService.cs", 3 },
        { "GameFrameX.Application.Game/App/GameAppRemoteConfigKey/TenantGameAppRemoteConfigKeyService.cs", 6 },
        { "GameFrameX.Application.Game/App/GameAppAssetPackage/TenantGameAppAssetPackageService.cs", 3 },
        { "GameFrameX.Application.Game/App/AppScope/AssetPackageScopeValidator.cs", 3 },
        { "GameFrameX.Application.Game/App/AppScope/PackageScopeValidator.cs", 2 },
        { "GameFrameX.Application.Game/App/GameAppVersionDefinition/TenantGameAppVersionDefinitionService.cs", 2 },
        { "GameFrameX.Application.Game/App/GameAppRemoteConfig/TenantGameAppRemoteConfigService.cs", 5 },
        { "GameFrameX.Application.Game/App/GameAppServerConfig/TenantGameAppServerConfigService.cs", 3 },
        { "GameFrameX.Core/Service/Role/TenantRoleService.cs", 1 },
        { "GameFrameX.Core/Service/File/TenantFileService.cs", 7 },
        { "GameFrameX.Core/Service/Config/SystemConfigService.cs", 1 },
        { "GameFrameX.Core/Service/PlatformCaptcha/PlatformCaptchaConfigProvider.cs", 18 },
        { "GameFrameX.Core/Service/PlatformCaptcha/PlatformCaptchaConfigService.cs", 2 },
        { "GameFrameX.Core/Service/Auth/SystemAuthService.cs", 5 },
        { "GameFrameX.Core/Service/Organization/TenantOrganizationService.cs", 2 },
        { "GameFrameX.Core/Service/User/TenantUserService.cs", 1 },
        { "GameFrameX.Core/Service/PlatformLogging/PlatformLoggingConfigProvider.cs", 9 },
        { "GameFrameX.Core/Service/PlatformLogging/PlatformLoggingConfigService.cs", 2 },
        { "GameFrameX.Core/Service/TenantPackage/GlobalPackageService.cs", 4 },
        { "GameFrameX.Core/Service/PlatformPasswordPolicy/PlatformPasswordPolicyConfigProvider.cs", 6 },
        { "GameFrameX.Core/Service/PlatformPasswordPolicy/PlatformPasswordPolicyConfigService.cs", 2 },
        { "GameFrameX.Core/Service/PlatformJwt/PlatformJwtConfigService.cs", 1 },
        { "GameFrameX.Core/Service/PlatformJwt/PlatformJwtRuntimeProvider.cs", 12 },
        { "GameFrameX.Core/Service/PlatformJwt/PlatformJwtTokenService.cs", 2 },
        { "GameFrameX.Core/Service/PlatformRateLimit/PlatformRateLimitConfigProvider.cs", 7 },
        { "GameFrameX.Core/Service/PlatformRateLimit/PlatformRateLimitConfigService.cs", 2 },
    };

    private static readonly Regex BareChineseErrorRegex = new(@"BusinessError\.Oh\(\s*\$?""", RegexOptions.Compiled);

    // 服务文件清单来源：与盘点（C257 notes.md / inventory.json）同源——Admin host 四项目中声明 IAdminApiController 家族服务类的 .cs
    private static readonly string[] AdminServiceDirectories =
    {
        "GameFrameX.Application.System",
        "GameFrameX.Application.Game",
        "GameFrameX.Core/Service",
    };

    [Fact]
    public void S1_DeclaredPublicActions_RequireExplicitTemplate()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            var declaredPublicActions = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                            .Where(m => !m.IsSpecialName && !m.IsDefined(typeof(NonActionAttribute), false))
                                            .ToList();
            var hasViolation = false;
            foreach (var method in declaredPublicActions)
            {
                var template = GetHttpTemplate(method);
                if (template is null)
                {
                    hasViolation = true;
                    break;
                }
            }

            if (hasViolation)
            {
                actual.Add(type.Name);
            }
        }

        AssertNoNewAndNoStale("S1", actual, ExplicitTemplateLegacyClasses,
                              "Admin host 服务类每个公开 action 必须带非空相对模板的 [HttpXxx(...)]（规范 §1 全显式模板式）；override 继承基类显式特性视为合规。存量未重写服务走类粒度例外清单，按模块清零批次（A1→B3）移除。");
    }

    [Fact]
    public void S2_HttpTemplates_MustNotBeAbsolutePath()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var template = GetHttpTemplate(method);
                if (template is not null && template.StartsWith("/", StringComparison.Ordinal))
                {
                    actual.Add(type.Name + "." + method.Name);
                }
            }
        }

        AssertNoNewAndNoStale("S2", actual, AbsoluteTemplateLegacy,
                              "显式模板禁止绝对路径（以 / 开头会绕过 api/{controller} 前缀）；swagger 集成端点为规范 §1 唯一长期豁免。");
    }

    [Fact]
    public void S3_ActionRouteSegments_MustNotEndWithAsync()
    {
        // C289（GFX-696）规范更新：public/internal Task 方法名必须带 Async 后缀（AsyncSuffixContractTests 强制），
        // C257 的“方法名禁 Async”退役；本规则收敛为 URL 契约守护 —— action 路由段来源
        // （ApiDescriptionSettings.Name，其次无模板时的方法名）禁止 Async 后缀，
        // 防止 URL 随方法名漂移（全量路由由 AdminEndpointGoldenSnapshotTests 冻结）。
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.IsSpecialName || method.IsDefined(typeof(NonActionAttribute), false))
                {
                    continue;
                }

                var configuredName = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true)?.Name;
                var segmentSource = configuredName ?? (GetHttpTemplate(method) is null ? method.Name : null);
                if (segmentSource != null && segmentSource.EndsWith("Async", StringComparison.Ordinal))
                {
                    actual.Add(type.Name + "." + method.Name);
                }
            }
        }

        AssertNoNewAndNoStale("S3", actual, AsyncRouteSegmentLegacy,
                              "action 路由段来源（Name 特性 / 无模板时方法名）禁止 Async 后缀（C289 起方法名 Async 后缀由 AsyncSuffixContractTests 强制，URL 段契约独立守护）；URL 冻结期存量走例外。");
    }

    [Fact]
    public void S4_ServiceClasses_MustNotHaveClassLevelAllowAnonymous()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            if (type.IsDefined(typeof(AllowAnonymousAttribute), false))
            {
                actual.Add(type.Name);
            }
        }

        AssertNoNewAndNoStale("S4", actual, ClassLevelAllowAnonymousLegacy,
                              "服务类禁止类级 [AllowAnonymous]（类级特性使全部 action 含写操作匿名可达，GFX-664）；公开端点一律 action 级标注 + 目录 IsPublic 登记。C258 落地后例外清单必须清零。");
    }

    [Fact]
    public void S5_DeclaredPublicActions_RequireDisplayName()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.IsSpecialName || method.IsDefined(typeof(NonActionAttribute), false))
                {
                    continue;
                }

                if (GetHttpTemplate(method) is null)
                {
                    // 无显式模板的存量（S1 例外类）不重复计 DisplayName 例外；重写批补模板时同步补 DisplayName
                    continue;
                }

                if (!method.IsDefined(typeof(DisplayNameAttribute), true))
                {
                    actual.Add(type.Name + "." + method.Name);
                }
            }
        }

        AssertNoNewAndNoStale("S5", actual, DisplayNameLegacy,
                              "公开 action 必须带 [DisplayName(\"中文\")]（规范 §7 元数据必填，Swagger 展示名）。");
    }

    [Fact]
    public void S6_ServiceFiles_BareChineseBusinessError_SnapshotOnlyDecreases()
    {
        var actual = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var file in EnumerateAdminServiceSourceFiles())
        {
            var source = File.ReadAllText(file);
            var count = BareChineseErrorRegex.Matches(StripComments(source)).Count;
            if (count > 0)
            {
                actual[ToRelative(file)] = count;
            }
        }

        var added = actual.Where(kv => !BareChineseErrorSnapshot.TryGetValue(kv.Key, out var expected) || expected != kv.Value)
                          .Select(kv => BareChineseErrorSnapshot.TryGetValue(kv.Key, out var expected) && expected != kv.Value
                                            ? kv.Key + ": " + expected + " -> " + kv.Value
                                            : kv.Key + ": 0 -> " + kv.Value)
                          .Order(StringComparer.Ordinal)
                          .ToList();
        Assert.True(added.Count == 0,
                    "S6 裸中文 BusinessError.Oh(\"…\") 快照只减不增（规范 §6 错误抛出唯一标准 BusinessError.Oh(XxxErrorCode.X)；模块错误码缺项先补枚举再抛）。变化的文件:" +
                    Environment.NewLine + string.Join(Environment.NewLine, added) +
                    Environment.NewLine + "若为清理，请同步更新 BareChineseErrorSnapshot。");
    }

    // ============ S7 试点 URL 冻结：GlobalDictionaryTypeService 显式模板重写前后路由逐条一致 ============

    [Theory]
    [InlineData(nameof(GlobalDictionaryTypeService.PageAsync), "api/globalDictionaryType/page", "POST")]
    [InlineData(nameof(GlobalDictionaryTypeService.GetListAsync), "api/globalDictionaryType/list", "GET")]
    [InlineData(nameof(GlobalDictionaryTypeService.GetDataListAsync), "api/globalDictionaryType/dataList", "GET")]
    [InlineData(nameof(GlobalDictionaryTypeService.AddDictionaryTypeAsync), "api/globalDictionaryType/add", "POST")]
    [InlineData(nameof(GlobalDictionaryTypeService.UpdateDictionaryTypeAsync), "api/globalDictionaryType/update", "POST")]
    [InlineData(nameof(GlobalDictionaryTypeService.DeleteDictionaryTypeAsync), "api/globalDictionaryType/delete", "POST")]
    [InlineData(nameof(GlobalDictionaryTypeService.GetDetailAsync), "api/globalDictionaryType/detail", "GET")]
    [InlineData(nameof(GlobalDictionaryTypeService.SetStatusAsync), "api/globalDictionaryType/setStatus", "POST")]
    [InlineData(nameof(GlobalDictionaryTypeService.GetAllDictionaryListAsync), "api/globalDictionaryType/allDictionaryList", "GET")]
    public void S7_PilotGlobalDictionaryType_RouteAndVerbFrozenAfterExplicitTemplateRewrite(string methodName, string expectedTemplate, string expectedVerb)
    {
        var applicationModel = BuildApplicationModel(typeof(GlobalDictionaryTypeService), methodName);
        new AdminApiControllerConvention().Apply(applicationModel);

        var controller = Assert.Single(applicationModel.Controllers);
        var action = Assert.Single(controller.Actions);
        var selector = Assert.Single(action.Selectors);
        Assert.Equal(expectedTemplate, selector.AttributeRouteModel!.Template);
        var constraint = Assert.Single(selector.ActionConstraints.OfType<HttpMethodActionConstraint>());
        Assert.Contains(expectedVerb, constraint.HttpMethods);
    }

    // ============ 工具 ============

    /// <summary>取 action 的 HTTP 特性模板；三种字面形态（字符串 / CrudActionRoutes 常量 / nameof）经特性实例统一为 Template 值。</summary>
    private static string? GetHttpTemplate(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<HttpMethodAttribute>(true);
        if (attribute is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(attribute.Template) ? null : attribute.Template.Trim();
    }

    private static IEnumerable<Type> EnumerateAdminServiceTypes()
    {
        var assemblies = new[]
        {
            typeof(GlobalDictionaryTypeService).Assembly, // GameFrameX.Application.System
            typeof(TenantGamePenaltyEntityService).Assembly, // GameFrameX.Application.Game
            typeof(BaseService<>).Assembly, // GameFrameX.Core
        };
        foreach (var assembly in assemblies.Distinct())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition
                    && typeof(IAdminApiController).IsAssignableFrom(type)
                    && type.Assembly == assembly)
                {
                    yield return type;
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateAdminServiceSourceFiles()
    {
        foreach (var directory in AdminServiceDirectories)
        {
            var path = Path.Combine(RepositoryRoot, directory);
            if (!Directory.Exists(path))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                if (IsBuildOutput(file))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    private static string StripComments(string source)
    {
        source = Regex.Replace(source, @"//[^\n]*", string.Empty);
        source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return source;
    }

    private static void AssertNoNewAndNoStale(string ruleId, HashSet<string> actual, HashSet<string> expected, string ruleDescription)
    {
        var newViolations = actual.Except(expected).Order(StringComparer.Ordinal).ToList();
        var staleExceptions = expected.Except(actual).Order(StringComparer.Ordinal).ToList();

        Assert.True(
            newViolations.Count == 0,
            ruleDescription + " 新增违规:" + Environment.NewLine + string.Join(Environment.NewLine, newViolations));
        Assert.True(
            staleExceptions.Count == 0,
            ruleId + " 例外清单存在已修复的僵尸条目（存量清理后必须同步删除对应例外项）:" + Environment.NewLine + string.Join(Environment.NewLine, staleExceptions));
    }

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

    private static bool IsBuildOutput(string file)
    {
        return file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static string ToRelative(string file)
    {
        return Path.GetRelativePath(RepositoryRoot, file).Replace('\\', '/');
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }
}