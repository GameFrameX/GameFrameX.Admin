// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

namespace GameFrameX.Entity.Client;

[SugarTable(null, "客户端资源包版本")]
[SysTable]
[IncreTable]
public class ClientAssetPackageVersion : EntityBase
{
    /// <summary>
    /// 渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道名称", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Channel { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    [SugarColumn(ColumnDescription = "平台", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Platform { get; set; }

    /// <summary>
    /// 游戏版本
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏版本", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string AppVersion { get; set; }


    /// <summary>
    /// 包名
    /// </summary>
    [SugarColumn(ColumnDescription = "包名", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Package { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    [SugarColumn(ColumnDescription = "语言", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Language { get; set; }

    /// <summary>
    /// 资源包名称
    /// </summary>
    [SugarColumn(ColumnDescription = "资源包名称", Length = 32)]
    [Required, MaxLength(32)]
    public virtual string AssetPackageName { get; set; }

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    [SugarColumn(ColumnDescription = "资源下载根路径", Length = 128)]
    [Required, MaxLength(128)]
    public virtual string RootPath { get; set; }

    /// <summary>
    /// 资源版本
    /// </summary>
    [SugarColumn(ColumnDescription = "资源版本", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Version { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = 512)]
    [MaxLength(512)]
    public string Description { get; set; }
}