using System.Text.Json;
using GameFrameX.Client.Api.GameUserArchive.Models;
using GameFrameX.Client.Api.GameUserArchive.Dto;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserArchive;

/// <summary>
/// 游戏玩家存档控制器（C8 存档三件套）
/// <para>路由：POST /api/GameUserArchive/{Action}（继承 BaseController 基类路由，不走动态 API 规则）。</para>
/// <para>存储为透传模型：22 张 String[] 分表以原始行字符串（客户端 DBTable 逗号拼接格式）存
/// BranchesJson，服务端不解析行内容——round-trip 逐元素字节等价的前提（见 C8 notes §E）。</para>
/// </summary>
public class GameUserArchiveController : BaseController
{
    private readonly Repository<TenantGameUserArchiveEntity> _archiveRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="archiveRepository">存档仓储</param>
    public GameUserArchiveController(Repository<TenantGameUserArchiveEntity> archiveRepository)
    {
        _archiveRepository = archiveRepository;
    }

    /// <summary>
    /// 上传整档（分表增量；原 BLUpdUserBranch）。
    /// <para>分表三态：SyncList 命中且数组非空 = 提交；命中且空数组 = 清空该表（arrDiscardSyncKey
    /// 语义）；未命中 = 保持服务端现状，空数组亦不得视为清空。</para>
    /// <para>乐观锁：SyncTime 为客户端上次获得的版本戳，与服务端不符 → body Rcode=3（客户端
    /// SyncModel 上传 rcode==3 分支：_SaveNeedForceSync + 强制 Download 后重放）；服务端无档
    /// （首传）跳过校验。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserArchiveUploadResponse>> Upload([FromBody] GameUserArchiveUploadRequest request)
    {
        var entity = await _archiveRepository.GetFirstAsync(x => x.UserId == request.UserId);
        if (entity != null && entity.SyncTime != request.SyncTime)
        {
            // 冲突走 body Rcode=3 而非信封错误码：客户端 HandleResponse 对一切信封 code≠0
            // 一律折叠成传输失败（bSuccess=false），旧通道的版本冲突消费分支等的是 body
            // rcode==3，信封错误码永远到不了那个分支（2026-09-07 修复；枚举
            // ArchiveErrorCode.ArchiveSyncConflict=60400001 保留为契约定义）。
            return HttpJsonResultData<GameUserArchiveUploadResponse>.Success(new GameUserArchiveUploadResponse { Rcode = 3, });
        }

        var branches = DeserializeBranches(entity);
        if (InSyncList(request.SyncList, "Energy"))
        {
            branches["Energy"] = request.Energy;
        }

        if (InSyncList(request.SyncList, "Board"))
        {
            branches["Board"] = request.Board;
        }

        if (InSyncList(request.SyncList, "CacheItem"))
        {
            branches["CacheItem"] = request.CacheItem;
        }

        if (InSyncList(request.SyncList, "MainTask"))
        {
            branches["MainTask"] = request.MainTask;
        }

        if (InSyncList(request.SyncList, "ItemUnlock"))
        {
            branches["ItemUnlock"] = request.ItemUnlock;
        }

        if (InSyncList(request.SyncList, "ShopItem"))
        {
            branches["ShopItem"] = request.ShopItem;
        }

        if (InSyncList(request.SyncList, "Shop"))
        {
            branches["Shop"] = request.Shop;
        }

        if (InSyncList(request.SyncList, "Inventory"))
        {
            branches["Inventory"] = request.Inventory;
        }

        if (InSyncList(request.SyncList, "Item"))
        {
            branches["Item"] = request.Item;
        }

        if (InSyncList(request.SyncList, "Tutorial"))
        {
            branches["Tutorial"] = request.Tutorial;
        }

        if (InSyncList(request.SyncList, "Orders"))
        {
            branches["Orders"] = request.Orders;
        }

        if (InSyncList(request.SyncList, "OrderMeta"))
        {
            branches["OrderMeta"] = request.OrderMeta;
        }

        if (InSyncList(request.SyncList, "Misc"))
        {
            branches["Misc"] = request.Misc;
        }

        if (InSyncList(request.SyncList, "Bundle"))
        {
            branches["Bundle"] = request.Bundle;
        }

        if (InSyncList(request.SyncList, "BundleMeta"))
        {
            branches["BundleMeta"] = request.BundleMeta;
        }

        if (InSyncList(request.SyncList, "ActivityData"))
        {
            branches["ActivityData"] = request.ActivityData;
        }

        if (InSyncList(request.SyncList, "Profile"))
        {
            branches["Profile"] = request.Profile;
        }

        if (InSyncList(request.SyncList, "ReturnUser"))
        {
            branches["ReturnUser"] = request.ReturnUser;
        }

        if (InSyncList(request.SyncList, "MainTaskMeta"))
        {
            branches["MainTaskMeta"] = request.MainTaskMeta;
        }

        if (InSyncList(request.SyncList, "LocalRewards"))
        {
            branches["LocalRewards"] = request.LocalRewards;
        }

        if (InSyncList(request.SyncList, "BiSync"))
        {
            branches["BiSync"] = request.BiSync;
        }

        if (InSyncList(request.SyncList, "Notice"))
        {
            branches["Notice"] = request.Notice;
        }

        // 版本戳毫秒级 + 兜底 +1：同毫秒内连续上传仍严格递增（乐观锁防重放语义）
        var newSyncTime = Math.Max(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), (entity?.SyncTime ?? 0) + 1);
        if (entity == null)
        {
            entity = new TenantGameUserArchiveEntity { UserId = request.UserId, };
            ApplyUploadState(entity, request, branches, newSyncTime);
            await _archiveRepository.InsertAsync(entity);
        }
        else
        {
            ApplyUploadState(entity, request, branches, newSyncTime);
            await _archiveRepository.UpdateAsync(entity);
        }

        return HttpJsonResultData<GameUserArchiveUploadResponse>.Success(new GameUserArchiveUploadResponse { Rcode = 0, NewSyncTime = newSyncTime, });
    }

    /// <summary>
    /// 下载整档（全量下行；原 BLGetUserBranch）。
    /// <para>Rcode 语义原样保留（0=成功；1/2/3 分支由客户端状态机消费）。
    /// 服务端无档 = 新用户场景，回 Rcode 1（旧通道静默忽略语义：客户端空分支本地起步）。
    /// 不可用 Rcode 0 + ForceSync=1 + 空档：ForceSync=1 的消费端是 _UseServerData，
    /// 空 user 数组在 response.user[1] 直接 nil 崩（C13 Editor 实连发现）。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserArchiveDownloadResponse>> Download([FromBody] GameUserArchiveDownloadRequest request)
    {
        var entity = await _archiveRepository.GetFirstAsync(x => x.UserId == request.UserId);
        if (entity == null)
        {
            return HttpJsonResultData<GameUserArchiveDownloadResponse>.Success(new GameUserArchiveDownloadResponse { Rcode = 1, });
        }

        var branches = DeserializeBranches(entity);
        var result = new GameUserArchiveDownloadResponse
        {
            Rcode = 0,
            SyncTime = entity.SyncTime,
            ForceSync = 0,
            User = new List<GameUserArchiveUserBranch>
            {
                new()
                {
                    Gold = entity.Gold,
                    Gem = entity.Gem,
                    SkipProp = entity.SkipProp,
                    Level = entity.Level,
                    Exp = entity.Exp,
                    Progress = long.TryParse(entity.Progress, out var progressValue) ? progressValue : 0,
                    Energy = entity.Energy,
                    Day = entity.Day,
                },
            },
            Energy = Branch(branches, "Energy"),
            Board = Branch(branches, "Board"),
            CacheItem = Branch(branches, "CacheItem"),
            MainTask = Branch(branches, "MainTask"),
            ItemUnlock = Branch(branches, "ItemUnlock"),
            ShopItem = Branch(branches, "ShopItem"),
            Shop = Branch(branches, "Shop"),
            Inventory = Branch(branches, "Inventory"),
            Item = Branch(branches, "Item"),
            Tutorial = Branch(branches, "Tutorial"),
            Orders = Branch(branches, "Orders"),
            OrderMeta = Branch(branches, "OrderMeta"),
            Misc = Branch(branches, "Misc"),
            Bundle = Branch(branches, "Bundle"),
            BundleMeta = Branch(branches, "BundleMeta"),
            ActivityData = Branch(branches, "ActivityData"),
            Profile = Branch(branches, "Profile"),
            ReturnUser = Branch(branches, "ReturnUser"),
            MainTaskMeta = Branch(branches, "MainTaskMeta"),
            LocalRewards = Branch(branches, "LocalRewards"),
            BiSync = Branch(branches, "BiSync"),
            Notice = Branch(branches, "Notice"),
            CurrentTask = entity.CurrentTask,
            CurrentChapter = entity.CurrentChapter,
            OrderChapterId = entity.OrderChapterId,
            OrderGroupId = entity.OrderGroupId,
            Icon = entity.Icon,
            Name = entity.NickName,
            Badge = entity.Badge,
            BadgeCount = entity.BadgeCount,
            Frame = entity.Frame,
            FrameCount = entity.FrameCount,
        };
        return HttpJsonResultData<GameUserArchiveDownloadResponse>.Success(result);
    }

    /// <summary>
    /// 查询目标玩家存档摘要（原 BLGetOtherProgress；绑定/切换冲突窗展示用）。
    /// <para>Rcode：0 = 有云端数据（摘要字段）；1 = 无云端数据（客户端展示兜底值）。</para>
    /// <para>客户端调用方已在 C7 删除（死代码），服务端按契约先建能力，接线随冲突窗恢复。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserArchiveOtherProgressResponse>> GetOtherProgress([FromBody] GameUserArchiveOtherProgressRequest request)
    {
        var entity = await _archiveRepository.GetFirstAsync(x => x.UserId == request.OtherId);
        if (entity == null)
        {
            return HttpJsonResultData<GameUserArchiveOtherProgressResponse>.Success(new GameUserArchiveOtherProgressResponse { Rcode = 1, });
        }

        var response = new GameUserArchiveOtherProgressResponse
        {
            Rcode = 0,
            SyncTime = entity.SyncTime,
            Gem = entity.Gem,
            SkipProp = entity.SkipProp,
            Gold = entity.Gold,
            Level = entity.Level,
            CurrentTask = entity.CurrentTask,
            CurrentChapter = entity.CurrentChapter,
            OrderChapterId = entity.OrderChapterId,
            OrderGroupId = entity.OrderGroupId,
            Badge = entity.Badge,
            BadgeCount = entity.BadgeCount,
            Frame = entity.Frame,
            FrameCount = entity.FrameCount,
        };
        return HttpJsonResultData<GameUserArchiveOtherProgressResponse>.Success(response);
    }

    /// <summary>
    /// 上传落库统一出口：透传 JSON、user 摘要列（GetOtherProgress/排行展示免解析）、
    /// 冗余摘要组与换端字段。user 表非空 = 提交（与分表三态无关，恒为全量摘要）。
    /// </summary>
    private static void ApplyUploadState(TenantGameUserArchiveEntity entity, GameUserArchiveUploadRequest request,
        Dictionary<string, List<string>> branches, long newSyncTime)
    {
        entity.BranchesJson = JsonSerializer.Serialize(branches);
        entity.SyncTime = newSyncTime;
        var user = request.User.FirstOrDefault();
        if (user != null)
        {
            entity.Gold = user.Gold;
            entity.Gem = user.Gem;
            entity.SkipProp = user.SkipProp;
            entity.Level = user.Level;
            entity.Exp = user.Exp;
            entity.Progress = user.Progress.ToString();
            entity.Energy = user.Energy;
            entity.Day = user.Day;
        }

        entity.CurrentTask = request.CurrentTask;
        entity.CurrentChapter = request.CurrentChapter;
        entity.OrderChapterId = request.OrderChapterId;
        entity.OrderGroupId = request.OrderGroupId;
        entity.Icon = request.Icon;
        entity.NickName = request.Name;
        // 基类 Name 列非空（管理面通用字段），与昵称同值填充
        entity.Name = request.Name;
        entity.Badge = request.Badge;
        entity.BadgeCount = request.BadgeCount;
        entity.Frame = request.Frame;
        entity.FrameCount = request.FrameCount;
        entity.InstallationId = request.InstallationId;
        entity.DeviceModel = request.DeviceModel;
        entity.ClientVersion = request.Version;
    }

    /// <summary>
    /// SyncList 命中判定：客户端表名形态混合（小写 item/board、下划线 activity_data/bi_sync、
    /// camelCase orderMeta/mainTaskMeta/cacheItem），与 DTO PascalCase 属性名按「去下划线 +
    /// 忽略大小写」归一比较；22 张表归一名互不冲突。
    /// </summary>
    private static bool InSyncList(List<string> syncList, string branchName)
    {
        if (syncList == null)
        {
            return false;
        }

        var normalized = branchName.Replace("_", string.Empty).ToLowerInvariant();
        foreach (var name in syncList)
        {
            if (name.Replace("_", string.Empty).ToLowerInvariant() == normalized)
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, List<string>> DeserializeBranches(TenantGameUserArchiveEntity entity)
    {
        if (entity == null || string.IsNullOrEmpty(entity.BranchesJson))
        {
            return new Dictionary<string, List<string>>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(entity.BranchesJson)
               ?? new Dictionary<string, List<string>>();
    }

    private static List<string> Branch(Dictionary<string, List<string>> branches, string name)
    {
        return branches.TryGetValue(name, out var value) ? value : new List<string>();
    }
}