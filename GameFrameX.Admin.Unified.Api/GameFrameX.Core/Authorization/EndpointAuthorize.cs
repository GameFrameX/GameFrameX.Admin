namespace GameFrameX.Core.Authorization;

/// <summary>
/// 端点鉴权纯函数（U6/C42）。把"端点绑定按钮 MenuId 集 ∩ 用户已授权 MenuId 集"抽为可单测的纯逻辑，
/// JwtHandler 请求期调用，避免把 DB/缓存依赖混进判定。
/// </summary>
public static class EndpointAuthorize
{
    /// <summary>
    /// 端点绑定的任一按钮 MenuId 命中用户已授权集即放行（OR 语义）。空绑定或空授权集拒绝。
    /// </summary>
    public static bool HasAnyBoundMenu(IEnumerable<long>? boundMenuIds, IEnumerable<long>? ownMenuIds)
    {
        if (boundMenuIds == null || ownMenuIds == null)
        {
            return false;
        }

        var owned = ownMenuIds is HashSet<long> ownSet
                        ? ownSet
                        : new HashSet<long>(ownMenuIds);
        if (owned.Count == 0)
        {
            return false;
        }

        foreach (var bound in boundMenuIds)
        {
            if (owned.Contains(bound))
            {
                return true;
            }
        }

        return false;
    }
}