using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Menu.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using Mapster;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 映射器派生 meta.menuContext。
/// </summary>
public class MenuOutputScopeTests
{
    [Fact]
    public void MenuOutputMeta_UsesConsoleContext()
    {
        var config = new TypeAdapterConfig();
        new SystemMenuMapper().Register(config);

        var output = new GlobalMenuEntity { }.Adapt<MenuResponse>(config);

        Assert.Equal("console", output.Meta.MenuContext);
    }
}