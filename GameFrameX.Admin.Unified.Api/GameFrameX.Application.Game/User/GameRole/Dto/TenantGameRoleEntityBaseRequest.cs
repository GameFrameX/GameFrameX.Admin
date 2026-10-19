namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 游戏角色基础输入参数
/// </summary>
public class TenantGameRoleEntityBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public virtual long AppId { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    public virtual long AreaId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public virtual string Description { get; set; } = null!;

    /// <summary>
    /// 角色当前的等级
    /// </summary>
    public virtual long? Level { get; set; }

    /// <summary>
    /// 角色最后一次登录时间
    /// </summary>
    public virtual long? LastLoginTime { get; set; }
}