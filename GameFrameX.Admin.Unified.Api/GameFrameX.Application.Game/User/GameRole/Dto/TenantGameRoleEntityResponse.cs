namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 游戏角色输出参数
/// </summary>
public class TenantGameRoleEntityResponse
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 角色当前的等级
    /// </summary>
    public long? Level { get; set; }

    /// <summary>
    /// 是否是沙盒账户
    /// </summary>
    public bool? IsSandbox { get; set; }

    /// <summary>
    /// 角色最后一次登录时间
    /// </summary>
    public long? LastLoginTime { get; set; }

    /// <summary>
    /// 角色注册时间
    /// </summary>
    public long? CreateTime { get; set; }

    /// <summary>
    /// 在先时长
    /// </summary>
    public long OnlineTime { get; set; }

    /// <summary>
    /// 当前关卡 
    /// </summary>
    /// <returns></returns>
    public int CurrentLevelConfigId { get; set; }

    /// <summary>
    /// 累充
    /// </summary>
    /// <returns></returns>
    public int AllRecharge { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 行版本（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}