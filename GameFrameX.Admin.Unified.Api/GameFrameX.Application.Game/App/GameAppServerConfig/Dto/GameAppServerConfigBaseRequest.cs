namespace GameFrameX.Application.Game.App.GameAppServerConfig.Dto;

/// <summary>
/// 游戏运营配置管理公共输入字段
/// </summary>
public class GameAppServerConfigBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域（应用工作台上下文必填；租户默认基线行只能由种子产生）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 配置键（AppId + Key 唯一）
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 配置内容 JSON 原文（服务端保存时归一化重写）
    /// </summary>
    public string Config { get; set; } = null!;

    /// <summary>
    /// 内容版本号。服务端保存时强制重算，前端传入值一律被忽略；
    /// 暴露为可写属性仅为复用通用 CRUD 的请求到实体映射，非契约输入
    /// </summary>
    public string Md5 { get; set; } = null!;

    /// <summary>
    /// 键级启停：false 时登录 Config 不下发该键（客户端删除本地该键保持空态）
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }
}
