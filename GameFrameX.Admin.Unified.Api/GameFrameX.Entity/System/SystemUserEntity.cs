using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.System;

/// <summary>
/// 平台主库用户表，仅承载 SuperAdmin 等平台账号。
/// </summary>
[SugarTable(null, "平台主库用户表")]
[PlatformTable]
[IncrementTable]
public class SystemUserEntity : SystemUserBase
{
    /// <summary>
    /// 密码是否已完成初始化配置（严格二值）。<c>false</c>=首装 seed 未设环境变量 <c>GFX_SUPERADMIN_INITIAL_PASSWORD</c>（等待登录页向导设置）；
    /// <c>true</c>=环境变量注入初始密码、用户经 SetupSuperAdminPassword 完成初始化，或存量 NULL 行经启动期回填治理（值与 DefaultValue 同源）。
    /// </summary>
    [SugarColumn(ColumnDescription = "密码是否已初始化配置", DefaultValue = "true")]
    public bool PasswordConfigured { get; set; }
}
