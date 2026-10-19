namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 游戏角色增加输入参数
/// </summary>
public class AddTenantGameRoleEntityRequest : TenantGameRoleEntityBaseRequest
{
    /// <summary>
    /// 英雄发型配置id
    /// </summary>
    public int HairstyleConfigId { get; set; }

    /// <summary>
    /// 选择的英雄id
    /// </summary>
    public int HeroConfigId { get; set; }
}