namespace GameFrameX.Core.Export;

/// <summary>
/// 标记导出敏感字段。被标记的属性即使在导出请求的列白名单中，
/// 后端也会强制剔除，确保敏感数据（密钥、手机号、余额等）永不进入导出文件。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ExportSensitiveAttribute : Attribute
{
}