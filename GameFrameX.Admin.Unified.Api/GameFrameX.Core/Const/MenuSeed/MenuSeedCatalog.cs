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
            var protectedKeys = Definitions.Where(definition => definition.SuperAdminOnly)
                                           .Select(definition => definition.Key).ToHashSet(StringComparer.Ordinal);
            // 超管子树可以挂在公共系统目录下，保护根不等于导航顶层根。
            return Definitions
                   .Where(definition => definition.SuperAdminOnly
                                        && (definition.ParentKey == null
                                            || !BelongsToSuperAdminOnlySubtree(definition.ParentKey, protectedKeys)))
                   .Select(definition => StableSeedId.Menu(definition.Key))
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
    /// <para>规则: SuperAdminOnly 菜单(含整棵子树)及其目录祖先 + 公共总览与运营首页(dashboard/home)。只补目录祖先，不引入祖先下的租户业务菜单。</para>
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
                    var parentKey = definition.ParentKey;
                    while (parentKey != null && DefinitionsByKey.TryGetValue(parentKey, out var parent))
                    {
                        if (parent.Type == MenuTypeEnum.Dir)
                        {
                            visibleKeys.Add(parent.Key);
                        }

                        parentKey = parent.ParentKey;
                    }
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

    private static InvalidOperationException NotLoaded()
    {
        return new InvalidOperationException("MenuSeedCatalog 尚未从 global_menu 加载。请在主库初始化后调用 MenuSeedCatalog.LoadFromDb。");
    }
}
