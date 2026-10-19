namespace GameFrameX.Client.Api.GameArea.Dto;

/// <summary>
/// 游戏大区
/// </summary>
public sealed class TenantGameAreaClassifyEntityResponse
{
    /// <summary>
    /// 大区Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 大区Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// 大区Id
    /// </summary>
    public List<TenantGameAreaEntityResponse> GameAreaList { get; set; } = null!;
}