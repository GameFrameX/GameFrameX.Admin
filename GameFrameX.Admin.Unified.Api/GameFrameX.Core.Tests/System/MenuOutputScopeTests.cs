using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Menu.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using Mapster;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 映射器将 global_menu.app_scope 枚举直传为 meta.appScope，并派生 meta.menuContext。
/// 路径前缀推断已删除，作用域是显式数据列 global_menu.app_scope（由种子 GlobalMenuSeedData.* 显式声明）。
/// </summary>
public class MenuOutputScopeTests
{
    [Fact]
    public void MenuOutputMeta_UsesConsoleContext()
    {
        var config = new TypeAdapterConfig();
        new SystemMenuMapper().Register(config);

        var output = new GlobalMenuEntity { AppScope = MenuAppScopeEnum.Console, }.Adapt<MenuResponse>(config);

        Assert.Equal("console", output.Meta.AppScope);
        Assert.Equal("console", output.Meta.MenuContext);
    }
}