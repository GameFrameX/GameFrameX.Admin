using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.Service.TenantMenuSync.Dto;

/// <summary>
/// 按套餐手动同步其全部租户菜单请求
/// </summary>
public class SyncTenantMenuByPackageRequest
{
    /// <summary>
    /// 套餐Id
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "套餐Id必须大于0")]
    public long PackageId { get; set; }
}
