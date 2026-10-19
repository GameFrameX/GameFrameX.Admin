namespace GameFrameX.Application.System.Region.Dto;

/// <summary>
/// 系统行政地区输出参数
/// </summary>
public class SystemRegionResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 简称
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// 组合名
    /// </summary>
    public string? MergerName { get; set; }

    /// <summary>
    /// 行政代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// 区号
    /// </summary>
    public string? CityCode { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 拼音
    /// </summary>
    public string? PinYin { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public float Lng { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    public float Lat { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}