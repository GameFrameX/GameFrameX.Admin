using GameFrameX.Client.Api.GameUserRank.Dto;
using GameFrameX.Client.Api.GameUserRank.Models;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserRank;

/// <summary>
/// 游戏玩家排行榜控制器（C9 排行域：RSGet*/RSQuery*/RSHeartBeat 9 op 自建）
/// <para>路由：POST /api/GameUserRank/{Action}（继承 BaseController 基类路由）。</para>
/// <para>分组模型：组按 RankType+ConfigKey+ConfigId+RoundToken 匹配（RoundToken = 报名时代币数），
/// 组容量满自动开新组；机器人补位行同表存储，保证 GetAsync/Query/HeartBeat 三视图一致。</para>
/// </summary>
public class GameUserRankController : BaseController
{
    /// <summary>
    /// 组容量（行数上限，满则开新组）
    /// </summary>
    public const int GroupCapacity = 50;

    /// <summary>
    /// 榜单固定规模（真实玩家 + 机器人凑满到此行数）
    /// </summary>
    public const int BoardSize = 30;

    /// <summary>
    /// 组 id 序号（同毫秒并发开组不撞）
    /// </summary>
    private static long _groupSeq;

    private readonly Repository<TenantGameRankEntryEntity> _rankRepository;
    private readonly Repository<TenantGameUserArchiveEntity> _archiveRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="rankRepository">排行榜条目仓储</param>
    /// <param name="archiveRepository">存档仓储（机器人从真实玩家档抽样）</param>
    public GameUserRankController(Repository<TenantGameRankEntryEntity> rankRepository,
        Repository<TenantGameUserArchiveEntity> archiveRepository)
    {
        _rankRepository = rankRepository;
        _archiveRepository = archiveRepository;
    }

    /// <summary>
    /// 报名进榜（GFX-706 由 Get 更名 JoinRank，消除 GET 语义误导；原 RSGetBakeOutRank/GetMultiRank/GetWeeklyRank1/GetWeeklyRank2，四 op 同构收编）
    /// <para>找同轮次令牌的未满组入组（满了开新组），插入/更新本人行（四展示字段 + Score=Token），
    /// 机器人补位到榜单固定规模，组内按 Score 降序算名次后返回全组。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserRankJoinResponse>> JoinRank([FromBody] GameUserRankJoinRequest request)
    {
        var entries = await _rankRepository.GetListAsync(x =>
                                                             x.RankType == request.RankType && x.ConfigKey == request.ConfigKey &&
                                                             x.ConfigId == request.ConfigId && x.RoundToken == request.Token);
        // 未满组取 GroupId 字典序最小者（同轮次先入的组先填）；无组则开新组
        var groupId = entries.GroupBy(x => x.GroupId)
                             .OrderBy(g => g.Key, StringComparer.Ordinal)
                             .FirstOrDefault(g => g.Count() < GroupCapacity)?.Key
                      ?? CreateGroupId();
        var own = entries.FirstOrDefault(x => x.GroupId == groupId && x.UserId == request.UserId);
        if (own == null)
        {
            own = new TenantGameRankEntryEntity
            {
                RankType = request.RankType,
                GroupId = groupId,
                ConfigKey = request.ConfigKey,
                ConfigId = request.ConfigId,
                UserId = request.UserId,
                Name = request.Name,
                Icon = request.Icon,
                IconFrame = request.IconFrame,
                Badge = request.Badge,
                Score = request.Token,
                RoundToken = request.Token,
            };
            await _rankRepository.InsertAsync(own);
        }
        else
        {
            own.Name = request.Name;
            own.Icon = request.Icon;
            own.IconFrame = request.IconFrame;
            own.Badge = request.Badge;
            own.Score = request.Token;
            await _rankRepository.UpdateAsync(own);
        }

        var rows = await _rankRepository.GetListAsync(x => x.RankType == request.RankType && x.GroupId == groupId);
        await PadRobotsAsync(request, groupId, rows);
        var beforeRank = rows.ToDictionary(x => x.Id, x => x.Rank);
        var ordered = RecomputeRanks(rows);
        var changed = ordered.Where(x => beforeRank[x.Id] != x.Rank).ToArray();
        if (changed.Length > 0)
        {
            await _rankRepository.UpdateRangeAsync(changed);
        }

        // ponytail: IsFinished 朴素时间窗判定——组创建时间（组内最早 CreatedTime）+ 固定窗长
        // （周榜 7 天 / 活动榜 3 天），无独立结算任务与活动配置源，超窗即视为已结算；
        // 接入活动配置/结算任务后改由配置下发窗长与重置点驱动
        var created = rows.Min(x => x.CreatedTime);
        var windowSeconds = request.RankType == 3 || request.RankType == 4 ? 7L * 86400 : 3L * 86400;
        var isFinished = DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= created + windowSeconds ? 1 : 0;
        return HttpJsonResultData<GameUserRankJoinResponse>.Success(new GameUserRankJoinResponse
        {
            Rcode = 0,
            IsFinished = isFinished,
            GroupId = groupId,
            Ranks = ToRankInfos(ordered),
        });
    }

    /// <summary>
    /// 查询组内排行（原 RSQueryBakeOutRank/QueryMultiRank/QueryWeeklyRank1/QueryWeeklyRank2，四 op 完全同构收编）
    /// <para>按 GroupId 查组内全部行，名次实时重算（分数可能因心跳变化）。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserRankQueryResponse>> Query([FromBody] GameUserRankQueryRequest request)
    {
        var rows = await _rankRepository.GetListAsync(x => x.RankType == request.RankType && x.GroupId == request.GroupId);
        // 查询只读：重算不落库（Rank 列仅作写路径缓存）
        var ordered = RecomputeRanks(rows);
        return HttpJsonResultData<GameUserRankQueryResponse>.Success(new GameUserRankQueryResponse
        {
            Rcode = 0,
            Ranks = ToRankInfos(ordered),
        });
    }

    /// <summary>
    /// 排行心跳（原 RSHeartBeat HTTP 分支收编；Session 旧通道会话透传不校验——Game.Api 无会话源）
    /// <para>更新本人四榜行 Score（无行则跳过该榜），回四榜名次（无行回 0）。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserRankHeartBeatResponse>> HeartBeat([FromBody] GameUserRankHeartBeatRequest request)
    {
        var response = new GameUserRankHeartBeatResponse { Rcode = 0, };
        response.BakeoutRank = await UpdateBoardScoreAsync(1, request.UserId, request.BakeoutToken);
        response.MultiRank = await UpdateBoardScoreAsync(2, request.UserId, request.MultiToken);
        response.Weekly1Rank = await UpdateBoardScoreAsync(3, request.UserId, request.Weekly1Token);
        response.Weekly2Rank = await UpdateBoardScoreAsync(4, request.UserId, request.Weekly2Token);
        return HttpJsonResultData<GameUserRankHeartBeatResponse>.Success(response);
    }

    /// <summary>
    /// 机器人补位到榜单固定规模（真实玩家档抽样 + 合成凑满，插入同表保证三视图一致）
    /// </summary>
    private async Task PadRobotsAsync(GameUserRankJoinRequest request, string groupId, List<TenantGameRankEntryEntity> rows)
    {
        if (rows.Count >= BoardSize)
        {
            return;
        }

        var memberIds = rows.Select(x => x.UserId).ToHashSet();
        // ponytail: 真实档候选全量取回内存随机（同 RandomRobots）；玩家量上来后改 SQL 随机抽样或机器人池配置
        var candidates = await _archiveRepository.GetListAsync(x => x.NickName != string.Empty && x.Icon != string.Empty);
        Shuffle(candidates);
        var pool = new Queue<TenantGameUserArchiveEntity>(
            candidates.Where(x => !memberIds.Contains(x.UserId) && x.UserId != request.UserId));
        var seq = 1;
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var robots = new List<TenantGameRankEntryEntity>();
        while (rows.Count + robots.Count < BoardSize)
        {
            long userId;
            string name;
            string icon;
            string iconFrame;
            string badge;
            if (pool.Count > 0)
            {
                var candidate = pool.Dequeue();
                userId = candidate.UserId;
                name = candidate.NickName;
                icon = candidate.Icon;
                iconFrame = candidate.Frame;
                badge = candidate.Badge;
            }
            else
            {
                // ponytail: 合成 id 用毫秒时间戳 + 序号（同 RandomRobots）；真实玩家 id 低概率相撞时改机器人池配置
                userId = stamp + seq;
                name = "Robot_" + seq;
                icon = "icon_robot_" + seq;
                iconFrame = string.Empty;
                badge = string.Empty;
                seq++;
            }

            // ponytail: 机器人分数围绕玩家 token 散布（伪随机 0.1x~1.9x）；旧服机器人难度曲线黑盒
            robots.Add(new TenantGameRankEntryEntity
            {
                RankType = request.RankType,
                GroupId = groupId,
                ConfigKey = request.ConfigKey,
                ConfigId = request.ConfigId,
                UserId = userId,
                Name = name,
                Icon = icon,
                IconFrame = iconFrame,
                Badge = badge,
                Score = (long)(request.Token * (Random.Shared.NextDouble() * 1.8 + 0.1)),
                RoundToken = request.Token,
            });
        }

        await _rankRepository.InsertRangeAsync(robots.ToArray());
        rows.AddRange(robots);
    }

    /// <summary>
    /// 更新一榜分数并返回组内名次（无行返回 0）
    /// </summary>
    private async Task<long> UpdateBoardScoreAsync(int rankType, long userId, long score)
    {
        // 玩家可能因 token 推进在多个组留有历史行，取最新一行（雪花 Id 单调）作为当前榜
        var ownRows = await _rankRepository.GetListAsync(x => x.RankType == rankType && x.UserId == userId);
        if (ownRows.Count == 0)
        {
            return 0;
        }

        var ownGroupId = ownRows.OrderByDescending(x => x.Id).First().GroupId;
        var groupRows = await _rankRepository.GetListAsync(x => x.RankType == rankType && x.GroupId == ownGroupId);
        var tracked = groupRows.First(x => x.UserId == userId);
        tracked.Score = score;
        var ordered = groupRows.OrderByDescending(x => x.Score).ThenBy(x => x.UserId).ToList();
        var rank = 0L;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].UserId == userId)
            {
                rank = i + 1;
                break;
            }
        }

        tracked.Rank = (int)rank;
        await _rankRepository.UpdateAsync(tracked);
        return rank;
    }

    /// <summary>
    /// 组内按 Score 降序（同分按 UserId 升序稳定排序）就地重算名次
    /// </summary>
    private static List<TenantGameRankEntryEntity> RecomputeRanks(List<TenantGameRankEntryEntity> rows)
    {
        var ordered = rows.OrderByDescending(x => x.Score).ThenBy(x => x.UserId).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Rank = i + 1;
        }

        return ordered;
    }

    private static List<GameRankInfo> ToRankInfos(List<TenantGameRankEntryEntity> ordered)
    {
        var result = new List<GameRankInfo>();
        foreach (var row in ordered)
        {
            result.Add(new GameRankInfo
            {
                UserId = row.UserId,
                Name = row.Name,
                Icon = row.Icon,
                IconFrame = row.IconFrame,
                Score = row.Score,
                Rank = row.Rank,
                Badge = row.Badge,
            });
        }

        return result;
    }

    private static string CreateGroupId()
    {
        return "g" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Interlocked.Increment(ref _groupSeq);
    }

    /// <summary>
    /// Fisher-Yates 洗牌（机器人候选随机化）
    /// </summary>
    private static void Shuffle(List<TenantGameUserArchiveEntity> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}