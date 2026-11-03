using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 客户端包 Fallback 匹配器基类，提供 2 轮匹配（原始包名 + 母包包名）和 6 级 Fallback 策略
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
/// 具体实现类提供六级查询条件。
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
    /// 执行 Fallback 匹配（2 轮 + 6 级）
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
    /// 在主库顺序执行六级回退。
    /// </summary>
    protected virtual async Task<TEntity?> TryMatchWithFallbackAsync(TRequest request)
    {
        // 第1级: 完全匹配所有参数
        var result = await Repository.GetFirstAsync(BuildExactPredicates(request));
        // 第2级: 默认语言
        if (result == null)
        {
            result = await Repository.GetFirstAsync(BuildPredicatesWithDefaultLanguage(request));
        }

        // 第3级: 默认语言 + 默认渠道（或请求渠道）
        if (result == null)
        {
            result = await Repository.GetFirstAsync(BuildPredicatesWithDefaultChannel(request));
        }

        // 第4级: 默认版本
        if (result == null)
        {
            result = await Repository.GetFirstAsync(BuildPredicatesWithDefaultVersion(request));
        }

        // 第5级: 默认包名
        if (result == null)
        {
            result = await Repository.GetFirstAsync(BuildPredicatesWithDefaultPackage(request));
        }

        // 第6级: 默认平台
        if (result == null)
        {
            result = await Repository.GetFirstAsync(BuildPredicatesWithDefaultPlatform(request));
        }

        return result;
    }

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

}
