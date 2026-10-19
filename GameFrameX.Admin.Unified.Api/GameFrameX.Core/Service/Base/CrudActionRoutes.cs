namespace GameFrameX.Core.Service.Base;

/// <summary>
/// 通用 CRUD Admin API 的公开 action 路由契约。
/// </summary>
public static class CrudActionRoutes
{
    public const string Add = "add";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Page = "page";
    public const string Detail = "detail";
    public const string List = "list";
    public const string BatchDelete = "batchDelete";
    public const string SetStatus = "setStatus";
    public const string Select = "select";
    public const string SelectString = "selectString";
    public const string Export = "export";
}