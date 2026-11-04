namespace GameFrameX.Application.Platform.Dictionary.Dto;

public class DictionaryDataRequest : BaseIdRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}