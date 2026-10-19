namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// GFX Online 平台错误码 Admin 侧镜像（段 320～327，见 <see cref="ErrorSegmentRegistry"/>；来源 change C339 / vault:C2 S1.3）。
/// <para>
/// 「Online 协议码 ↔ Admin 镜像码」映射语义：Online Runtime 的协议错误码按 vault 八段分段
/// （1xxx 系统与网络 / 2xxx 鉴权与 Session / 3xxx 租户·App·Server 作用域 / 4xxx 参数与资源 /
/// 5xxx 业务状态 / 6xxx 幂等与冲突 / 7xxx 限流与风控 / 8xxx 临时失败与可重试），
/// Admin 侧按「协议段 Nxxx ↔ 镜像段 32(N-1)」一一镜像登记为 320～327 八个子段；
/// Online 返回非 0 协议码时由 OnlineServerClient 按协议段映射到本枚举镜像成员抛 <c>BusinessError</c>，
/// 原始协议码随异常 errors 透出（机器可判断 <c>Code</c> + 可本地化 <c>MessageKey</c> 双通道，客户端不得依赖中文文案判断流程）。
/// </para>
/// <para>
/// 镜像成员的唯一来源是 vault 错误码表（vault 详表未落盘期间以 vault spec.md L105 八段定义 +
/// 2026-09-13 项目负责人评审结论拟定，快照见 gfx-doc/dev/online-platform/contracts/error-code-table.md）；
/// 三端一致性由 OnlineErrorCodeContractTests 三方对照守护（OnlineErrorCode ↔ error-codes.json ↔ vault 表快照），
/// 映射关系不可手改数值，调整须回 vault 评审后同步三端。
/// </para>
/// </summary>
public enum OnlineErrorCode
{
    // ---- 段 320：镜像 vault 1xxx 系统与网络 ----

    /// <summary>
    /// Online 系统内部错误（镜像 1xxx）
    /// </summary>
    [ErrorText("Online 系统内部错误", "Online internal error")]
    InternalError = 32000001,

    /// <summary>
    /// Online 网络超时（镜像 1xxx）
    /// </summary>
    [ErrorText("Online 网络超时", "Online network timeout")]
    NetworkTimeout = 32000002,

    /// <summary>
    /// Online 网络错误（镜像 1xxx）
    /// </summary>
    [ErrorText("Online 网络错误", "Online network error")]
    NetworkError = 32000003,

    // ---- 段 321：镜像 vault 2xxx 鉴权和 Session ----

    /// <summary>
    /// Online 会话令牌已过期（镜像 2xxx）
    /// </summary>
    [ErrorText("Online 会话令牌已过期", "Online session token expired")]
    TokenExpired = 32100001,

    /// <summary>
    /// Online 会话令牌已被吊销（镜像 2xxx）
    /// </summary>
    [ErrorText("Online 会话令牌已被吊销", "Online session token revoked")]
    TokenRevoked = 32100002,

    /// <summary>
    /// Online 会话无效或未授权（镜像 2xxx）
    /// </summary>
    [ErrorText("Online 会话无效或未授权", "Online session invalid or unauthorized")]
    SessionInvalid = 32100003,

    // ---- 段 322：镜像 vault 3xxx 租户、App、Server 作用域 ----

    /// <summary>
    /// Online 作用域越界拒绝（跨租户 / 跨 App / 跨服，镜像 3xxx；VC-1.6/1.7 Admin 半边拒绝码）
    /// </summary>
    [ErrorText("Online 作用域越界，请求被拒绝", "Online scope violation, request denied")]
    ScopeDenied = 32200001,

    /// <summary>
    /// Online 跨租户请求被拒绝（镜像 3xxx）
    /// </summary>
    [ErrorText("Online 跨租户请求被拒绝", "Online cross-tenant request denied")]
    CrossTenantDenied = 32200002,

    /// <summary>
    /// Online 跨 App 请求被拒绝（镜像 3xxx）
    /// </summary>
    [ErrorText("Online 跨 App 请求被拒绝", "Online cross-app request denied")]
    CrossAppDenied = 32200003,

    /// <summary>
    /// Online 区服作用域非法（镜像 3xxx）
    /// </summary>
    [ErrorText("Online 区服作用域非法", "Online server scope violation")]
    ServerScopeDenied = 32200004,

    /// <summary>
    /// Online 作用域缺失（Tenant/App/Server 未提供，镜像 3xxx）
    /// </summary>
    [ErrorText("Online 作用域缺失", "Online scope missing")]
    ScopeMissing = 32200005,

    // ---- 段 323：镜像 vault 4xxx 参数和资源 ----

    /// <summary>
    /// Online 参数校验失败（镜像 4xxx）
    /// </summary>
    [ErrorText("Online 参数校验失败", "Online parameter validation failed")]
    ParameterInvalid = 32300001,

    /// <summary>
    /// Online 资源不存在（镜像 4xxx）
    /// </summary>
    [ErrorText("Online 资源不存在", "Online resource not found")]
    ResourceNotFound = 32300002,

    // ---- 段 324：镜像 vault 5xxx 业务状态 ----

    /// <summary>
    /// Online 业务状态未准备（镜像 5xxx）
    /// </summary>
    [ErrorText("Online 业务状态未准备", "Online business state not ready")]
    StateNotReady = 32400001,

    /// <summary>
    /// Online 业务状态已结束（镜像 5xxx）
    /// </summary>
    [ErrorText("Online 业务状态已结束", "Online business state already ended")]
    StateEnded = 32400002,

    /// <summary>
    /// Online 业务当前状态不允许该操作（镜像 5xxx）
    /// </summary>
    [ErrorText("Online 业务当前状态不允许该操作", "Online business state does not allow this operation")]
    StateOperationForbidden = 32400003,

    // ---- 段 325：镜像 vault 6xxx 幂等和冲突 ----

    /// <summary>
    /// Online 重复请求（幂等命中首次结果，镜像 6xxx）
    /// </summary>
    [ErrorText("Online 重复请求", "Online duplicate request")]
    DuplicateRequest = 32500001,

    /// <summary>
    /// Online 版本冲突（镜像 6xxx）
    /// </summary>
    [ErrorText("Online 版本冲突", "Online version conflict")]
    VersionConflict = 32500002,

    // ---- 段 326：镜像 vault 7xxx 限流、安全和风控 ----

    /// <summary>
    /// Online 频率超限（镜像 7xxx）
    /// </summary>
    [ErrorText("Online 频率超限", "Online rate limit exceeded")]
    RateLimitExceeded = 32600001,

    /// <summary>
    /// Online 风控拒绝（镜像 7xxx）
    /// </summary>
    [ErrorText("Online 风控拒绝", "Online risk control rejected")]
    RiskControlRejected = 32600002,

    /// <summary>
    /// Online 账号已被封禁（镜像 7xxx）
    /// </summary>
    [ErrorText("Online 账号已被封禁", "Online account banned")]
    AccountBanned = 32600003,

    // ---- 段 327：镜像 vault 8xxx 临时失败和可重试 ----

    /// <summary>
    /// Online 服务忙，稍后重试（镜像 8xxx）
    /// </summary>
    [ErrorText("Online 服务忙，请稍后重试", "Online service busy, please retry later")]
    ServiceBusy = 32700001,

    /// <summary>
    /// Online 依赖服务暂不可用（镜像 8xxx）
    /// </summary>
    [ErrorText("Online 依赖服务暂不可用", "Online dependency temporarily unavailable")]
    DependencyUnavailable = 32700002,
}
