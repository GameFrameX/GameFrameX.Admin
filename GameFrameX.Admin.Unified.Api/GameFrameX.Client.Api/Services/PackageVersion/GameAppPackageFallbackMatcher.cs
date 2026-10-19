using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 客户端包 Fallback 匹配器基类，提供 2 轮匹配（原始包名 + 母包包名）和 6 级 Fallback 策略，并支持应用作用域（AppId）优先级
/// </summary>
/// <typeparam name="TRequest">请求类型，必须实现 IPackageRequest</typeparam>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <remarks>
/// 此基类封装了客户端包匹配的通用逻辑：
/// - 第1轮：使用原始包名进行 6 级 Fallback 匹配
/// - 第2轮：解析母包包名并进行 6 级 Fallback 匹配
///
/// Fallback 策略（从高到低优先级）：
/// 1. 完全匹配所有参数
/// 2. 默认语言
/// 3. 默认语言 + 默认渠道（或请求渠道）
/// 4. 默认版本
/// 5. 默认包名
/// 6. 默认平台
///
/// 应用作用域优先级：请求携带正值 AppId 时，先以 App 精确作用域（AppId == request.AppId）跑完整 6 级；
/// 未命中再以租户默认作用域（AppId 为 null 或 0）跑完整 6 级。request.AppId 为 null/0 时只跑租户默认作用域。
///
/// 具体实现类需要提供构建查询条件的逻辑，以及应用作用域过滤条件。
/// </remarks>
public abstract class GameAppPackageFallbackMatcher<TRequest, TEntity> : IFallbackMatcher<TRequest, TEntity>
    where TEntity : EntityBase, new()
    where TRequest : class, IPackageRequest
{
    /// <summary>
    /// 数据仓储
    /// </summary>
    protected Repository<TEntity> Repository { get; }

    /// <summary>
    /// 客户端包解析器
    /// </summary>
    protected GameAppPackageResolver Resolver { get; }

    /// <summary>
    /// 初始化 Fallback 匹配器
    /// </summary>
    /// <param name="repository">数据仓储</param>
    /// <param name="resolver">客户端包解析器</param>
    protected GameAppPackageFallbackMatcher(Repository<TEntity> repository, GameAppPackageResolver resolver)
    {
        Repository = repository;
        Resolver = resolver;
    }

    /// <summary>
    /// 执行 Fallback 匹配（2 轮 + 6 级，并叠加应用作用域优先级）
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>匹配的实体，如果未找到则返回 null</returns>
    public async Task<TEntity?> MatchAsync(TRequest request)
    {
        // 第1轮：原始包名匹配
        var result = await TryMatchWithFallbackAsync(request);

        // 第2轮：母包包名匹配
        if (result == null)
        {
            var masterName = Resolver.Resolve(request.PackageName);
            if (!string.IsNullOrEmpty(masterName) && masterName != request.PackageName)
            {
                // 创建新的请求对象，使用母包包名
                var masterRequest = CloneRequestWithPackage(request, masterName);
                result = await TryMatchWithFallbackAsync(masterRequest);
            }
        }

        return result;
    }

    /// <summary>
    /// 执行 6 级 Fallback 匹配，并按应用作用域优先级（App 精确 → 租户默认）跑两遍
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>匹配的实体，如果未找到则返回 null</returns>
    protected virtual async Task<TEntity?> TryMatchWithFallbackAsync(TRequest request)
    {
        // App 精确作用域：仅在请求携带正值 AppId 时尝试
        if (request.AppId.HasValue && request.AppId.Value > 0)
        {
            var appResult = await RunSixLevelsAsync(request, request.AppId);
            if (appResult != null)
            {
                return appResult;
            }
        }

        // 租户默认作用域：AppId 为 null 或 0
        return await RunSixLevelsAsync(request, null);
    }

    /// <summary>
    /// 在指定应用作用域下顺序执行 6 级 Fallback
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="appIdFilter">应用作用域过滤；正值表示 App 精确，null 表示租户默认</param>
    /// <returns>匹配的实体，如果未找到则返回 null</returns>
    private async Task<TEntity?> RunSixLevelsAsync(TRequest request, long? appIdFilter)
    {
        var appScope = BuildAppScopePredicate(appIdFilter);

        // 第1级: 完全匹配所有参数
        var result = await Repository.GetFirstAsync(Combine(BuildExactPredicates(request), appScope));
        // 第2级: 默认语言
        if (result == null)
        {
            result = await Repository.GetFirstAsync(Combine(BuildPredicatesWithDefaultLanguage(request), appScope));
        }

        // 第3级: 默认语言 + 默认渠道（或请求渠道）
        if (result == null)
        {
            result = await Repository.GetFirstAsync(Combine(BuildPredicatesWithDefaultChannel(request), appScope));
        }

        // 第4级: 默认版本
        if (result == null)
        {
            result = await Repository.GetFirstAsync(Combine(BuildPredicatesWithDefaultVersion(request), appScope));
        }

        // 第5级: 默认包名
        if (result == null)
        {
            result = await Repository.GetFirstAsync(Combine(BuildPredicatesWithDefaultPackage(request), appScope));
        }

        // 第6级: 默认平台
        if (result == null)
        {
            result = await Repository.GetFirstAsync(Combine(BuildPredicatesWithDefaultPlatform(request), appScope));
        }

        return result;
    }

    /// <summary>
    /// 构建应用作用域过滤条件。appIdFilter 为正值时要求 AppId 精确匹配；为 null 时匹配租户默认（AppId 为 null 或 0）
    /// </summary>
    /// <param name="appIdFilter">应用作用域过滤值</param>
    /// <returns>应用作用域查询条件</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildAppScopePredicate(long? appIdFilter);

    /// <summary>
    /// 构建完全匹配所有参数的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildExactPredicates(TRequest request);

    /// <summary>
    /// 构建使用默认语言的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildPredicatesWithDefaultLanguage(TRequest request);

    /// <summary>
    /// 构建使用默认渠道的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildPredicatesWithDefaultChannel(TRequest request);

    /// <summary>
    /// 构建使用默认版本的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildPredicatesWithDefaultVersion(TRequest request);

    /// <summary>
    /// 构建使用默认包名的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildPredicatesWithDefaultPackage(TRequest request);

    /// <summary>
    /// 构建使用默认平台的查询条件
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>查询条件表达式</returns>
    protected abstract Expression<Func<TEntity, bool>> BuildPredicatesWithDefaultPlatform(TRequest request);

    /// <summary>
    /// 克隆请求并修改包名
    /// </summary>
    /// <param name="request">原始请求</param>
    /// <param name="newPackageName">新的包名</param>
    /// <returns>克隆的新请求对象</returns>
    protected abstract TRequest CloneRequestWithPackage(TRequest request, string newPackageName);

    /// <summary>
    /// 将两个谓词用逻辑 AND 合并为一个谓词（对齐参数表达式）
    /// </summary>
    /// <param name="left">左侧谓词</param>
    /// <param name="right">右侧谓词</param>
    /// <returns>合并后的谓词</returns>
    protected static Expression<Func<TEntity, bool>> Combine(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right)
    {
        var parameter = left.Parameters[0];
        var visitor = new ParameterReplaceVisitor(right.Parameters[0], parameter);
        var body = Expression.AndAlso(left.Body, visitor.Visit(right.Body));
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    /// <summary>
    /// 参数替换访问器，用于把右侧谓词的参数替换为左侧谓词的参数
    /// </summary>
    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        /// <summary>
        /// 初始化参数替换访问器
        /// </summary>
        /// <param name="oldParameter">被替换的参数</param>
        /// <param name="newParameter">替换后的参数</param>
        public ParameterReplaceVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        /// <summary>
        /// 访问参数表达式，命中被替换参数时返回新参数
        /// </summary>
        /// <param name="node">参数表达式</param>
        /// <returns>替换后的参数表达式</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : node;
        }
    }
}