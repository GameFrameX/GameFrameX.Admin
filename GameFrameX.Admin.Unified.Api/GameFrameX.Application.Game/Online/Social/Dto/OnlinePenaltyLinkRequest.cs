using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 处罚联动命令输入参数（高危端点，权限码 onlinePenaltyLinkCommand:execute 仅同步 TenantAdmin）。
/// <para>作用域从处罚事实源行推导（AppId 必有；AreaId 有值 = 单区服联动，null = App 下全部区服逐一联动）。</para>
/// </summary>
public class OnlinePenaltyLinkRequest
{
    /// <summary>
    /// 处罚事实源行 Id（必须处于生效中）。
    /// </summary>
    [Required(ErrorMessage = "处罚Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "处罚Id必须大于0")]
    public long PenaltyId { get; set; }

    /// <summary>
    /// 联动命令（Kick / Ban / Mute，三值硬白名单，X3 红线）。
    /// </summary>
    [Required(ErrorMessage = "联动命令不能为空")]
    public OnlinePenaltyCommandKind Command { get; set; }
}
