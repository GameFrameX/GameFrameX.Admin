using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Const;

/// <summary>
/// 租户基础数据默认模板。
/// </summary>
public static class TenantDefaultDataTemplates
{
    public const long MainTenantId = 1300000000001;

    private const string EnterprisePackageCode = "enterprise";

    private const string DefaultAdminPhoneValue = "13884998275";

    private const string DefaultAdminEmailValue = "admin@alianblank.com";

    private const string RootOrganizationKeyValue = "publisher";

    private const string AdminRoleCodeValue = CommonConst.SysAdminRole;

    private const string AdminPositionCodeValue = "general_manager";

    private static readonly Lazy<IReadOnlyList<OrganizationTemplate>> OrganizationTemplateCache =
        new(() => LoadOrganizationTemplates().Select(template => new OrganizationTemplate(
                                                StableSeedId.TenantOrganizationEntity(template.Key),
                                                template.Key,
                                                template.ParentKey,
                                                template.Name,
                                                template.Code,
                                                template.Type,
                                                template.Level,
                                                template.OrderNo,
                                                template.Remark)).ToArray());

    private static readonly Lazy<IReadOnlyList<RoleTemplate>> RoleTemplateCache =
        new(() => LoadRoleTemplates().Select(template => new RoleTemplate(
                                                 StableSeedId.TenantRoleEntity(template.Code),
                                                 template.Name,
                                                 template.DataScope,
                                                 template.Code,
                                                 template.SyncRole,
                                                 template.OrderNo,
                                                 template.Remark)).ToArray());

    private static readonly Lazy<IReadOnlyDictionary<string, TenantMenuSyncRole>> RoleSyncRoleCache = new(() => RoleTemplates.ToDictionary(template => template.Code, template => template.SyncRole, StringComparer.Ordinal));

    private static readonly Lazy<IReadOnlyList<PositionTemplate>> PositionTemplateCache =
        new(() => LoadPositionTemplates().Select(template => new PositionTemplate(
                                                     StableSeedId.TenantPosition(template.Code),
                                                     template.Name,
                                                     template.Code,
                                                     template.OrderNo,
                                                     template.Remark)).ToArray());

    public static long EnterprisePackageId
    {
        get { return StableSeedId.TenantPackage(EnterprisePackageCode); }
    }

    public static string DefaultAdminPhone
    {
        get { return DefaultAdminPhoneValue; }
    }

    public static string DefaultAdminEmail
    {
        get { return DefaultAdminEmailValue; }
    }

    public static string RootOrganizationKey
    {
        get { return RootOrganizationKeyValue; }
    }

    public static string AdminRoleCode
    {
        get { return AdminRoleCodeValue; }
    }

    public static string AdminPositionCode
    {
        get { return AdminPositionCodeValue; }
    }

    public sealed record OrganizationTemplate(long SeedId, string Key, string? ParentKey, string Name, string Code, string Type, int Level, int OrderNo, string Remark);

    public sealed record RoleTemplate(long SeedId, string Name, DataScopeEnum DataScope, string Code, TenantMenuSyncRole SyncRole, int OrderNo, string Remark);

    public sealed record PositionTemplate(long SeedId, string Name, string Code, int OrderNo, string Remark);

    public static IReadOnlyList<OrganizationTemplate> OrganizationTemplates
    {
        get { return OrganizationTemplateCache.Value; }
    }

    public static IReadOnlyList<RoleTemplate> RoleTemplates
    {
        get { return RoleTemplateCache.Value; }
    }

    public static IReadOnlyList<PositionTemplate> PositionTemplates
    {
        get { return PositionTemplateCache.Value; }
    }

    public static TenantMenuSyncRole ResolveMenuSyncRole(string roleCode)
    {
        return RoleSyncRoleCache.Value.GetValueOrDefault(roleCode, TenantMenuSyncRole.None);
    }

    private static IReadOnlyList<TenantOrganizationTemplateAttribute> LoadOrganizationTemplates()
    {
        var templates = LoadAttributes<TenantOrganizationTemplateAttribute>().ToArray();
        return templates.Length == 0 ? throw new InvalidOperationException("默认租户组织模板为空。") : templates;
    }

    private static IReadOnlyList<TenantRoleTemplateAttribute> LoadRoleTemplates()
    {
        var templates = LoadAttributes<TenantRoleTemplateAttribute>().ToArray();
        if (templates.Length == 0)
        {
            throw new InvalidOperationException("默认租户角色模板为空。");
        }

        if (!templates.Any(template => string.Equals(template.Code, CommonConst.SysAdminRole, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("租户默认管理员角色必须使用系统管理员角色编码。");
        }

        return templates;
    }

    private static IReadOnlyList<TenantPositionTemplateAttribute> LoadPositionTemplates()
    {
        var templates = LoadAttributes<TenantPositionTemplateAttribute>().ToArray();
        return templates.Length == 0 ? throw new InvalidOperationException("默认租户职位模板为空。") : templates;
    }

    private static IEnumerable<TAttribute> LoadAttributes<TAttribute>() where TAttribute : Attribute
    {
        return typeof(TenantDefaultDataTemplates).Assembly
                                                 .GetTypes()
                                                 .SelectMany(type => type.GetCustomAttributes(typeof(TAttribute), false))
                                                 .Cast<TAttribute>();
    }
}