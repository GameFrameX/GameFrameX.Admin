namespace GameFrameX.Application.System.Plugin.Dto;

/// <summary>
/// 动态程序集编译结果
/// </summary>
public class CompileAssemblyResponse
{
    /// <summary>
    /// 编译生成的动态程序集名称；更新/移除插件时按该名称定位并卸载对应程序集
    /// </summary>
    public string AssemblyName { get; set; } = null!;
}