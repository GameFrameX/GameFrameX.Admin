
using GameFrameX.Client.Api.GameUserArchive.Models;
namespace GameFrameX.Client.Api.GameUserArchive.Dto;

/// <summary>
/// 存档下载结果（原 BLGetUserBranch 出参 GetUserBranchResp，38 字段；下行全量）
/// <para>与上行相比：无 SyncList/InstallationId/DeviceModel；多 SyncTime/ForceSync。</para>
/// </summary>
public class GameUserArchiveDownloadResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功；1 = 静默忽略；2 = 版本错误；3 = 作弊检测——旧协议 rcode 分支语义原样保留）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 服务端存档版本戳
    /// </summary>
    public long SyncTime { get; set; }

    /// <summary>
    /// user 分表（1 个元素）
    /// </summary>
    public List<GameUserArchiveUserBranch> User { get; set; } = new();

    /// <summary>
    /// 体力表
    /// </summary>
    public List<string> Energy { get; set; } = new();

    /// <summary>
    /// 棋盘表层
    /// </summary>
    public List<string> Board { get; set; } = new();

    /// <summary>
    /// 缓存道具表
    /// </summary>
    public List<string> CacheItem { get; set; } = new();

    /// <summary>
    /// 主线任务表
    /// </summary>
    public List<string> MainTask { get; set; } = new();

    /// <summary>
    /// 废弃占位（恒空串）
    /// </summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>
    /// 道具解锁表
    /// </summary>
    public List<string> ItemUnlock { get; set; } = new();

    /// <summary>
    /// 商店道具表
    /// </summary>
    public List<string> ShopItem { get; set; } = new();

    /// <summary>
    /// 商店表
    /// </summary>
    public List<string> Shop { get; set; } = new();

    /// <summary>
    /// 仓库表
    /// </summary>
    public List<string> Inventory { get; set; } = new();

    /// <summary>
    /// 道具表
    /// </summary>
    public List<string> Item { get; set; } = new();

    /// <summary>
    /// 引导表
    /// </summary>
    public List<string> Tutorial { get; set; } = new();

    /// <summary>
    /// 订单表
    /// </summary>
    public List<string> Orders { get; set; } = new();

    /// <summary>
    /// 订单元数据表
    /// </summary>
    public List<string> OrderMeta { get; set; } = new();

    /// <summary>
    /// 杂项表
    /// </summary>
    public List<string> Misc { get; set; } = new();

    /// <summary>
    /// 礼包表
    /// </summary>
    public List<string> Bundle { get; set; } = new();

    /// <summary>
    /// 废弃占位（恒空串）
    /// </summary>
    public string AdStatus { get; set; } = string.Empty;

    /// <summary>
    /// 礼包元数据表
    /// </summary>
    public List<string> BundleMeta { get; set; } = new();

    /// <summary>
    /// 活动数据表
    /// </summary>
    public List<string> ActivityData { get; set; } = new();

    /// <summary>
    /// 玩家档案表
    /// </summary>
    public List<string> Profile { get; set; } = new();

    /// <summary>
    /// 回归玩家表
    /// </summary>
    public List<string> ReturnUser { get; set; } = new();

    /// <summary>
    /// 主线任务元数据表
    /// </summary>
    public List<string> MainTaskMeta { get; set; } = new();

    /// <summary>
    /// 本地奖励表
    /// </summary>
    public List<string> LocalRewards { get; set; } = new();

    /// <summary>
    /// BI 同步表
    /// </summary>
    public List<string> BiSync { get; set; } = new();

    /// <summary>
    /// 公告表
    /// </summary>
    public List<string> Notice { get; set; } = new();

    /// <summary>
    /// 强制覆盖标记（1 = 要求客户端以服务端数据覆盖本地）
    /// </summary>
    public long ForceSync { get; set; }

    /// <summary>
    /// 当前主线任务 id
    /// </summary>
    public string CurrentTask { get; set; } = string.Empty;

    /// <summary>
    /// 当前章节 id
    /// </summary>
    public long CurrentChapter { get; set; }

    /// <summary>
    /// 订单所属章节 id
    /// </summary>
    public long OrderChapterId { get; set; }

    /// <summary>
    /// 订单组 id
    /// </summary>
    public long OrderGroupId { get; set; }

    /// <summary>
    /// 头像 id
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 昵称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 当前佩戴徽章
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// 徽章数量
    /// </summary>
    public long BadgeCount { get; set; }

    /// <summary>
    /// 当前头像框
    /// </summary>
    public string Frame { get; set; } = string.Empty;

    /// <summary>
    /// 头像框数量
    /// </summary>
    public long FrameCount { get; set; }
}