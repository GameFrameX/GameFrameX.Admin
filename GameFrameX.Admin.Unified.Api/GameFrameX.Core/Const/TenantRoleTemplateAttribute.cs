using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Const;

/// <summary>
/// 声明默认租户角色模板，菜单授权由角色编码映射到租户菜单同步预设派生。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TenantRoleTemplateAttribute : Attribute
{
    public TenantRoleTemplateAttribute(string code, string name, DataScopeEnum dataScope, TenantMenuSyncRole syncRole)
    {
        Code = code;
        Name = name;
        DataScope = dataScope;
        SyncRole = syncRole;
    }

    public string Code { get; }

    public string Name { get; }

    public DataScopeEnum DataScope { get; }

    public TenantMenuSyncRole SyncRole { get; }

    public int OrderNo { get; set; }

    public string Remark { get; set; } = string.Empty;
}