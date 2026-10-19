using System.Reflection;
using GameFrameX.Core.DependencyInjection;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.Enum;

/// <summary>
/// AdminEnum 公开边界契约测试（C165）。
/// 验证 SystemEnumService 改用 [AdminEnum] 显式标记后，与历史 Enum.json 程序集白名单行为等价：
/// 标记集合 == GameFrameX.Core + GameFrameX.Entity 程序集（枚举原独立程序集 Core.Enum 已并入）的全部 public enum，未标记枚举被过滤。
/// </summary>
public class AdminEnumVisibilityTests
{
    /// <summary>
    /// 历史 Enum.json 白名单暴露的两个程序集。
    /// </summary>
    private static readonly string[] LegacyWhitelistAssemblies =
    {
        "GameFrameX.Core",
        "GameFrameX.Entity",
    };

    private static List<Type> GetLegacyWhitelistPublicEnums()
    {
        // 用具体类型触发程序集加载，确保目标程序集已被 AppDomain 装载
        var assemblies = new[]
        {
            typeof(ProjectTypeCatalog).Assembly, // GameFrameX.Core
            typeof(AccountTypeEnum).Assembly, // GameFrameX.Entity
        };

        return assemblies
               .Where(a => LegacyWhitelistAssemblies.Contains(a.GetName().Name, StringComparer.Ordinal))
               .SelectMany(a => a.GetExportedTypes())
               .Where(t => t.IsEnum)
               .ToList();
    }

    /// <summary>
    /// 非管理面枚举豁免：随功能收敛入 Core 程序集、但属于协议/中间件内部契约、
    /// 不应出现在 Admin 前端枚举列表的枚举（不带 [AdminEnum] = 不暴露，语义正确）。
    /// C8 T4a：签名/加密信封中间件共享层收敛带入的密钥环查找结果枚举。
    /// </summary>
    private static readonly HashSet<string> NonAdminEnums = new()
    {
        "GameFrameX.Core.Middleware.SessionKeyLookupResult",
        "GameFrameX.Core.Middleware.SignatureKeyLookupResult",
        // C263（GFX-669）：统一错误码体系枚举是协议层内部契约（服务端出站 + 前端码表由本地化 JSON 导出），
        // 不应出现在 Admin 前端业务枚举下拉列表；C262 T3 平移 Admin 域枚举时按同规则追加。
        "GameFrameX.Core.Enum.Errors.ErrorDomain",
        "GameFrameX.Core.Enum.Errors.SystemErrorCode",
        "GameFrameX.Core.Enum.Errors.CommonErrorCode",
        "GameFrameX.Core.Enum.Errors.AuthErrorCode",
        "GameFrameX.Core.Enum.Errors.ProtocolErrorCode",
        "GameFrameX.Core.Enum.Errors.AccountErrorCode",
        "GameFrameX.Core.Enum.Errors.GameRoleErrorCode",
        "GameFrameX.Core.Enum.Errors.AreaErrorCode",
        "GameFrameX.Core.Enum.Errors.ArchiveErrorCode",
        "GameFrameX.Core.Enum.Errors.GameHomeErrorCode",
        "GameFrameX.Core.Enum.Errors.OrderErrorCode",
        "GameFrameX.Core.Enum.Errors.RedeemCodeErrorCode",
        "GameFrameX.Core.Enum.Errors.PaymentErrorCode",
        // C265（GFX-671）：Admin 业务域错误码枚举同规则豁免（服务端出站 + 码表由本地化 JSON 导出，不进前端枚举下拉）。
        "GameFrameX.Core.Enum.Errors.SystemUserErrorCode",
        "GameFrameX.Core.Enum.Errors.RoleMenuErrorCode",
        "GameFrameX.Core.Enum.Errors.DictionaryErrorCode",
        "GameFrameX.Core.Enum.Errors.PlatformConfigErrorCode",
        "GameFrameX.Core.Enum.Errors.TenantErrorCode",
        "GameFrameX.Core.Enum.Errors.OrganizationErrorCode",
        "GameFrameX.Core.Enum.Errors.RegionErrorCode",
        "GameFrameX.Core.Enum.Errors.PositionErrorCode",
        "GameFrameX.Core.Enum.Errors.FileErrorCode",
        "GameFrameX.Core.Enum.Errors.JobErrorCode",
        "GameFrameX.Core.Enum.Errors.DatabaseErrorCode",
        "GameFrameX.Core.Enum.Errors.OpenAccessErrorCode",
        "GameFrameX.Core.Enum.Errors.MailCampaignErrorCode",
        "GameFrameX.Core.Enum.Errors.PrintErrorCode",
        "GameFrameX.Core.Enum.Errors.PluginErrorCode",
        // C339（GFX-750）：Online 八段镜像错误码同规则豁免（服务端出站 + 码表由本地化 JSON 导出，不进前端枚举下拉）。
        "GameFrameX.Core.Enum.Errors.OnlineErrorCode",
        // C350（GFX-754）：Online 社交治理 317 段业务错误码同规则豁免（错误码枚举不进前端枚举下拉）。
        "GameFrameX.Core.Enum.Errors.OnlineModerationErrorCode",
        // Core.Enum 程序集并入 GameFrameX.Entity 后，Entity 域内邮件营销 4 个内部枚举
        // 进入白名单程序集 public enum 全集；现状即不带 [AdminEnum]（不暴露给前端下拉），如实豁免。
        "GameFrameX.Entity.Game.Mail.MailCampaignMailType",
        "GameFrameX.Entity.Game.Mail.MailCampaignExpirePolicy",
        "GameFrameX.Entity.Game.Mail.MailCampaignPublishStatus",
        "GameFrameX.Entity.Game.Mail.MailCampaignServerCode",
    };

    /// <summary>
    /// 迁移等价性：历史白名单暴露的全部 public enum 都带 [AdminEnum] 标记，且无遗漏。
    /// 这保证 SystemEnumService.GetEnumTypeList() 改用 [AdminEnum] 过滤后，类型列表输出与历史逐字一致。
    /// </summary>
    [Fact]
    public void LegacyWhitelistEnums_AllMarkedWithAdminEnum()
    {
        var whitelistEnums = GetLegacyWhitelistPublicEnums();
        var unmarked = whitelistEnums
                       .Where(t => !NonAdminEnums.Contains(t.FullName!))
                       .Where(t => t.GetCustomAttribute<AdminEnumAttribute>() == null)
                       .Select(t => t.FullName)
                       .OrderBy(name => name, StringComparer.Ordinal)
                       .ToList();

        Assert.True(
            unmarked.Count == 0,
            "以下历史白名单 enum 缺少 [AdminEnum] 标记：" + string.Join(", ", unmarked));
    }

    /// <summary>
    /// SystemEnumService 过滤规则等价于 [AdminEnum] 判定：标记枚举通过，未标记枚举被过滤。
    /// </summary>
    [Fact]
    public void AdminEnumFilter_TaggedVisible_UntaggedFiltered()
    {
        bool IsAdminEnumVisible(Type t)
        {
            return t.IsEnum && t.GetCustomAttribute<AdminEnumAttribute>() != null;
        }

        Assert.True(IsAdminEnumVisible(typeof(OrderStatusEnum)));
        Assert.True(IsAdminEnumVisible(typeof(GameLanguage)));
        Assert.False(IsAdminEnumVisible(typeof(UnmarkedTestEnum)));
    }

    /// <summary>
    /// 前端依赖的 6 个枚举必须带 [AdminEnum] 标记，保证 enumDataList 按名读取仍可工作。
    /// </summary>
    [Theory]
    [InlineData(typeof(OrderStatusEnum))]
    [InlineData(typeof(GameMailType))]
    [InlineData(typeof(GameMailStatus))]
    [InlineData(typeof(GameUserStatus))]
    [InlineData(typeof(GameServerStatus))]
    [InlineData(typeof(GameLanguage))]
    public void FrontendConsumedEnums_AreMarkedAdminEnum(Type enumType)
    {
        Assert.NotNull(enumType.GetCustomAttribute<AdminEnumAttribute>());
    }

    /// <summary>
    /// 未标记的测试枚举，用于验证过滤逻辑排除未公开枚举。
    /// </summary>
    private enum UnmarkedTestEnum
    {
        A,
        B,
    }
}