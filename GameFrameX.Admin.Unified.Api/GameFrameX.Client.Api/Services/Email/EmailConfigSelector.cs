using GameFrameX.Entity.Business.ClientApiConfig;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 从主库选择业务邮件配置，用途精确匹配优先于默认用途。
/// 同用途按优先级、更新时间及 Id 降序选择，不跨邮件域回退。
/// </summary>
public static class EmailConfigSelector
{
    public static async Task<EmailConfigEntity?> SelectAsync(
        ISqlSugarClient database,
        int purpose,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        var purposes = purpose > 0 ? new[] { purpose, 0 } : new[] { 0 };
        foreach (var candidate in purposes)
        {
            var entity = await database.Queryable<EmailConfigEntity>()
                .Where(c => c.IsEnabled == true && c.Purpose == candidate)
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
}
