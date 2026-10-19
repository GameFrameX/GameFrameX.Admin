namespace GameFrameX.Core.Service.File.Dto;

/// <summary>
/// 系统文件输出参数
/// </summary>
public class TenantFileResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 提供者
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 关联的租户对象存储配置 ID（TenantObjectStorageConfigEntity.Id）。
    /// 新上传文件必填，用于按精确配置解析下载/删除路径；历史记录为 null 时按本地 legacy 行为兜底。
    /// </summary>
    public long? StorageConfigId { get; set; }

    /// <summary>
    /// 仓储名称
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// 文件名称（源文件名）
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// 存储路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小KB
    /// </summary>
    public string? SizeKb { get; set; }

    /// <summary>
    /// 文件大小信息-计算后的
    /// </summary>
    public string? SizeInfo { get; set; }

    /// <summary>
    /// 外链地址-OSS上传后生成外链地址方便前端预览
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 文件MD5
    /// </summary>
    public string? FileMd5 { get; set; }

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