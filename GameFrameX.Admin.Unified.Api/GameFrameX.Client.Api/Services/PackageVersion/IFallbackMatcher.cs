namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// Fallback 匹配器接口，提供多级降级匹配策略
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IFallbackMatcher<TRequest, TEntity>
    where TEntity : EntityBase, new()
    where TRequest : class
{
    /// <summary>
    /// 执行 Fallback 匹配
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <returns>匹配的实体，如果未找到则返回 null</returns>
    Task<TEntity?> MatchAsync(TRequest request);
}