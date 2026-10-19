using System.Text.Json.Serialization;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏玩家账号
/// </summary>
[SugarTable(null, "游戏玩家账号")]
// 首页聚合「今日新增玩家」按 CreatedTime 过滤计数走索引（C321 / GFX-730）
[SugarIndex("idx_tenant_game_user_created_time", nameof(CreatedTime), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameUserEntity : EntityBasicSelect
{
    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnDescription = "用户名")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// <para>敏感凭据字段，不参与 JSON 序列化（HTTP 响应不可见），数据库存储不受影响。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "密码")]
    [JsonIgnore]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 邮箱用户名
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱用户名")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 邮箱密码
    /// <para>敏感凭据字段，不参与 JSON 序列化（HTTP 响应不可见），数据库存储不受影响。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱密码")]
    [JsonIgnore]
    public string EmailPassword { get; set; } = null!;

    /// <summary>
    /// 账号状态
    /// </summary>
    [SugarColumn(ColumnDescription = "账号状态", DefaultValue = "0")]
    public GameUserStatus Status { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarColumn(ColumnDescription = "手机", Length = EntityStringLength.Length256)]
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像")]
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 谷歌账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "谷歌账号ID", Length = EntityStringLength.Length2048)]
    public string GoogleId { get; set; } = null!;

    /// <summary>
    /// 苹果账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "苹果账号ID", Length = EntityStringLength.Length2048)]
    public string AppleId { get; set; } = null!;

    /// <summary>
    /// Facebook账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Facebook账号ID", Length = EntityStringLength.Length2048)]
    public string FacebookId { get; set; } = null!;

    /// <summary>
    /// Twitter账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Twitter账号ID", Length = EntityStringLength.Length2048)]
    public string TwitterId { get; set; } = null!;

    /// <summary>
    /// Discord账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Discord账号ID", Length = EntityStringLength.Length2048)]
    public string DiscordId { get; set; } = null!;

    /// <summary>
    /// QQ账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "QQ账号ID", Length = EntityStringLength.Length2048)]
    public string QqId { get; set; } = null!;

    /// <summary>
    /// Wechat账号ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Wechat账号ID", Length = EntityStringLength.Length2048)]
    public string WeChatId { get; set; } = null!;

    /// <summary>
    /// 设备标识（C12 A1：匿名设备账号主绑定键，DeviceLogin 按 DeviceId 找号或建号）
    /// </summary>
    [SugarColumn(ColumnDescription = "设备标识", Length = EntityStringLength.Length2048)]
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// 删除时间
    /// </summary>
    [SugarColumn(ColumnDescription = "账号删除时间")]
    public long? DeletedTime { get; set; }
}