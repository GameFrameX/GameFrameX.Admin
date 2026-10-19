namespace GameFrameX.Application.System.Region.Dto;

/// <summary>
/// 新增/编辑行政地区输入参数。
/// 显式声明前端表单可填字段（与前端 beforeSubmit 白名单一致），不再继承 <see cref="SystemRegionEntity"/>，避免种子数据字段（ShortName、MergerName、ZipCode、Level、PinYin、Lng、Lat）与子节点导航属性被前端批量赋值。
/// </summary>
public class AddRegionRequest
{
    /// <summary>
    /// 主键Id（新增为 0，编辑由前端回填）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 行政代码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 区号
    /// </summary>
    public string CityCode { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public long? RowVersion { get; set; }
}