namespace GameFrameX.Client.Api.GameUserSession.Dto;

/// <summary>
/// 登录请求（原 BLLogin LoginReq 18 字段；键名与客户端 GameApiConvert.LoginKeyMap 同源定稿）
/// </summary>
public class GameUserSessionLoginRequest
{
    /// <summary>
    /// 玩家 id（App 级信任链下用户身份由 body 携带）
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 客户端版本（语义版本，服务端版本比对用）
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 渠道号
    /// </summary>
    public long Channel { get; set; }

    /// <summary>
    /// 玩家等级
    /// </summary>
    public long UserLevel { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    public long Exp { get; set; }

    /// <summary>
    /// 任务进度
    /// </summary>
    public long UserProgress { get; set; }

    /// <summary>
    /// 国家/地区
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// 语言
    /// </summary>
    public string Lang { get; set; } = string.Empty;

    /// <summary>
    /// 归因渠道（AppsFlyer MediaSource）
    /// </summary>
    public string NetworkName { get; set; } = string.Empty;

    /// <summary>
    /// 归因 tracker 名
    /// </summary>
    public string TrackerName { get; set; } = string.Empty;

    /// <summary>
    /// 广告组
    /// </summary>
    public string AdGroup { get; set; } = string.Empty;

    /// <summary>
    /// 广告组 id
    /// </summary>
    public string AdGroupId { get; set; } = string.Empty;

    /// <summary>
    /// 广告 id（客户端恒带 "::" 前缀）
    /// </summary>
    public string AdId { get; set; } = string.Empty;

    /// <summary>
    /// 配置条目清单（P0-1 回显法上行：key→md5）
    /// </summary>
    public List<Models.GameUserSessionConfigEntry> ConfigEntries { get; set; } = new();

    /// <summary>
    /// 请求 id（时间戳；响应配对回显，幂等由 C4 信封 mid 承载）
    /// </summary>
    public long RequestId { get; set; }

    /// <summary>
    /// 奖励 key 清单（客户端恒空数组）
    /// </summary>
    public List<string> RewardKeys { get; set; } = new();

    /// <summary>
    /// 安装 uuid
    /// </summary>
    public string InstallId { get; set; } = string.Empty;

    /// <summary>
    /// 机型
    /// </summary>
    public string DeviceModel { get; set; } = string.Empty;
}