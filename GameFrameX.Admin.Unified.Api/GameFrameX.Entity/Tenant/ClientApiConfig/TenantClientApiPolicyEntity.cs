using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 租户 Client API 运行策略合并配置（C378 四表合一：Token / 登录票据 / 区服输出 / 订单策略）。
/// <para>节划分理由：令牌节是服务端内部签名密钥语义，天然租户级（不区分应用）；票据节与输出节均为应用作用域的运行窗口 / 开关，随行作用域回退。</para>
/// <para>存量数据由启动期迁移执行器从原四表幂等迁入（同键 Id 大者优先），迁移后原四表停写。</para>
/// </summary>
[SugarTable(null, "租户ClientAPI运行策略")]
[TenantTable]
[IncrementTable]
public class TenantClientApiPolicyEntity : EntityBasic
{
    /// <summary>
    /// 是否要求请求签名。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否要求请求签名", DefaultValue = "false")]
    public bool SignatureRequired { get; set; }

    /// <summary>
    /// 是否要求设备证明。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否要求设备证明", DefaultValue = "false")]
    public bool DeviceProofRequired { get; set; }

    /// <summary>
    /// 风险策略生效模式。
    /// </summary>
    [SugarColumn(ColumnDescription = "风控策略", Length = EntityStringLength.Length512, IsNullable = true)]
    public string RiskPolicy { get; set; } = string.Empty;

    /// <summary>
    /// 【令牌节】访问令牌签名密钥。服务端内部 HMAC 签发/校验使用，任何人都无需读取明文；
    /// 空白提交时服务端自动生成 64-hex（C368 空值防护语义承载）。列表/详情恒为脱敏占位。
    /// </summary>
    [SugarColumn(ColumnDescription = "访问令牌签名密钥", Length = EntityStringLength.Length512)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// 【令牌节】访问令牌有效期（分钟）。运行时钳制见 ClientApiPlatformDefaults。
    /// </summary>
    [SugarColumn(ColumnDescription = "令牌有效期分钟", DefaultValue = "0")]
    public int LifetimeMinutes { get; set; }

    /// <summary>
    /// 【票据节】票据固定到期时间（Unix 秒）。0 表示永不过期；非 0 值必须晚于当前时间且不超过 30 天（C142 决策）。
    /// </summary>
    [SugarColumn(ColumnDescription = "票据到期时间", DefaultValue = "0")]
    public long TicketExpireTime { get; set; }

    /// <summary>
    /// 【票据节】是否允许首次签发时自动创建租户应用级 RSA key。实际值可被平台默认覆盖。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否允许首次签发自动建Key", DefaultValue = "false")]
    public bool EnableKeyAutoCreate { get; set; } = true;

    /// <summary>
    /// 【输出节】区服列表是否返回 HttpManagerUrl。
    /// </summary>
    [SugarColumn(ColumnDescription = "区服列表返回HttpManager", DefaultValue = "false")]
    public bool EnableReturnHttpManager { get; set; }

    /// <summary>
    /// 【输出节】订单是否启用沙箱直发（测试充值）。
    /// </summary>
    [SugarColumn(ColumnDescription = "订单沙箱直发", DefaultValue = "false")]
    public bool EnableSandBoxRecharge { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}
