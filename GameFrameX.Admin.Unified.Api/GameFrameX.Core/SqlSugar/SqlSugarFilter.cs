using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.SqlSugar;

public static class SqlSugarFilter
{
    private static ICache? Cache;
    private static IHttpContextAccessor? HttpContextAccessor;
    private static IReadOnlyCollection<Type> EffectiveTypes = Array.Empty<Type>();

    /// <summary>
    /// 配置 SqlSugar 查询过滤器运行期依赖；过滤器回调不再通过 历史框架 App 静态门面取上下文。
    /// </summary>
    public static void Configure(ICache cache, IHttpContextAccessor httpContextAccessor, IEnumerable<Type> effectiveTypes)
    {
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        EffectiveTypes = effectiveTypes.ToArray();
    }

    /// <summary>
    /// 清除用户数据权限缓存
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dbConfigId"></param>
    public static void ClearUserDataScopeCache(long userId, string dbConfigId)
    {
        var cache = GetCache();
        var systemCacheService = HttpContextAccessor?.HttpContext?.RequestServices.GetService<SystemCacheService>();

        cache.Remove($"db:{dbConfigId}:dataScope:{userId}");
        // 删除最大数据权限缓存
        systemCacheService?.Remove($"{CacheConst.KeyRoleMaxDataScope}{userId}");
    }

    /// <summary>
    /// 配置用户仅本人数据过滤器
    /// </summary>
    public static int SetDataScopeFilter(SqlSugarScopeProvider db)
    {
        var maxDataScope = (int)DataScopeEnum.All;

        var userId = HttpContextAccessor?.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return maxDataScope;
        }

        // 获取用户最大数据范围。缓存未命中时不能用 int 默认值 0 覆盖 All，
        // 否则 All 权限用户会继续进入机构过滤器并触发递归初始化。
        var systemCacheService = HttpContextAccessor?.HttpContext?.RequestServices.GetService<SystemCacheService>();
        if (systemCacheService == null)
        {
            return maxDataScope;
        }

        var cachedDataScope = systemCacheService.Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
        if (cachedDataScope > 0)
        {
            maxDataScope = NormalizeDataScope(cachedDataScope);
        }
        else
        {
            var currentUserId = long.Parse(userId);
            var roleList = db.Queryable<UserRoleEntity>()
                             .LeftJoin<RoleEntity>((userRole, role) => userRole.RoleId == role.Id)
                             .Where((userRole, role) => userRole.UserId == currentUserId)
                             .Select((userRole, role) => role.DataScope)
                             .ToList();
            if (roleList.Count > 0)
            {
                maxDataScope = roleList.Min(u => NormalizeDataScope((int)u));
            }

            systemCacheService.Set(CacheConst.KeyRoleMaxDataScope + userId, maxDataScope);
        }

        // 组织机制退场后，部门及自定义范围收紧为本人；只有明确 All 才可读取全部。
        if (maxDataScope == (int)DataScopeEnum.All)
        {
            return maxDataScope;
        }

        // 配置用户数据范围缓存
        var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:dataScope:{userId}";
        var cache = GetCache();
        var dataScopeFilter = cache.Get<ConcurrentDictionary<Type, LambdaExpression>>(cacheKey);
        if (dataScopeFilter == null)
        {
            // 获取业务实体数据表
            var entityTypes = EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                                                        && u.IsSubclassOf(typeof(EntityBasicData)));
            if (!entityTypes.Any())
            {
                return maxDataScope;
            }

            dataScopeFilter = new ConcurrentDictionary<Type, LambdaExpression>();
            foreach (var entityType in entityTypes)
            {
                // 排除非当前数据库实体
                var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                if (tAtt != null && db.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString())
                {
                    continue;
                }

                var lambda = DynamicExpressionParser.ParseLambda(new[]
                                                                 {
                                                                     Expression.Parameter(entityType, "u"),
                                                                 }, typeof(bool), $"u.{nameof(EntityBasicData.CreatedId)}=@0", userId);
                db.QueryFilter.AddTableFilter(entityType, lambda);
                dataScopeFilter.TryAdd(entityType, lambda);
            }

            cache.Add(cacheKey, dataScopeFilter);
        }
        else
        {
            foreach (var filter in dataScopeFilter)
            {
                db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
            }
        }

        return maxDataScope;
    }

    internal static int NormalizeDataScope(int dataScope)
    {
        return dataScope == (int)DataScopeEnum.All ? (int)DataScopeEnum.All : (int)DataScopeEnum.Self;
    }

    private static ICache GetCache()
    {
        return Cache ?? throw new InvalidOperationException("SqlSugarFilter 尚未配置运行期缓存。");
    }
}