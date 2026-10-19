namespace GameFrameX.Application.System.Dictionary.Dto;

public class QueryDictionaryDataRequest : BaseEnabledRequest
{
    /// <summary>
    /// 编码
    /// </summary>
    [Required(ErrorMessage = "字典唯一编码不能为空")]
    public string Code { get; set; } = string.Empty;
}