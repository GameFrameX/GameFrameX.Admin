namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包参数对象
/// </summary>
public class ClientAssetPackageDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
        
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
