using GameFrameX.Core.Const;
using GameFrameX.Entity.Base;
using SqlSugar;

namespace GameFrameX.Hub.Api.Services;

/// <summary>
/// Hub API 操作人审计 handler：经共享层 SqlSugarClientConfig.AddSqlSugarClientService 的 userAuditHandler
/// 挂点接入（ClaimConst 位于 GameFrameX.Core，共享层无法反向引用，故 Hub 差异留在 host 端参数化）。
/// <para>依据当前 context.User claims 填充 CreatedId / CreatedName（插入）与 UpdatedId / UpdatedName（更新），
/// 修 Hub.Api 既有 CreatedId 恒 0 缺陷；JWT 与 TenantGameAppOpenAccessEntity 机器凭证路径均受益。
/// 逻辑承接自原 Hub.Api SqlSugarClientConfig（三 host 仓储收敛 C259）。</para>
/// </summary>
public static class HubUserAuditHandler
{
    /// <summary>
    /// 创建绑定指定 HTTP 上下文访问器的操作人审计回调。
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器，读取当前 context.User claims。</param>
    /// <returns></returns>
    public static Action<DataFilterModel> Handle(IHttpContextAccessor httpContextAccessor)
    {
        return entityInfo =>
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                // CreatedId / CreatedName（对齐 Core SqlSugarSetup，修 Hub.Api 既有 CreatedId 恒 0 缺陷；JWT 与 TenantGameAppOpenAccessEntity 机器凭证路径均受益）
                if (user != null && entityInfo.PropertyName == nameof(EntityBasic.CreatedId))
                {
                    var createdId = ((dynamic)entityInfo.EntityValue).CreatedId;
                    if (createdId == 0 || createdId == null)
                    {
                        entityInfo.SetValue(user.FindFirst(ClaimConst.UserId)?.Value);
                    }
                }
                else if (user != null && entityInfo.PropertyName == nameof(EntityBasic.CreatedName))
                {
                    var createdName = ((dynamic)entityInfo.EntityValue).CreatedName;
                    if (string.IsNullOrEmpty(createdName))
                    {
                        entityInfo.SetValue(user.FindFirst(ClaimConst.RealName)?.Value ?? user.FindFirst(ClaimConst.Account)?.Value);
                    }
                }
            }

            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                if (user != null && entityInfo.PropertyName == nameof(EntityBasic.UpdatedId))
                {
                    entityInfo.SetValue(user.FindFirst(ClaimConst.UserId)?.Value);
                }
                else if (user != null && entityInfo.PropertyName == nameof(EntityBasic.UpdatedName))
                {
                    entityInfo.SetValue(user.FindFirst(ClaimConst.RealName)?.Value ?? user.FindFirst(ClaimConst.Account)?.Value);
                }
            }
        };
    }
}