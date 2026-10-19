namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户请求读写动作分类器。
/// <para>用于「过期租户禁止写操作、允许读操作」的门禁裁决：action 名命中只读前缀集合判为读并放行，否则判为写并按过期规则拦截。</para>
/// <para>策略偏向安全侧——未知 action 默认判为写，漏判只会多挡读、不会漏挡写。</para>
/// <para>刻意用方法名前缀而非 HTTP 方法：项目内分页等查询走 POST（如 apiSystemTenantPagePost），用 HTTP 方法会把过期租户的读误判为写。</para>
/// </summary>
public static class TenantReadActionClassifier
{
    /// <summary>
    /// 只读动作方法名前缀集合（命中即判为读，放行过期租户）。扩充需同步更新 TenantReadActionClassifierTests。
    /// <para>刻意排除 <c>Is</c> / <c>Has</c>：会误伤 <c>Issue</c>（发放）/ <c>Hash</c> 等写操作。</para>
    /// </summary>
    public static readonly string[] ReadPrefixes =
    {
        "Get",
        "Page",
        "List",
        "Query",
        "Find",
        "Fetch",
        "Read",
        "View",
        "Info",
        "Detail",
        "Tree",
        "Select",
        "Option",
        "Options",
        "Check",
        "Verify",
        "Export",
        "Count",
        "Exist",
    };

    /// <summary>
    /// 判断 action 名是否为只读动作。null/空判为写（安全侧）。
    /// </summary>
    /// <param name="actionName">MVC action 方法名（优先 <c>[ApiDescriptionSettings(Name = ...)]</c>，否则取 <c>MethodInfo.Name</c>）。</param>
    /// <returns>命中只读前缀返回 true；否则 false。</returns>
    public static bool IsReadAction(string? actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        foreach (var prefix in ReadPrefixes)
        {
            if (actionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}