using GameFrameX.Entity.Global;
using GameFrameX.Core.Service.Menu.Dto;

namespace GameFrameX.Core.Service.Menu;

/// <summary>
/// 配置菜单对象映射
/// </summary>
public class SystemMenuMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<GlobalMenuEntity, MenuResponse>()
              .Map(t => t.Meta.Title, o => o.Title)
              .Map(t => t.Meta.TitleCode, o => o.TitleCode)
              .Map(t => t.Meta.Icon, o => o.Icon)
              .Map(t => t.Meta.IsIframe, o => o.IsIframe)
              .Map(t => t.Meta.IsLink, o => o.OutLink)
              .Map(t => t.Meta.IsHide, o => o.IsHide)
              .Map(t => t.Meta.IsKeepAlive, o => o.IsKeepAlive)
              .Map(t => t.Meta.IsAffix, o => o.IsAffix)
              .Map(t => t.Meta.MenuContext, o => "console");
    }

}
