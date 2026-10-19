namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 基础更新输入参数契约。
/// </summary>
public interface ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
    /// </summary>
    long? RowVersion { get; set; }
}