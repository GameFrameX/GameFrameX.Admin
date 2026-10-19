using GameFrameX.Entity.Global;
using GameFrameX.Core.Service.Menu.Dto;
using GameFrameX.Entity.Enum;

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
              .Map(t => t.Meta.AppScope, o => ScopeCode(o.AppScope))
              .Map(t => t.Meta.MenuContext, o => ContextCode(o.AppScope));
    }

    /// <summary>
    /// AppScope 枚举 → 前端约定字符串（platform / tenant / app）。
    /// </summary>
    private static string ScopeCode(MenuAppScopeEnum scope)
    {
        return scope switch
        {
            MenuAppScopeEnum.Platform => "platform",
            MenuAppScopeEnum.App      => "app",
            _                         => "tenant",
        };
    }

    /// <summary>
    /// AppScope 枚举 → MenuContext 字符串（tenantShell / appWorkspace）。
    /// 平台域页面不切换菜单上下文：侧边栏在平台管理子页下保持完整菜单（platform 子菜单就地展开），
    /// 仅应用工作台（App 域）切换为 appWorkspace 上下文。
    /// </summary>
    private static string ContextCode(MenuAppScopeEnum scope)
    {
        return scope switch
        {
            MenuAppScopeEnum.App => "appWorkspace",
            _                    => "tenantShell",
        };
    }
}