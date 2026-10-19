using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// App 会话密钥（AES-256-GCM 加密信封对称密钥环，change C5 server-envelope-middleware）。
/// <para>形状照 <see cref="TenantAppSignatureKeyEntity"/>（C2 密钥环先例）：Kid 版本化寻址、NotBefore/Expire 租约、
/// Rotate 双活、禁删保护；区别在算法为 AES-GCM 信封密钥（会话层，双向 c2s/s2c HKDF 分钥）而非 HMAC 签名密钥。</para>
/// <para><b>归属：</b>AppId 归属某个具体应用（IAppScoped），密钥域 = (租户, 应用, Kid)；同一应用可同时存在多把生效 key，
/// 客户端信封自报 kid 定密钥（双活轮换窗口内新旧 key 共存）。</para>
/// </summary>
[SugarTable(null, "App 会话密钥")]
[TenantTable]
[IncrementTable]
public class TenantSessionKeyEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 ID。密钥域归属：解密时按 (AppId, Kid) 定位会话密钥，防止跨应用挪用信封。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用 ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// Key ID（如 sess-app-1001-20260818120000-a1b2），进入信封 JSON 的 kid 字段。
    /// <para>字符白名单 [A-Za-z0-9._-]{1,64}（信封受控 JSON 契约），客户端固定用某 Kid 加密，
    /// 服务端按信封自报 kid 精确择 key，实现多版本客户端各自持不同 key 共存。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "Key ID", Length = EntityStringLength.Length256)]
    public string Kid { get; set; } = string.Empty;

    /// <summary>
    /// 会话密钥（明文存储，32 字节 CSPRNG 的 64 位 hex）。
    /// <para><b>为何明文：</b>AES-GCM 是对称算法，解密端必须持与加密端完全相同的密钥，
    /// 单向 hash 无法还原密钥参与计算（同 C2 签名密钥与 OpenAccess 理由）。缓解措施：管理面列表/详情一律脱敏 ******，
    /// 仅 Add/Rotate 响应一次性明文回显，且实体禁删（见管理面 Delete 保护）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "会话密钥（明文 hex，对称算法解密必需）", Length = EntityStringLength.Length1024)]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// 租约生效时间（Unix 秒）。null 表示无生效下限；生效条件为 NotBefore ≤ now 且 now &lt; Expire。
    /// <para>未到 NotBefore 的 key 拒用（60105），防止提前泄出的密钥被使用。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "生效时间", IsNullable = true)]
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 租约到期时间（Unix 秒）。null 表示永不过期；到期后 key 被查询排除，信封解密拒收（60105）。
    /// <para>到期退场是 Rotate 轮换的主要路径：新 key 插入时给旧 key 设到期日，到点自动失活。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间", IsNullable = true)]
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 被轮换标记时间（Unix 秒）；null 表示从未被轮换。纯元数据，不参与解密判定。
    /// <para>Rotate 时给应用当前最新生效 key 打此标记，便于审计追溯；被标记 key 在租约内仍可解密（双活窗口）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "被轮换时间", IsNullable = true)]
    public long? RotatedTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}