namespace GameFrameX.Core.Const;

/// <summary>
/// 声明默认租户职位模板，模板 ID 由职位编码稳定派生。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TenantPositionTemplateAttribute : Attribute
{
    public TenantPositionTemplateAttribute(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; }

    public string Name { get; }

    public int OrderNo { get; set; }

    public string Remark { get; set; } = string.Empty;
}