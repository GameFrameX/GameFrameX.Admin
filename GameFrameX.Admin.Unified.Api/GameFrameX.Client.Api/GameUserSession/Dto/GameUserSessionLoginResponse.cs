using GameFrameX.Client.Api.GameUserReward.Models;
namespace GameFrameX.Client.Api.GameUserSession.Dto;

/// <summary>
/// 登录响应（原 BLLogin LoginResp 28 字段；键名与客户端 GameApiConvert.LoginRespKeyMap 同源定稿，
/// 四态去向见 C13 change.md §1：新建下发 / 复用 C8 存档行 / 已在别处 / 空缺省）
/// </summary>
public class GameUserSessionLoginResponse
{
    /// <summary>
    /// 结果码（0 正常 / 1 版本过低 / 2 账号注销）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// CDN 清单句柄（catalogue 管理面留 P1，空缺省安全）
    /// </summary>
    public string Catalogue { get; set; } = string.Empty;

    /// <summary>
    /// CDN 清单 md5（空缺省安全）
    /// </summary>
    public string CatalogueMd5 { get; set; } = string.Empty;

    /// <summary>
    /// 强制下载标记（0 = 不强制）
    /// </summary>
    public long ForceDownload { get; set; }

    /// <summary>
    /// 服务端配置 JSON（C280 差异下发：上行 md5 相等回 {"key":{"md5"}}；不等/缺失回
    /// {"key":{"md5","config"}}；种子无内容的键不回显。种子源见 GameServerConfigSeed）
    /// </summary>
    public string Config { get; set; } = string.Empty;

    /// <summary>
    /// 存档最后同步时间（复用 C8 存档行 SyncTime；无档 = 0）
    /// </summary>
    public long LastSyncTime { get; set; }

    /// <summary>
    /// 服务器时间（毫秒时间戳，客户端 //1000 后 RefreshServerTime）
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 更新提示类型（C6 GetInfo 预填优先，此处空缺省 0）
    /// </summary>
    public long UpdateHint { get; set; }

    /// <summary>
    /// 更新提示强提醒结束时间（毫秒时间戳，空缺省 0）
    /// </summary>
    public long UpdateHintEndEnterTime { get; set; }

    /// <summary>
    /// 商店地址（空缺省安全）
    /// </summary>
    public string StoreUrl { get; set; } = string.Empty;

    /// <summary>
    /// 奖励下发（空缺省空数组；元素 Content 为内嵌 JSON，客户端二次 decode）
    /// </summary>
    public List<GameUserRewardInfo> Rewards { get; set; } = new();

    /// <summary>
    /// 社交绑定（空缺省空数组；SocialPackage 五字段）
    /// </summary>
    public List<Models.GameUserSessionSocialBinding> SocialBindings { get; set; } = new();

    /// <summary>
    /// 累计充值（分；C11 订单已到账汇总）
    /// </summary>
    public long TotalPay { get; set; }

    /// <summary>
    /// 当月充值（分；管理面留 P1，空缺省 0）
    /// </summary>
    public long MonthPay { get; set; }

    /// <summary>
    /// 请求 id 回显（客户端不读，协议对齐保留）
    /// </summary>
    public long RequestId { get; set; }

    /// <summary>
    /// AB 分组名（AB 管理面留 P1，空缺省 ""）
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 有效分组名清单（空缺省空数组）
    /// </summary>
    public List<string> ValidGroupName { get; set; } = new();

    /// <summary>
    /// 强制同步（0 不强制 / 1 强制下行 / 2 作弊标记；自建起步恒 0）
    /// </summary>
    public long ForceSync { get; set; }

    /// <summary>
    /// 强制 SSO（0 缺省不触发注册流）
    /// </summary>
    public long ForceSso { get; set; }

    /// <summary>
    /// 存档行安装 uuid（换端判定；无档空串）
    /// </summary>
    public string InstallationId { get; set; } = string.Empty;

    /// <summary>
    /// 存档行机型（换端判定；无档空串）
    /// </summary>
    public string UpdateDeviceModel { get; set; } = string.Empty;

    /// <summary>
    /// 当前会话 id（P0-2 存档行承载；本次登录新生成）
    /// </summary>
    public long Session { get; set; }

    /// <summary>
    /// 注销时间（毫秒时间戳；rcode=2 时消费，注销时间管理面留 P1 起步 0）
    /// </summary>
    public long LogoutTime { get; set; }

    /// <summary>
    /// 心跳间隔秒（默认 60；管理面留 P1）
    /// </summary>
    public long HeartBeatInterval { get; set; }

    /// <summary>
    /// 空闲心跳间隔秒（默认 300；客户端当前为死参仍下发，行为对齐）
    /// </summary>
    public long HeartBeatIntervalIdle { get; set; }

    /// <summary>
    /// 已读公告 id 清单（空缺省空数组）
    /// </summary>
    public List<long> ReceivedBulletinIds { get; set; } = new();

    /// <summary>
    /// 回归用户权益到期时间（空缺省 0）
    /// </summary>
    public long ReturnExpiredTime { get; set; }

    /// <summary>
    /// 离开天数（回归用户消费；空缺省 0）
    /// </summary>
    public long LostDays { get; set; }
}