namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 基于委托的 SqlSugar 租户 ID 解析器。
/// </summary>
public sealed class DelegateSqlSugarTenantIdResolver : ISqlSugarTenantIdResolver
{
    private readonly Func<long?> _resolver;

    /// <summary>
    /// 初始化基于委托的租户 ID 解析器。
    /// </summary>
    /// <param name="resolver">租户 ID 获取委托，null 时始终返回 null。</param>
    public DelegateSqlSugarTenantIdResolver(Func<long?> resolver)
    {
        _resolver = resolver ?? (() => null);
    }

    /// <inheritdoc />
    public long? GetTenantId()
    {
        return _resolver();
    }
}