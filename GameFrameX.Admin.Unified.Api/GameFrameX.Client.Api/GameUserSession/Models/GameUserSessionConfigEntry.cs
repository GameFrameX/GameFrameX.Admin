namespace GameFrameX.Client.Api.GameUserSession.Models;

/// <summary>
/// 登录上行配置条目（原 BLLogin LoginReq.config_entries 数组元素；md5 清单形态）
/// </summary>
public class GameUserSessionConfigEntry
{
    /// <summary>
    /// 配置键（ServerConfigKey/ActivityType/BuffType 枚举串）
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// 客户端当前该键的 md5（空 = 客户端本地无此配置）
    /// </summary>
    public string ConfigMd5 { get; set; } = string.Empty;
}