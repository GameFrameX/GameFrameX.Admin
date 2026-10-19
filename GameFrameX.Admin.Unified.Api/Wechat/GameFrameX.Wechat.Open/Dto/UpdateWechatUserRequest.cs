namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 更新微信用户输入参数（乐观锁依赖 RowVersion，未携带时跳过版本比对）
/// </summary>
public class UpdateWechatUserRequest : AddWechatUserRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改
    /// </summary>
    public long? RowVersion { get; set; }
}
