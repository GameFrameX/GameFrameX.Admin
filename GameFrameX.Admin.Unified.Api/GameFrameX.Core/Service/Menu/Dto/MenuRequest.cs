using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Menu.Dto;

public class MenuRequest
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    public MenuTypeEnum? Type { get; set; }
}