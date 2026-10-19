using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统文件表
/// </summary>
[SugarTable(null, "租户库文件表")]
[TenantTable]
[IncrementTable]
public class TenantFileEntity : EntityBasic
{
    /// <summary>
    /// 提供者
    /// </summary>
    [SugarColumn(ColumnDescription = "提供者", Length = EntityStringLength.Length256)]
    public string? Provider { get; set; }

    /// <summary>
    /// 关联的租户对象存储配置 ID（TenantObjectStorageConfigEntity.Id）。
    /// 新上传文件必填，用于按精确配置解析下载/删除路径；历史记录为 null 时按本地 legacy 行为兜底。
    /// </summary>
    [SugarColumn(ColumnDescription = "对象存储配置ID", IsNullable = true)]
    public long? StorageConfigId { get; set; }

    /// <summary>
    /// 仓储名称
    /// </summary>
    [SugarColumn(ColumnDescription = "仓储名称", Length = EntityStringLength.Length256)]
    public string? BucketName { get; set; }

    /// <summary>
    /// 文件名称（源文件名）
    /// </summary>
    [SugarColumn(ColumnDescription = "文件名称", Length = EntityStringLength.Length256)]
    public string? FileName { get; set; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    [SugarColumn(ColumnDescription = "文件后缀", Length = EntityStringLength.Length256)]
    public string? Suffix { get; set; }

    /// <summary>
    /// 存储路径
    /// </summary>
    [SugarColumn(ColumnDescription = "存储路径", Length = EntityStringLength.Length256)]
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小KB
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小KB", Length = EntityStringLength.Length256)]
    public string? SizeKb { get; set; }

    /// <summary>
    /// 文件大小信息-计算后的
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小信息", Length = EntityStringLength.Length256)]
    public string? SizeInfo { get; set; }

    /// <summary>
    /// 外链地址-OSS上传后生成外链地址方便前端预览
    /// </summary>
    [SugarColumn(ColumnDescription = "外链地址", Length = EntityStringLength.Length1024)]
    public string? Url { get; set; }

    /// <summary>
    /// 文件MD5
    /// </summary>
    [SugarColumn(ColumnDescription = "文件MD5", Length = EntityStringLength.Length256)]
    public string? FileMd5 { get; set; }
}