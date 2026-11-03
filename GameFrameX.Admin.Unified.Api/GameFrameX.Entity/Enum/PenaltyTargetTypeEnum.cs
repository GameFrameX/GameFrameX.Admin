namespace GameFrameX.Entity.Enum;

/// <summary>
/// 处罚对象类型。处罚事实源（TenantGamePenaltyEntity）与举报事实源（TenantGameReportEntity）共用的对象分类。
/// </summary>
[Description("处罚对象类型")]
[AdminEnum]
public enum PenaltyTargetTypeEnum
{
    /// <summary>
    /// 游戏账号（TenantGameUserEntity.Id）。
    /// </summary>
    [Description("账号")] Account = 1,

    /// <summary>
    /// 游戏角色（TenantGameRoleEntity.Id）。
    /// </summary>
    [Description("角色")] Role = 2,

    /// <summary>
    /// 设备（TenantGameAppDeviceEntity 设备标识）。
    /// </summary>
    [Description("设备")] Device = 3,

    /// <summary>
    /// IP 地址。
    /// </summary>
    [Description("IP")] Ip = 4,
}