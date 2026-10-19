namespace GameFrameX.Core.Api;

/// <summary>
/// Admin API 描述设置。
/// <para>该属性只承载项目内稳定的接口名称、分组和排序元数据，不参与 历史框架 动态 API 发现。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiDescriptionSettingsAttribute : Attribute
{
    public ApiDescriptionSettingsAttribute()
    {
    }

    public ApiDescriptionSettingsAttribute(string groupName)
    {
        GroupName = groupName;
    }

    public string? GroupName { get; }

    public string? Module { get; set; }

    public string? Name { get; set; }

    public int Order { get; set; }
}