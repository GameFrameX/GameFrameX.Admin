using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.Service.TenantMenuSync.Dto;

/// <summary>
/// 手动同步单个租户菜单请求
/// </summary>
public class SyncTenantMenuRequest
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "租户Id必须大于0")]
    public long TenantId { get; set; }
}
