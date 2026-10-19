using System.Globalization;
using GameFrameX.Client.Api.GameUserSession.Models;
using GameFrameX.Client.Api.GameUserSession.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Client.Api.GameUserSession;

/// <summary>
/// 游戏玩家登录会话控制器（C13 登录控制面：LoginAsync 自建，承接原 BLLogin）
/// <para>路由：POST /api/GameUserSession/{Action}（继承 BaseController 基类路由）。</para>
/// <para>App 级信任链：用户身份由 body.UserId 携带（不经 SsoManager token），AppId 取自
/// <see cref="ClientApiAppContext"/>（同 C11：中间件上下文优先，无中间件时内联读头）。</para>
/// </summary>
public class GameUserSessionController : BaseController
{
    /// <summary>
    /// 最低客户端版本（ponytail: "0.0.0" 恒放行起步，管理面配置化留 P1；
    /// LauncherOptions 类注释禁止业务配置回流，故用常量承载）
    /// </summary>
    internal const string MinClientVersion = "0.0.0";

    private const long HeartBeatIntervalDefault = 60;
    private const long HeartBeatIntervalIdleDefault = 300;

    /// <summary>
    /// 会话 id 序号（同毫秒并发登录不撞）
    /// </summary>
    private static long _sessionSeq;

    private readonly Repository<TenantGameUserArchiveEntity> _archiveRepository;
    private readonly Repository<TenantGameUserEntity> _userRepository;
    private readonly Repository<TenantGameOrderEntity> _orderRepository;
    private readonly Repository<TenantGameAppServerConfigEntity> _serverConfigRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="archiveRepository">玩家存档仓储（C8 存档行 + P0-2 会话列）</param>
    /// <param name="userRepository">玩家账号仓储（注销判定）</param>
    /// <param name="orderRepository">支付订单仓储（累计充值汇总）</param>
    /// <param name="serverConfigRepository">游戏运营配置仓储（C315：Login config 数据源切租户库）</param>
    public GameUserSessionController(Repository<TenantGameUserArchiveEntity> archiveRepository,
        Repository<TenantGameUserEntity> userRepository,
        Repository<TenantGameOrderEntity> orderRepository,
        Repository<TenantGameAppServerConfigEntity> serverConfigRepository)
    {
        _archiveRepository = archiveRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _serverConfigRepository = serverConfigRepository;
    }

    /// <summary>
    /// 登录（28 字段四态去向见 C13 change.md §1）。
    /// <para>P0-3 保留：rcode=1 版本过低 / rcode=2 账号注销（AccountNoticeWindow 消费）。
    /// 其余空缺省字段按"空安全"约定下发（0 / "" / 空数组）。</para>
    /// <para>config 走 C280 差异下发（原 P0-1 回显法升级）：种子内容源 10 键，上行 md5
    /// 相等只回 {md5}，不等/缺失回 {md5, config}（对齐原厂 BLLogin 增量同步语义）。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserSessionLoginResponse>> Login([FromBody] GameUserSessionLoginRequest request)
    {
        // 注销判定：P0-3 rcode=2；查无行 = 放行（游客链路 Game.Api 不持有账号行）
        var user = await _userRepository.GetFirstAsync(x => x.Id == request.UserId);
        if (user != null && user.Status == GameUserStatus.Deleted)
        {
            return HttpJsonResultData<GameUserSessionLoginResponse>.Success(new GameUserSessionLoginResponse
            {
                Rcode = 2,
                LogoutTime = user.DeletedTime ?? 0,
                RequestId = request.RequestId,
            });
        }

        // 版本过低：P0-3 rcode=1（MinClientVersion 恒放行起步，直测 IsClientVersionTooLow）
        if (IsClientVersionTooLow(request.Version, MinClientVersion))
        {
            return HttpJsonResultData<GameUserSessionLoginResponse>.Success(new GameUserSessionLoginResponse
            {
                Rcode = 1,
                RequestId = request.RequestId,
            });
        }

        // 存档行（复用 C8）：有档读 SyncTime/InstallationId/DeviceModel 并覆盖会话标记；无档 lastSyncTime=0 不建行（存档链路负责建行）
        var archive = await _archiveRepository.GetFirstAsync(x => x.UserId == request.UserId);
        var lastSyncTime = 0L;
        var installationId = string.Empty;
        var updateDeviceModel = string.Empty;
        var session = CreateSessionId();
        if (archive != null)
        {
            lastSyncTime = archive.SyncTime;
            installationId = archive.InstallationId;
            updateDeviceModel = archive.DeviceModel;
            archive.SessionId = session;
            archive.SessionDeviceModel = request.DeviceModel;
            await _archiveRepository.UpdateAsync(archive);
        }

        // 累计充值：C11 订单已到账（Shipped 及以后）汇总（AccountId = userId 字符串）
        var accountId = request.UserId.ToString(CultureInfo.InvariantCulture);
        var orders = await _orderRepository.GetListAsync(x => x.AccountId == accountId && x.Status >= OrderStatusEnum.Shipped);
        var totalPay = orders.Sum(x => (long)x.Amount);

        return HttpJsonResultData<GameUserSessionLoginResponse>.Success(new GameUserSessionLoginResponse
        {
            Rcode = 0,
            Config = await BuildConfigAsync(request.ConfigEntries),
            LastSyncTime = lastSyncTime,
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TotalPay = totalPay,
            RequestId = request.RequestId,
            InstallationId = installationId,
            UpdateDeviceModel = updateDeviceModel,
            Session = session,
            HeartBeatInterval = HeartBeatIntervalDefault,
            HeartBeatIntervalIdle = HeartBeatIntervalIdleDefault,
        });
    }

    /// <summary>
    /// 客户端版本是否过低（语义化版本比较；客户端版本解析失败放行——不拦游客，min 解析失败同放行）
    /// </summary>
    internal static bool IsClientVersionTooLow(string clientVersion, string minVersion)
    {
        if (Version.TryParse(minVersion, out var min) == false)
        {
            return false;
        }

        if (Version.TryParse(clientVersion, out var version) == false)
        {
            return false;
        }

        return version < min;
    }

    /// <summary>
    /// config 差异下发（C280，原 P0-1 回显法升级；C315 数据源切租户库）：对齐原厂 BLLogin 语义——
    /// 上行 md5 与服务端 md5 相等 → 只回 {"md5"}（客户端不写不删不重启，省流量）；
    /// 不等或上行缺失 → 回 {"md5","config"}（客户端整体覆盖）；
    /// 上行有但服务端没有的键不回显（客户端删除本地该键，与原厂「无内容不下发」一致）。
    /// <para>生效行 = AppId=null 出厂基线打底 + 当前应用行按 Key 覆盖，Enabled=false 键不下发
    /// （GameServerConfigProvider）；运营改配置经 C314 管理面保存即自动重算 md5。</para>
    /// <para>"md5"/"config" 为协议小写键（客户端 ConfigModel 按 value.md5 / value.config 读取）。</para>
    /// </summary>
    private async Task<string> BuildConfigAsync(List<Models.GameUserSessionConfigEntry>? entries)
    {
        var uploaded = new Dictionary<string, string>();
        if (entries != null)
        {
            foreach (var entry in entries)
            {
                uploaded[entry.ConfigKey] = entry.ConfigMd5 ?? string.Empty;
            }
        }

        var effective = await GameServerConfigProvider.LoadEffectiveAsync(_serverConfigRepository, ClientApiAppContext.ResolveAppId(HttpContext));
        var config = new Dictionary<string, object>(effective.Count);
        foreach (var pair in effective)
        {
            if (uploaded.TryGetValue(pair.Key, out var clientMd5) && clientMd5 == pair.Value.Md5)
            {
                config[pair.Key] = new Dictionary<string, string> { ["md5"] = pair.Value.Md5, };
            }
            else
            {
                config[pair.Key] = new Dictionary<string, object>
                {
                    ["md5"] = pair.Value.Md5,
                    ["config"] = System.Text.Json.JsonDocument.Parse(pair.Value.Config).RootElement.Clone(),
                };
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(config);
    }

    private static long CreateSessionId()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Interlocked.Increment(ref _sessionSeq);
    }
}