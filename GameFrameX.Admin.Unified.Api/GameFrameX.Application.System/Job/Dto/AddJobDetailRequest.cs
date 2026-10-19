namespace GameFrameX.Application.System.Job.Dto;

public class AddJobDetailRequest : SystemJobDetailEntity
{
    /// <summary>
    /// 作业Id
    /// </summary>
    [Required(ErrorMessage = "作业Id不能为空")]
    [MinLength(2, ErrorMessage = "作业Id不能少于2个字符")]
    public override string JobId { get; set; } = string.Empty;
}