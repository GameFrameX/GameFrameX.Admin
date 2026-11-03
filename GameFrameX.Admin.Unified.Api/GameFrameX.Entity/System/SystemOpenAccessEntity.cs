using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.System;

/// <summary>
/// 开放接口身份表
/// </summary>
[SugarTable(null, "开放接口身份表")]
[PlatformTable]
[IncrementTable]
public class SystemOpenAccessEntity : EntityBasic
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [SugarColumn(ColumnDescription = "身份标识", Length = EntityStringLength.Length256)]
    public virtual string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 密钥
    /// </summary>
    [SugarColumn(ColumnDescription = "密钥", Length = EntityStringLength.Length512)]
    public virtual string AccessSecret { get; set; } = string.Empty;



    /// <summary>
    /// 绑定用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定用户Id", DefaultValue = "0")]
    public virtual long BindUserId { get; set; }
}