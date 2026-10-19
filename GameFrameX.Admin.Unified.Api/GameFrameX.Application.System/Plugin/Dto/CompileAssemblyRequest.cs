namespace GameFrameX.Application.System.Plugin.Dto;

/// <summary>
/// 动态编译 C# 代码输入参数
/// </summary>
public class CompileAssemblyRequest
{
    /// <summary>
    /// C# 源代码
    /// </summary>
    [Required(ErrorMessage = "C# 代码不能为空")]
    public string CsharpCode { get; set; } = string.Empty;

    /// <summary>
    /// 程序集名称，未传时由编译器自动生成
    /// </summary>
    public string? AssemblyName { get; set; }
}
