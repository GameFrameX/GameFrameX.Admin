using System.Security.Cryptography;
using System.Text;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Const.MenuSeed;

/// <summary>
/// 菜单、按钮和租户默认授权的 DB 缓存入口。
/// </summary>
public static class MenuSeedCatalog
{
    private static IReadOnlyList<GlobalMenuEntity>? _dbMenus;
    private static IReadOnlyList<MenuSeedDefinition>? _dbDefinitions;
    private static IReadOnlyDictionary<string, MenuSeedDefinition>? _dbDefinitionsByKey;

    /// <summary>
    /// 获取全部有效菜单和按钮。启动期必须先调用 <see cref="LoadFromDb"/>。
    /// </summary>
    public static IReadOnlyList<GlobalMenuEntity> Menus
    {
        get { return _dbMenus ?? throw NotLoaded(); }
    }

    /// <summary>
    /// 获取全部有效菜单定义。定义由 DB 的 global_menu 行重建，不再来自 C# 静态数组。
    /// </summary>
    public static IReadOnlyList<MenuSeedDefinition> AllDefinitions
    {
        get { return Definitions; }
    }

    private static IReadOnlyList<MenuSeedDefinition> Definitions
    {
        get { return _dbDefinitions ?? throw NotLoaded(); }
    }

    private static IReadOnlyDictionary<string, MenuSeedDefinition> DefinitionsByKey
    {
        get { return _dbDefinitionsByKey ?? throw NotLoaded(); }
    }

    /// <summary>
    /// 获取仅超级管理员可见的根菜单 ID。
    /// </summary>
    public static IReadOnlyList<long> SuperAdminOnlyRootMenuIds
    {
        get
        {
            return Definitions
                   .Where(u => u.ParentKey == null && u.SuperAdminOnly)
                   .Select(u => StableSeedId.Menu(u.Key))
                   .ToArray();
        }
    }

    /// <summary>
    /// 获取 SuperAdmin-only 树下允许租户调用的只读辅助按钮 ID。
    /// </summary>
    public static IReadOnlySet<long> TenantReadableSuperAdminOnlyMenuIds
    {
        get
        {
            return Definitions
                   .Where(u => u.SuperAdminOnly && u.AllowTenantWhenSuperAdminOnly)
                   .Select(u => StableSeedId.Menu(u.Key))
                   .ToHashSet();
        }
    }

    /// <summary>
    /// 获取 SuperAdmin 可见菜单 ID。
    /// <para>规则: SuperAdminOnly 菜单(含整棵子树) + 公共工作台(dashboard/home)。租户业务菜单不向 SuperAdmin 展示, 避免无目标租户上下文时仓储层抛 D1016。</para>
    /// </summary>
    public static IReadOnlyList<long> SuperAdminVisibleMenuIds
    {
        get
        {
            var superAdminOnlyKeys = Definitions
                                     .Where(u => u.SuperAdminOnly)
                                     .Select(u => u.Key)
                                     .ToHashSet(StringComparer.Ordinal);

            var visibleKeys = new HashSet<string>(StringComparer.Ordinal) { "route:dashboard", "route:home", };
            foreach (var definition in Definitions)
            {
                if (BelongsToSuperAdminOnlySubtree(definition.Key, superAdminOnlyKeys))
                {
                    visibleKeys.Add(definition.Key);
                }
            }

            return visibleKeys.Select(u => StableSeedId.Menu(u)).ToArray();
        }
    }

    /// <summary>
    /// 获取 SuperAdmin-only 整棵子树的菜单 ID（根 + 所有后代）。
    /// <para>供租户登录菜单树过滤：任何落入超管专属子树的菜单（含历史残留授权，如某菜单由租户可见改为超管专属后旧租户仍残留 role-menu 授权）都不应出现在租户导航中。</para>
    /// </summary>
    public static IReadOnlySet<long> SuperAdminOnlySubtreeMenuIds
    {
        get { return GetSuperAdminOnlySubtreeMenuIds(Definitions); }
    }

    /// <summary>
    /// 获取租户管理员默认菜单和按钮权限。
    /// </summary>
    public static IReadOnlyList<long> TenantAdminMenuIds
    {
        get { return GetMenuIdsForRoleCode(TenantDefaultDataTemplates.AdminRoleCode); }
    }

    /// <summary>
    /// 当前菜单目录版本号：definitions 引擎消费字段（Key / ParentKey / Type / SyncRoles / SuperAdminOnly /
    /// AllowTenantWhenSuperAdminOnly / AppScope）的确定性 SHA-256 哈希。
    /// <para>Title / Icon / OrderNo / 时间戳等展示字段不入哈希——防止 cosmetic 编辑触发全租户无差异重同步；
    /// SyncRoles / SuperAdminOnly 漂移必检。同内容多次计算结果不变（C353 状态表比对口径）。</para>
    /// </summary>
    public static string CatalogVersion
    {
        get { return ComputeCatalogVersion(Definitions); }
    }

    /// <summary>
    /// 计算给定菜单定义集合的目录版本号。纯函数，供单测注入自定义 definitions 验证字段口径。
    /// </summary>
    public static string ComputeCatalogVersion(IReadOnlyList<MenuSeedDefinition> definitions)
    {
        var lines = definitions
                    .Select(u => string.Join('|',
                        u.Key,
                        u.ParentKey ?? string.Empty,
                        ((int)u.Type).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ((int)u.SyncRoles).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        u.SuperAdminOnly ? "1" : "0",
                        u.AllowTenantWhenSuperAdminOnly ? "1" : "0",
                        ((int)u.AppScope).ToString(System.Globalization.CultureInfo.InvariantCulture)))
                    .OrderBy(u => u, StringComparer.Ordinal)
                    .ToArray();

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', lines))));
    }

    /// <summary>
    /// 获取 SuperAdmin-only 整棵子树的菜单 ID（根 + 所有后代）。纯函数重载，供闭包计算器 / 同步引擎注入 definitions 复用。
    /// </summary>
    public static IReadOnlySet<long> GetSuperAdminOnlySubtreeMenuIds(IReadOnlyList<MenuSeedDefinition> definitions)
    {
        var superAdminOnlyKeys = definitions
                                 .Where(u => u.SuperAdminOnly)
                                 .Select(u => u.Key)
                                 .ToHashSet(StringComparer.Ordinal);
        var definitionsByKey = definitions.ToDictionary(u => u.Key, u => u, StringComparer.Ordinal);
        return definitions
               .Where(u => BelongsToSuperAdminOnlySubtree(u.Key, superAdminOnlyKeys, definitionsByKey))
               .Select(u => StableSeedId.Menu(u.Key))
               .ToHashSet();
    }

    /// <summary>
    /// 按当前 SyncRoles 选出应同步给指定角色的菜单 ID 集合（含按钮父回退）。纯函数重载，供同步引擎注入 definitions 复用。
    /// </summary>
    public static IReadOnlyList<long> GetSyncedMenuIds(IReadOnlyList<MenuSeedDefinition> definitions, TenantMenuSyncRole role)
    {
        return SelectSyncedDefinitions(definitions, role)
               .Select(u => StableSeedId.Menu(u.Key))
               .Distinct()
               .ToArray();
    }

    /// <summary>
    /// 根据默认角色编码获取菜单 ID。
    /// </summary>
    public static IReadOnlyList<long> GetMenuIdsForRoleCode(string roleCode)
    {
        return SelectSyncedDefinitions(Definitions, ResolveRole(roleCode))
               .Select(u => StableSeedId.Menu(u.Key))
               .Distinct()
               .ToArray();
    }

    /// <summary>
    /// 根据默认角色编码生成角色菜单授权。
    /// </summary>
    public static IReadOnlyList<TenantRoleMenuEntity> BuildRoleMenus(long roleId, string roleCode)
    {
        return SelectSyncedDefinitions(Definitions, ResolveRole(roleCode))
               .Select(u => new TenantRoleMenuEntity
               {
                   Id = StableSeedId.RoleMenu(roleCode, u.Key),
                   RoleId = roleId,
                   MenuId = StableSeedId.Menu(u.Key),
               })
               .GroupBy(u => u.MenuId)
               .Select(u => u.First())
               .ToArray();
    }

    /// <summary>
    /// 计算种子菜单范围内、不应同步给指定角色的菜单 Id 集合（C189 role-menu 幂等收敛的删除候选）。
    /// <para>以当前 SyncRoles 为权威：期望集 = <see cref="SelectSyncedDefinitions"/>；surplus = 全部种子菜单 Id − 期望集。
    /// 仅限种子菜单范围（global_menu 定义），保护租户手动授权的非种子菜单不被收敛误删。
    /// 与 <see cref="BuildRoleMenus"/> 共用同一份期望集计算，保证"补入"与"删除"对同一 SyncRoles 收敛一致。</para>
    /// </summary>
    public static IReadOnlyList<long> GetSurplusSeedMenuIds(string roleCode)
    {
        return GetSurplusSeedMenuIds(Definitions, ResolveRole(roleCode));
    }

    /// <summary>
    /// 计算给定菜单定义集合中、不应同步给指定角色的菜单 Id 集合。纯函数重载，供单测注入自定义 SyncRoles 验证增/减/改。
    /// </summary>
    public static IReadOnlyList<long> GetSurplusSeedMenuIds(IReadOnlyList<MenuSeedDefinition> definitions, TenantMenuSyncRole role)
    {
        if (role == TenantMenuSyncRole.None)
        {
            return Array.Empty<long>();
        }

        var expectedMenuIds = SelectSyncedDefinitions(definitions, role)
                              .Select(u => StableSeedId.Menu(u.Key))
                              .ToHashSet();

        return definitions
               .Select(u => StableSeedId.Menu(u.Key))
               .Where(u => !expectedMenuIds.Contains(u))
               .ToList();
    }

    /// <summary>
    /// 按当前 SyncRoles 选出应同步给指定角色的菜单/按钮定义。
    /// <see cref="GetMenuIdsForRoleCode"/> / <see cref="BuildRoleMenus"/> / <see cref="GetSurplusSeedMenuIds"/> 的共享期望集计算，
    /// 确保"补入授权"与"删除多余"基于完全一致的期望集，幂等收敛。
    /// </summary>
    private static IReadOnlyList<MenuSeedDefinition> SelectSyncedDefinitions(IReadOnlyList<MenuSeedDefinition> definitions, TenantMenuSyncRole role)
    {
        if (role == TenantMenuSyncRole.None)
        {
            return Array.Empty<MenuSeedDefinition>();
        }

        var syncedMenuKeys = definitions
                             .Where(u => u.Type != MenuTypeEnum.Button && u.SyncRoles.HasFlag(role))
                             .Select(u => u.Key)
                             .ToHashSet(StringComparer.Ordinal);

        return definitions
               .Where(u => ShouldSyncToRole(u, role, syncedMenuKeys))
               .ToList();
    }

    /// <summary>
    /// 从主库 global_menu 加载有效菜单基线，并校验它能独立承担菜单/按钮 seed 源职责。
    /// </summary>
    public static void LoadFromDb(ISqlSugarClient db)
    {
        var dbMenus = db.Queryable<GlobalMenuEntity>()
                        .Where(u => u.IsDeleted == false)
                        .OrderBy(u => u.Pid)
                        .OrderBy(u => u.OrderNo)
                        .OrderBy(u => u.Id)
                        .ToList();

        ValidateDbMenus(dbMenus);

        var idToKey = dbMenus.ToDictionary(u => u.Id, u => u.Key!, EqualityComparer<long>.Default);
        var dbDefinitions = dbMenus.Select(dbMenu =>
        {
            string? parentKey = null;
            if (dbMenu.Pid != 0)
            {
                parentKey = idToKey[dbMenu.Pid];
            }

            return new MenuSeedDefinition(
                dbMenu.Key!,
                parentKey,
                dbMenu.Type,
                dbMenu.Name,
                dbMenu.Path,
                dbMenu.Component,
                dbMenu.Redirect,
                dbMenu.Permission,
                dbMenu.Title,
                dbMenu.TitleCode,
                dbMenu.Icon,
                dbMenu.OutLink,
                dbMenu.IsIframe,
                dbMenu.IsHide,
                dbMenu.IsKeepAlive,
                dbMenu.IsAffix,
                dbMenu.OrderNo,
                dbMenu.IsEnabled ?? true,
                dbMenu.Remark,
                dbMenu.CreatedTime,
                dbMenu.UpdateTime ?? dbMenu.CreatedTime,
                (TenantMenuSyncRole)dbMenu.SyncRoles,
                dbMenu.SuperAdminOnly,
                dbMenu.AllowTenantWhenSuperAdminOnly,
                dbMenu.AppScope);
        }).ToArray();

        _dbMenus = dbMenus;
        _dbDefinitions = dbDefinitions;
        _dbDefinitionsByKey = dbDefinitions.ToDictionary(u => u.Key, u => u, StringComparer.Ordinal);
    }

    private static void ValidateDbMenus(IReadOnlyList<GlobalMenuEntity> dbMenus)
    {
        if (dbMenus.Count == 0)
        {
            throw new InvalidOperationException("global_menu 菜单基线为空。请先执行受控菜单迁移，再启动 Admin Host。");
        }

        var missingKeyIds = dbMenus
                            .Where(u => string.IsNullOrWhiteSpace(u.Key))
                            .Select(u => u.Id)
                            .Take(10)
                            .ToArray();
        if (missingKeyIds.Length > 0)
        {
            throw new InvalidOperationException($"global_menu 存在缺少 Key 的有效菜单: {string.Join(", ", missingKeyIds)}。");
        }

        var duplicateKey = dbMenus.GroupBy(u => u.Key!, StringComparer.Ordinal)
                                  .FirstOrDefault(u => u.Count() > 1);
        if (duplicateKey != null)
        {
            // 列出冲突行 id/key，避免只给重复值无法定位两行来源（C331 / GFX-741）
            var keyConflicts = duplicateKey.Select(u => $"{u.Id}/{u.Key}").ToArray();
            throw new InvalidOperationException($"global_menu.Key 重复: {duplicateKey.Key}（冲突行: {string.Join(", ", keyConflicts)}）。");
        }

        var duplicatePermission = dbMenus.Where(u => !string.IsNullOrWhiteSpace(u.Permission))
                                         .GroupBy(u => u.Permission!, StringComparer.Ordinal)
                                         .FirstOrDefault(u => u.Count() > 1);
        if (duplicatePermission != null)
        {
            // 列出冲突行 id/key，避免只给重复值无法定位两行来源（C331 / GFX-741）
            var permissionConflicts = duplicatePermission.Select(u => $"{u.Id}/{u.Key}").ToArray();
            throw new InvalidOperationException($"global_menu.Permission 重复: {duplicatePermission.Key}（冲突行: {string.Join(", ", permissionConflicts)}）。");
        }

        var ids = dbMenus.Select(u => u.Id).ToHashSet();
        var missingParent = dbMenus.FirstOrDefault(u => u.Pid != 0 && !ids.Contains(u.Pid));
        if (missingParent != null)
        {
            throw new InvalidOperationException($"global_menu 父级不存在: Key={missingParent.Key}, Pid={missingParent.Pid}。");
        }

        var allowedRoles = global::System.Enum.GetValues<TenantMenuSyncRole>().Aggregate(TenantMenuSyncRole.None, (current, value) => current | value);
        var invalidRole = dbMenus.FirstOrDefault(u => ((TenantMenuSyncRole)u.SyncRoles & ~allowedRoles) != 0);
        if (invalidRole != null)
        {
            throw new InvalidOperationException($"global_menu.SyncRoles 包含未知位: Key={invalidRole.Key}, SyncRoles={invalidRole.SyncRoles}。");
        }

        var invalidAppScope = dbMenus.FirstOrDefault(u => !global::System.Enum.IsDefined(typeof(MenuAppScopeEnum), u.AppScope));
        if (invalidAppScope != null)
        {
            throw new InvalidOperationException($"global_menu.AppScope 非法值: Key={invalidAppScope.Key}, AppScope={invalidAppScope.AppScope}。");
        }

        EnsureNoCycles(dbMenus);
    }

    private static void EnsureNoCycles(IReadOnlyList<GlobalMenuEntity> dbMenus)
    {
        var byId = dbMenus.ToDictionary(u => u.Id);
        foreach (var menu in dbMenus)
        {
            var seen = new HashSet<long>();
            var current = menu;
            while (current.Pid != 0)
            {
                if (!seen.Add(current.Id))
                {
                    throw new InvalidOperationException($"global_menu 父子关系存在环: Key={menu.Key}。");
                }

                current = byId[current.Pid];
            }
        }
    }

    /// <summary>
    /// 沿父链向上查找, 若自身或任一祖先标记为 SuperAdminOnly, 则视为属于超管专属子树。
    /// </summary>
    private static bool BelongsToSuperAdminOnlySubtree(string key, IReadOnlySet<string> superAdminOnlyKeys)
    {
        return BelongsToSuperAdminOnlySubtree(key, superAdminOnlyKeys, DefinitionsByKey);
    }

    /// <summary>
    /// 纯函数变体：父链查找基于传入的定义集合（供纯重载在目录未加载时使用）。
    /// </summary>
    private static bool BelongsToSuperAdminOnlySubtree(string key, IReadOnlySet<string> superAdminOnlyKeys, IReadOnlyDictionary<string, MenuSeedDefinition> definitionsByKey)
    {
        var current = key;
        var guard = 0;
        while (!string.IsNullOrEmpty(current) && guard++ < 100)
        {
            if (superAdminOnlyKeys.Contains(current))
            {
                return true;
            }

            if (!definitionsByKey.TryGetValue(current, out var definition) || definition.ParentKey == null)
            {
                return false;
            }

            current = definition.ParentKey;
        }

        return false;
    }

    private static bool ShouldSyncToRole(MenuSeedDefinition definition, TenantMenuSyncRole role, IReadOnlySet<string> syncedMenuKeys)
    {
        if (definition.SyncRoles.HasFlag(role))
        {
            return true;
        }

        return role == TenantMenuSyncRole.TenantAdmin
               && definition.Type == MenuTypeEnum.Button
               && definition.ParentKey != null
               && !definition.SuperAdminOnly
               && syncedMenuKeys.Contains(definition.ParentKey);
    }

    private static TenantMenuSyncRole ResolveRole(string roleCode)
    {
        return TenantDefaultDataTemplates.ResolveMenuSyncRole(roleCode);
    }

    private static InvalidOperationException NotLoaded()
    {
        return new InvalidOperationException("MenuSeedCatalog 尚未从 global_menu 加载。请在主库初始化后调用 MenuSeedCatalog.LoadFromDb。");
    }
}