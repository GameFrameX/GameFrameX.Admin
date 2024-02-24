namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本参数对象
/// </summary>
public class ClientAssetPackageVersionDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Channel { get; set; }
        
    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; }
        
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string AppVersion { get; set; }
        
    /// <summary>
    /// 包名
    /// </summary>
    public string Package { get; set; }
        
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; }
        
    /// <summary>
    /// 资源包名称
    /// </summary>
    public string AssetPackageName { get; set; }
        
    /// <summary>
    /// 资源版本
    /// </summary>
    public string Version { get; set; }
        
    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; }
        
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
        
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
        
}
