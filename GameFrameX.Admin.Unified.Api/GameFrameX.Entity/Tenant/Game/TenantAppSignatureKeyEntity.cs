using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// App 传输签名密钥（HMAC-SHA256 对称密钥环，change C2 sig-keyring-server）。
/// <para>形状照 <see cref="TenantGameLoginTicketKeyEntity"/> 体系：Kid 版本化寻址、NotBefore/Expire 租约、
/// Rotate 双活、禁删保护；区别在算法为 HMAC 对称密钥（LoginTicket 为 RSA 非对称）。</para>
/// <para><b>归属：</b>AppId 归属某个具体应用（IAppScoped），密钥域 = (租户, 应用)；同一应用可同时存在多把生效 key，
/// 单 key 失效（禁用/到期）仅拒用该 key 签名的请求，不影响其余 key（多版本客户端共存的基础）。</para>
/// </summary>
[SugarTable(null, "App 传输签名密钥")]
[TenantTable]
[IncrementTable]
public class TenantAppSignatureKeyEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 ID。密钥域归属：验签时按 (AppId, Kid) 定位密钥，防止跨应用挪用签名。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用 ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// Key ID（如 sk-app-1001-20260817120000-a1b2），进入 v2 签名头与 message 串。
    /// <para>人可读、wire 稳定，对齐 LoginTicket Kid 的版本化风格；客户端固定用某 Kid 签名，
    /// 服务端按 Kid 精确择 key，实现多版本客户端各自持不同 key 共存。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "Key ID", Length = EntityStringLength.Length256)]
    public string Kid { get; set; } = string.Empty;

    /// <summary>
    /// HMAC 对称密钥（明文存储）。
    /// <para><b>为何明文：</b>HMAC 是对称算法，验签端必须持与签名端完全相同的密钥重算 MAC，
    /// 单向 hash（如 AppSecret 的 PBKDF2 模式）无法还原出密钥参与计算，因此本字段不可照 AppSecret 做 hash-only 落库
    /// （同 OpenAccess 理由）。缓解措施：管理面列表/详情一律脱敏为 ******，仅 Add/Rotate 响应中一次性明文回显，
    /// 且实体禁删（见管理面 Delete 保护）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "HMAC 密钥（明文，对称算法验签必需）", Length = EntityStringLength.Length1024)]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// 租约生效时间（Unix 秒）。null 表示无生效下限；生效条件为 NotBefore ≤ now 且 now &lt; Expire。
    /// <para>新 key 可预设未来生效时间实现定时切换；未到 NotBefore 的 key 拒签（60103），防止提前泄出的密钥被使用。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "生效时间", IsNullable = true)]
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 租约到期时间（Unix 秒）。null 表示永不过期；到期后 key 被查询自然排除，拒签（60103）。
    /// <para>到期退场是 Rotate 轮换的主要路径：新 key 插入时给旧 key 设一个到期日，到点自动失活，无需人工干预。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间", IsNullable = true)]
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 被轮换标记时间（Unix 秒）；null 表示从未被轮换。纯元数据，不参与验签判定。
    /// <para>Rotate 时给应用当前最新生效 key 打此标记，便于审计追溯「这把 key 是何时让位给新 key 的」；
    /// 被标记的 key 在租约/启用条件满足前仍可验签（双活窗口），是否退场只由 IsEnabled 与租约决定。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "被轮换时间", IsNullable = true)]
    public long? RotatedTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}