using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Const.MenuSeed;

/// <summary>
/// 菜单和按钮的源码定义。业务键是唯一标识，数值主键由业务键稳定派生，避免在代码里手写菜单 ID。
/// </summary>
public sealed record MenuSeedDefinition(
    string Key,
    string? ParentKey,
    MenuTypeEnum Type,
    string? Name,
    string? Path,
    string? Component,
    string? Redirect,
    string? Permission,
    string Title,
    string? TitleCode,
    string? Icon,
    string? OutLink,
    bool IsIframe,
    bool IsHide,
    bool IsKeepAlive,
    bool IsAffix,
    int OrderNo,
    bool IsEnabled,
    string? Remark,
    long CreatedTime,
    long UpdateTime,
    TenantMenuSyncRole SyncRoles,
    bool SuperAdminOnly,
    bool AllowTenantWhenSuperAdminOnly,
    MenuAppScopeEnum AppScope);