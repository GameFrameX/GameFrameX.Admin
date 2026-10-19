namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// <see cref="BindingDriftCategory"/> 的严重级别查表。
/// </summary>
public static class BindingDriftSeverityMap
{
    /// <summary>
    /// 返回指定类别的稳定严重级别。
    /// </summary>
    public static BindingDriftSeverity Of(BindingDriftCategory category)
    {
        switch (category)
        {
            case BindingDriftCategory.LiveEndpointMissingFromDb:
            case BindingDriftCategory.MenuButtonPermissionOrphan:
            case BindingDriftCategory.DuplicateEndpointKey:
            {
                return BindingDriftSeverity.Warning;
            }
            case BindingDriftCategory.DbOrphanStillEnabled:
            case BindingDriftCategory.NonPublicEndpointWithoutBinding:
            case BindingDriftCategory.BindingToMissingMenu:
            case BindingDriftCategory.BindingToMissingEndpoint:
            {
                return BindingDriftSeverity.Error;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(category), category, "未知的绑定漂移类别。");
            }
        }
    }
}