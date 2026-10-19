using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Export;

/// <summary>
/// 导出范围枚举。
/// </summary>
[AdminEnum]
public enum ExportRangeEnum
{
    /// <summary>全量导出，受单次硬上限保护，避免同步导出阻塞请求。</summary>
    All = 0,

    /// <summary>仅导出当前分页。</summary>
    CurrentPage = 1,
}