namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// Online Runtime 业务响应体（来源 change C339 / vault:C2 S1.2 公共响应结构，Admin 侧消费形态）。
/// <para>
/// 物理传输层复用 Foundation <c>HttpJsonResultData</c> 信封（与 Server HTTP 交互惯例一致，经
/// <c>ToHttpJsonResult&lt;string&gt;</c> 解包）；本类型是信封 data 内的业务响应结构，对齐 vault C2 §1：
/// 响应至少包含 <c>Code / MessageKey / RequestId / ServerTime / Data</c>——机器判断走 <c>Code</c>，
/// 本地化走 <c>MessageKey</c>（Admin 侧按镜像数字码展示，不依赖中文文案判断业务成败）。
/// </para>
/// <para>字段名以 vault S1.2 定稿为唯一权威；vault 详表落盘前以此首版形态经 <c>OnlineServerClient</c> 内部调整，调用方无感。</para>
/// </summary>
public sealed class OnlineApiResponse<T> where T : class
{
    /// <summary>
    /// Online 协议业务码：0 成功；非 0 按 vault 八段分段（1xxx 系统网络 / 2xxx 鉴权 Session / 3xxx 作用域 /
    /// 4xxx 参数资源 / 5xxx 业务状态 / 6xxx 幂等冲突 / 7xxx 限流风控 / 8xxx 临时失败）。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 可本地化消息键（Online 侧 MessageKey 命名规范）。
    /// </summary>
    public string MessageKey { get; set; } = null!;

    /// <summary>
    /// 诊断消息（服务端原文，仅日志/排查用，客户端不得依赖其文案判断流程）。
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// 请求 Id（Online 侧回填，用于链路追踪）。
    /// </summary>
    public string RequestId { get; set; } = null!;

    /// <summary>
    /// 服务端时间戳（Unix 秒）。
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 业务数据载荷。
    /// </summary>
    public T Data { get; set; } = null!;
}
