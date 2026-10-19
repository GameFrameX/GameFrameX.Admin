namespace GameFrameX.Application.System.Dictionary.Dto;

public class GetDataDictionaryDataRequest
{
    /// <summary>
    /// 字典类型Id
    /// </summary>
    [Required(ErrorMessage = "字典类型Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "字典类型Id必须大于0")]
    public long DictionaryTypeId { get; set; }
}