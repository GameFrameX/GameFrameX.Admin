using GameFrameX.Client.Api.GameUserArchive;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// Game.Api 路由守护（C8 T4b 前置修复的锁定测试）。
/// <para>Game.Api 不走 Admin 动态 API 路由，Controller 必须带类级
/// <c>[Route("api/[controller]/[action]")]</c>——T3 三个 Controller 均漏加（单测直接
/// new 调方法不经过路由层，掩盖到 T4b 联调才暴露为 404）。本测试锁两层：
/// 类级 Route 存在 + 公开 action 均有 HTTP 谓词特性，缺一即红。</para>
/// </summary>
public class GameApiControllerRoutingTests
{
    [Fact]
    public void AllControllers_HaveClassLevelRoute()
    {
        var assembly = typeof(GameUserArchiveController).Assembly;
        var controllers = assembly.GetExportedTypes()
                                  .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                                  .ToList();
        Assert.NotEmpty(controllers);

        var missing = controllers
                      .Where(t => t.GetCustomAttribute<RouteAttribute>() == null)
                      .Select(t => t.Name)
                      .ToList();
        Assert.True(missing.Count == 0,
                    "以下 Controller 缺类级 [Route]（MapControllers 下无法路由，运行时 404）：" + string.Join(", ", missing));
    }

    [Fact]
    public void AllPublicActions_HaveHttpVerbAttribute()
    {
        var assembly = typeof(GameUserArchiveController).Assembly;
        var controllers = assembly.GetExportedTypes()
                                  .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        var missing = new List<string>();
        foreach (var controller in controllers)
        {
            var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var action in actions)
            {
                var hasVerb = action.GetCustomAttribute<HttpPostAttribute>() != null
                              || action.GetCustomAttribute<HttpGetAttribute>() != null
                              || action.GetCustomAttribute<HttpPutAttribute>() != null;
                if (!hasVerb)
                {
                    missing.Add(controller.Name + "." + action.Name);
                }
            }
        }

        Assert.True(missing.Count == 0,
                    "以下公开 action 缺 HTTP 谓词特性（默认 GET 约定与 POST-only 契约不符）：" + string.Join(", ", missing));
    }
}