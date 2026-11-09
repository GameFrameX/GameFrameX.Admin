using System.Text.Json.Serialization;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 游戏玩家账号
/// </summary>
[SugarTable(null, "游戏玩家账号")]
// 首页聚合「今日新增玩家」按 CreatedTime 过滤计数走索引
[SugarIndex("idx_game_user_created_time", nameof(CreatedTime), OrderByType.Asc)]
[BusinessTable]
[IncrementTable]
public class GameUserEntity : EntityBasicSelect
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
    /// 邮箱用户名（绑定项，D-29：除必填项外全部可空）
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱用户名", IsNullable = true)]
    public string? Email { get; set; }

    /// <summary>
    /// 邮箱密码（绑定项）
    /// <para>敏感凭据字段，不参与 JSON 序列化（HTTP 响应不可见），数据库存储不受影响。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱密码", IsNullable = true)]
    [JsonIgnore]
    public string? EmailPassword { get; set; }

    /// <summary>
    /// 账号状态
    /// </summary>
    [SugarColumn(ColumnDescription = "账号状态", DefaultValue = "0")]
    public GameUserStatus Status { get; set; }

    /// <summary>
    /// 手机（绑定项，经 BindPhone / 统一 Unbind type=phone 维护）
    /// </summary>
    [SugarColumn(ColumnDescription = "手机", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", IsNullable = true)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 谷歌账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "谷歌账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? GoogleId { get; set; }

    /// <summary>
    /// 苹果账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "苹果账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? AppleId { get; set; }

    /// <summary>
    /// Facebook账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "Facebook账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? FacebookId { get; set; }

    /// <summary>
    /// Twitter账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "Twitter账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? TwitterId { get; set; }

    /// <summary>
    /// Discord账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "Discord账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? DiscordId { get; set; }

    /// <summary>
    /// QQ账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "QQ账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? QqId { get; set; }

    /// <summary>
    /// Wechat账号ID（绑定项）
    /// </summary>
    [SugarColumn(ColumnDescription = "Wechat账号ID", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? WeChatId { get; set; }

    /// <summary>
    /// 设备标识（A1：匿名设备账号主绑定键，DeviceLogin 按 DeviceId 找号或建号）
    /// </summary>
    [SugarColumn(ColumnDescription = "设备标识", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    [SugarColumn(ColumnDescription = "账号删除时间")]
    public long? DeletedTime { get; set; }
}
