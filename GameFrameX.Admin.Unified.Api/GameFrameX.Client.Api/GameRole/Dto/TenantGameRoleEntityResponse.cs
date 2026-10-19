namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 游戏角色输出参数
/// </summary>
public sealed class TenantGameRoleEntityResponse : EntityBaseId
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 区服名字
    /// </summary>
    public string ServerName { get; set; } = null!;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色头像
    /// </summary>
    public uint Avatar { get; set; }

    /// <summary>
    /// 角色当前的等级
    /// </summary>
    public long Level { get; set; }

    /// <summary>
    /// 角色最后一次登录时间
    /// </summary>
    public long? LastLoginTime { get; set; }

    /// <summary>
    /// 英雄发型配置id
    /// </summary>
    public int HairstyleConfigId { get; set; }

    /// <summary>
    /// 选择的英雄id
    /// </summary>
    public int HeroConfigId { get; set; }
}