using GameFrameX.Client.Api.GameUserMisc.Dto;
using GameFrameX.Client.Api.GameUserMisc.Models;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserMisc;

/// <summary>
/// 游戏杂项控制器（C8：BLMarkLogout / BLRandomRobots 自建）
/// <para>路由：POST /api/GameUserMisc/{Action}（继承 BaseController 基类路由）。</para>
/// </summary>
public class GameUserMiscController : BaseController
{
    private readonly Repository<TenantGameUserLogoutRecordEntity> _logoutRepository;
    private readonly Repository<TenantGameUserArchiveEntity> _archiveRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logoutRepository">登出记录仓储</param>
    /// <param name="archiveRepository">存档仓储（机器人从真实玩家档抽样）</param>
    public GameUserMiscController(Repository<TenantGameUserLogoutRecordEntity> logoutRepository,
        Repository<TenantGameUserArchiveEntity> archiveRepository)
    {
        _logoutRepository = logoutRepository;
        _archiveRepository = archiveRepository;
    }

    /// <summary>
    /// 登出标记（原 BLMarkLogout；仅删号成功后上报，在线统计用，响应 rcode 客户端不消费）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<MarkLogoutResponse>> MarkLogout([FromBody] MarkLogoutRequest request)
    {
        // 仅删号成功后上报（在线统计用）；响应 rcode 客户端不消费，落库即成功
        await _logoutRepository.InsertAsync(new TenantGameUserLogoutRecordEntity
        {
            UserId = request.UserId,
            ClientVersion = request.Version,
            LogoutTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Name = nameof(TenantGameUserLogoutRecordEntity),
        });
        return HttpJsonResultData<MarkLogoutResponse>.Success(new MarkLogoutResponse { Rcode = 0, });
    }

    /// <summary>
    /// 随机机器人（原 BLRandomRobots；真实档抽样 + 合成凑满，客户端严格断言数量）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<RandomRobotsResponse>> RandomRobots([FromBody] RandomRobotsRequest request)
    {
        var count = (int)request.RobotCount;
        var exclude = new HashSet<long>(request.ExcludeUserIds) { request.UserId, };
        // ponytail: 真实档候选全量取回内存随机（候选=昵称与头像齐全的档，机器人展示需要头像；
        // 空头像的档由合成兜底补位）；玩家量上来后改 SQL 随机抽样（跨 DbType 随机函数有差异），
        // 或引入独立机器人池配置
        var candidates = await _archiveRepository.GetListAsync(x => x.NickName != string.Empty && x.Icon != string.Empty);
        var pool = candidates.Where(x => !exclude.Contains(x.UserId)).ToList();
        Shuffle(pool);
        var robots = new List<GameUserRobotInfo>();
        foreach (var candidate in pool)
        {
            if (robots.Count >= count)
            {
                break;
            }

            robots.Add(new GameUserRobotInfo { UserId = candidate.UserId, Name = candidate.NickName, Icon = candidate.Icon, });
        }

        // 合成机器人凑满（客户端严格断言数量==请求，不足比伪造更糟）。
        // ponytail: id 用毫秒时间戳 + 序号（正数、< 2^53 Lua 安全整数）；
        // 真实玩家 id 若也落在该段有低概率相撞，届时改独立机器人池配置
        var seq = 1;
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (robots.Count < count)
        {
            robots.Add(new GameUserRobotInfo { UserId = stamp + seq, Name = "Robot_" + seq, Icon = "icon_robot_" + seq, });
            seq++;
        }

        return HttpJsonResultData<RandomRobotsResponse>.Success(new RandomRobotsResponse { Rcode = 0, Robots = robots, });
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