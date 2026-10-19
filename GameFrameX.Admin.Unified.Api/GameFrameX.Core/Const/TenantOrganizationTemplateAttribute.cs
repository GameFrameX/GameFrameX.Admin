namespace GameFrameX.Core.Const;

/// <summary>
/// 声明默认租户组织模板，模板 ID 由组织业务键稳定派生。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TenantOrganizationTemplateAttribute : Attribute
{
    public TenantOrganizationTemplateAttribute(string key, string name, string code, string type)
    {
        Key = key;
        Name = name;
        Code = code;
        Type = type;
    }

    public string Key { get; }

    public string? ParentKey { get; set; }

    public string Name { get; }

    public string Code { get; }

    public string Type { get; }

    public int Level { get; set; }

    public int OrderNo { get; set; }

    public string Remark { get; set; } = string.Empty;
}