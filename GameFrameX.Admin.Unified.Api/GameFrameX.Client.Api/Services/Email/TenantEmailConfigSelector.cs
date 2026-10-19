using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件配置选择器。
/// <para>作用域优先级（从高到低）：<c>TenantId + AppId + Purpose</c> &gt; <c>TenantId + AppId</c> &gt; <c>TenantId + Purpose</c> &gt; <c>TenantId</c>。</para>
/// <para>同匹配层按 <c>Priority</c> 降序，再按 <c>UpdateTime DESC, Id DESC</c>。</para>
/// <para>本选择器仅作用于租户库；不读取平台主库，禁止跨邮件域 fallback。</para>
/// </summary>
public static class TenantEmailConfigSelector
{
    /// <summary>
    /// 按租户上下文 + 用途从租户库挑选最佳配置；未找到返回 null。
    /// </summary>
    /// <param name="db">租户库 SqlSugar 客户端，由调用方通过 <c>ClientApiTenantDbProvider</c> 获取。</param>
    /// <param name="context">当前请求的租户解析结果（<c>TenantId</c> + <c>AppId</c>）。</param>
    /// <param name="purpose">租户业务邮件用途。传 0 视为通配，匹配 <c>Purpose=0</c> 的配置。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static async Task<TenantEmailConfigEntity?> SelectAsync(
        ISqlSugarClient db,
        ClientApiTenantResolution context,
        int purpose,
        CancellationToken cancellationToken = default)
    {
        if (db == null)
        {
            throw new ArgumentNullException(nameof(db));
        }

        if (context == null || context.TenantId <= 0)
        {
            return null;
        }

        var appId = context.AppId.GetValueOrDefault();
        var scopes = BuildScopeCandidates(appId, purpose);

        foreach (var candidate in scopes)
        {
            var entity = await db.Queryable<TenantEmailConfigEntity>()
                                 .Where(c => c.TenantId == context.TenantId &&
                                             c.IsEnabled == true &&
                                             c.AppId == candidate.AppId &&
                                             c.Purpose == candidate.Purpose)
                                 .OrderByDescending(c => c.Priority)
                                 .OrderByDescending(c => c.UpdateTime)
                                 .OrderByDescending(c => c.Id)
                                 .FirstAsync(cancellationToken);
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// 按优先级倒序生成候选作用域列表。
    /// <para>候选顺序：<c>(AppId, Purpose)</c> → <c>(AppId, 0)</c> → <c>(0, Purpose)</c> → <c>(0, 0)</c>，由 <see cref="EmailScopeCandidate"/> 承载。</para>
    /// </summary>
    private static IEnumerable<EmailScopeCandidate> BuildScopeCandidates(long appId, int purpose)
    {
        if (appId > 0 && purpose > 0)
        {
            yield return new EmailScopeCandidate(appId, purpose);
            yield return new EmailScopeCandidate(appId, 0);
            yield return new EmailScopeCandidate(0, purpose);
            yield return new EmailScopeCandidate(0, 0);
        }
        else if (appId > 0)
        {
            yield return new EmailScopeCandidate(appId, 0);
            yield return new EmailScopeCandidate(0, 0);
        }
        else if (purpose > 0)
        {
            yield return new EmailScopeCandidate(0, purpose);
            yield return new EmailScopeCandidate(0, 0);
        }
        else
        {
            yield return new EmailScopeCandidate(0, 0);
        }
    }
}