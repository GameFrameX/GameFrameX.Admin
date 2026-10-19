namespace GameFrameX.Entity.Enum;

/// <summary>
/// 反作弊事件指向对象类型（反作弊事件账本 TenantGameAntiCheatEventEntity.TargetType）。
/// <para>严格锁定 4 值：值与 C11-C（C115）计划的 PenaltyTargetTypeEnum 对齐（Account=1/Role=2/Device=3/Ip=4），
/// 便于 C11-D-followup 在复核通过后将反作弊建议映射为同维度处罚，避免语义错位。</para>
/// </summary>
[Description("反作弊指向对象")]
[AdminEnum]
public enum AntiCheatTargetTypeEnum
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
    /// 设备标识（TenantGameAppDeviceEntity）。
    /// </summary>
    [Description("设备")] Device = 3,

    /// <summary>
    /// IP 地址。
    /// </summary>
    [Description("IP")] Ip = 4,
}