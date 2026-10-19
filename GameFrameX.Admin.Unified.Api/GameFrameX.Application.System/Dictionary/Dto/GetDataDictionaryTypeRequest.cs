namespace GameFrameX.Application.System.Dictionary.Dto;

public class GetDataDictionaryTypeRequest
{
    /// <summary>
    /// 编码
    /// </summary>
    [Required(ErrorMessage = "字典类型编码不能为空")]
    public string Code { get; set; } = string.Empty;
}