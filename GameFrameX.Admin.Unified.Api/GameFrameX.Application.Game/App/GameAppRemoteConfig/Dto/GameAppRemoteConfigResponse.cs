using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Application.Game.App.GameAppRemoteConfig.Dto;

/// <summary>
/// 远程配置管理输出参数
/// </summary>
public class GameAppRemoteConfigResponse : IBasePageResponse<TenantGameAppRemoteConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// App版本
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 检测资源更新地址
    /// </summary>
    public string CheckResourceUrl { get; set; } = null!;

    /// <summary>
    /// 检测APP更新地址
    /// </summary>
    public string CheckAppVersionUrl { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Aot 代码列表
    /// </summary>
    public string AotCodeList { get; set; } = null!;

    /// <summary>
    /// 模式内容映射 JSON (key=ModeCode, value=content object)
    /// </summary>
    public string? ModeContents { get; set; }

    /// <summary>
    /// 当前激活模式编码
    /// </summary>
    public string? ActiveModeCode { get; set; }

    /// <summary>
    /// 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
    /// </summary>
    public int RolloutPercent { get; set; } = 100;

    /// <summary>
    /// 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。
    /// </summary>
    public string? DeviceWhitelistJson { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}