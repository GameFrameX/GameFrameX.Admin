namespace GameFrameX.Application.Game.System.Email;

/// <summary>
/// 平台系统邮件 Provider 配置选择器。
/// <para>选择策略：<c>Purpose</c> 精确匹配 &gt; 默认（<c>Purpose = 0</c> / <see cref="GameFrameX.Core.Service.Message.SystemEmailPurpose.Default"/>）。</para>
/// <para>同匹配层按 <c>Priority</c> 降序，再按 <c>UpdateTime DESC, Id DESC</c>，保证选择结果确定。</para>
/// <para>本选择器不识别租户上下文；调用方负责传入正确的主库 <see cref="ISqlSugarClient"/>。</para>
/// </summary>
public static class SystemEmailProviderSelector
{
    /// <summary>
    /// 按平台系统邮件用途从主库挑选最佳 Provider 配置；未找到返回 null。
    /// </summary>
    /// <param name="db">主库 SqlSugar 客户端，由调用方注入。</param>
    /// <param name="purpose">平台系统邮件用途。传 0 视为通配，匹配默认配置。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static async Task<GameFrameX.Entity.System.SystemEmailProviderEntity?> SelectAsync(
        ISqlSugarClient db,
        int purpose,
        CancellationToken cancellationToken = default)
    {
        if (db == null)
        {
            throw new ArgumentNullException(nameof(db));
        }

        var exact = await db.Queryable<GameFrameX.Entity.System.SystemEmailProviderEntity>()
                            .Where(c => c.IsEnabled == true && c.Purpose == purpose)
                            .OrderByDescending(c => c.Priority)
                            .OrderByDescending(c => c.UpdateTime)
                            .OrderByDescending(c => c.Id)
                            .FirstAsync(cancellationToken);
        if (exact != null)
        {
            return exact;
        }

        if (purpose == 0)
        {
            return null;
        }

        return await db.Queryable<GameFrameX.Entity.System.SystemEmailProviderEntity>()
                       .Where(c => c.IsEnabled == true && c.Purpose == 0)
                       .OrderByDescending(c => c.Priority)
                       .OrderByDescending(c => c.UpdateTime)
                       .OrderByDescending(c => c.Id)
                       .FirstAsync(cancellationToken);
    }
}