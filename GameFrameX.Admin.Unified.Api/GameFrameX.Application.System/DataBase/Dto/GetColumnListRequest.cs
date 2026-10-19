using GameFrameX.Core.Extension.Const;

namespace GameFrameX.Application.System.DataBase.Dto;

/// <summary>
/// 获取字段列表输入参数
/// </summary>
public class GetColumnListRequest
{
    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// ConfigId，未传时默认主库
    /// </summary>
    public string ConfigId { get; set; } = SqlSugarConst.MainConfigId;
}
