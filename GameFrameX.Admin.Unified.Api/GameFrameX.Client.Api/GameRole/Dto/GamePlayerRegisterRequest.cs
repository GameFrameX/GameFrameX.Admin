namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 注册输入对象
/// </summary>
public sealed class GamePlayerRegisterRequest
{
    /// <summary>
    /// 角色名
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 英雄发型
    /// </summary>
    public int HairstyleConfigId { get; set; }

    /// <summary>
    /// 选择的英雄
    /// </summary>
    public int HeroConfigId { get; set; }
}