namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 当前租户 ID 解析器。
/// </summary>
public interface ISqlSugarTenantIdResolver
{
    /// <summary>
    /// 获取当前租户 ID。
    /// </summary>
    long? GetTenantId();
}