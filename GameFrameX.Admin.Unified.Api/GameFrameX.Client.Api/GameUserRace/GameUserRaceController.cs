using GameFrameX.Client.Api.GameUserRace.Dto;
using GameFrameX.Client.Api.GameUserRace.Models;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserRace;

/// <summary>
/// 游戏玩家比赛控制器（C9 比赛域：BL 比赛 6 op 自建，形状全同收编单端点）
/// <para>路由：POST /api/GameUserRace/Entry（继承 BaseController 基类路由）。</para>
/// </summary>
public class GameUserRaceController : BaseController
{
    private readonly Repository<TenantGameUserArchiveEntity> _archiveRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="archiveRepository">存档仓储（机器人从真实玩家档抽样）</param>
    public GameUserRaceController(Repository<TenantGameUserArchiveEntity> archiveRepository)
    {
        _archiveRepository = archiveRepository;
    }

    /// <summary>
    /// 比赛报名（原 BL 比赛 6 op：Coin/Pk/Pk1/Crash/Upgrade/Day，形状全同；Day 例外无 Round）
    /// <para>无状态即时生成机器人，不落库：按 GroupsNum 每档生成对应数量（真实玩家档抽样 + 合成凑满），
    /// 成绩按 group_type 难度基准 × Round（难度曲线）伪随机生成。客户端严格断言返回数量 == RobotCount。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserRaceEntryResponse>> Entry([FromBody] GameUserRaceEntryRequest request)
    {
        var total = (int)request.RobotCount;
        var exclude = new HashSet<long>(request.ExcludeUserIds) { request.UserId, };
        // ponytail: 真实档候选全量取回内存随机（同 RandomRobots）；玩家量上来后改 SQL 随机抽样或机器人池配置
        var candidates = await _archiveRepository.GetListAsync(x => x.NickName != string.Empty && x.Icon != string.Empty);
        Shuffle(candidates);
        var pool = new Queue<TenantGameUserArchiveEntity>(candidates.Where(x => !exclude.Contains(x.UserId)));
        var round = Math.Max(request.Round ?? 1, 1);
        var seq = 1;
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        GameRaceRobotInfo TakeNext(string groupType)
        {
            long userId;
            string name;
            string icon;
            if (pool.Count > 0)
            {
                var candidate = pool.Dequeue();
                userId = candidate.UserId;
                name = candidate.NickName;
                icon = candidate.Icon;
            }
            else
            {
                // ponytail: 合成 id 用毫秒时间戳 + 序号（正数、< 2^53 Lua 安全整数，同 RandomRobots）；
                // 真实玩家 id 若也落在该段有低概率相撞，届时改独立机器人池配置
                userId = stamp + seq;
                name = "Robot_" + seq;
                icon = "icon_robot_" + seq;
                seq++;
            }

            return BuildRobot(request.RaceType, groupType, round, userId, name, icon);
        }

        var robots = new List<GameRaceRobotInfo>();
        // ponytail: 档位数量以 RobotCount 为总上限截断、各档之和不足时按首档兜底凑满
        // （客户端严格断言数量；旧协议 GroupsNum 各档之和恒等于 RobotCount，越界组合为防御性兜底）
        foreach (var tier in request.GroupsNum)
        {
            var tierCount = (int)Math.Min(tier.GroupNum, Math.Max(0, total - robots.Count));
            for (var i = 0; i < tierCount; i++)
            {
                robots.Add(TakeNext(tier.GroupType));
            }
        }

        var padGroupType = request.GroupsNum.FirstOrDefault()?.GroupType ?? "1";
        while (robots.Count < total)
        {
            robots.Add(TakeNext(padGroupType));
        }

        return HttpJsonResultData<GameUserRaceEntryResponse>.Success(new GameUserRaceEntryResponse
        {
            Rcode = 0,
            Robots = robots,
        });
    }

    /// <summary>
    /// 构造单个机器人（计分制填 Score 十进制字符串，计时制填 Second 秒数，两者互斥）
    /// </summary>
    private static GameRaceRobotInfo BuildRobot(int raceType, string groupType, long round, long userId, string name, string icon)
    {
        var ability = ResolveDifficultyBase(groupType) * round;
        var robot = new GameRaceRobotInfo
        {
            UserId = userId,
            Name = name,
            Icon = icon,
            GroupType = groupType,
            Ability = ability,
        };
        if (IsScoringRace(raceType))
        {
            // 计分制：难度基准 × 轮次 ±20% 抖动的十进制字符串
            robot.Score = (ability * (80 + Random.Shared.Next(0, 41)) / 100).ToString();
        }
        else
        {
            // 计时制：难度/轮次越高用时越短（900 基准秒 + 小抖动）
            robot.Second = Math.Max(1, 900 / ability + Random.Shared.Next(0, 5));
        }

        return robot;
    }

    /// <summary>
    /// 计分制赛型（Coin/Pk/Day 用 Score）；计时制（Pk1/Crash/Upgrade）用 Second
    /// </summary>
    private static bool IsScoringRace(int raceType)
    {
        return raceType == 1 || raceType == 2 || raceType == 6;
    }

    /// <summary>
    /// group_type 难度基准（1..9 稳定映射）
    /// <para>ponytail: 旧服难度曲线黑盒，可解析数字取 1..9、否则字符和取模；接入比赛配置后改查配置表。</para>
    /// </summary>
    private static long ResolveDifficultyBase(string groupType)
    {
        if (long.TryParse(groupType, out var parsed) && parsed > 0)
        {
            return Math.Min(parsed, 9);
        }

        var sum = 0;
        foreach (var c in groupType)
        {
            sum += c;
        }

        return sum % 9 + 1;
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