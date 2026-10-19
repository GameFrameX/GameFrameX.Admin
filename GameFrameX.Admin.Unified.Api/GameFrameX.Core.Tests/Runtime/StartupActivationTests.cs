using GameFrameX.Web.Api;

namespace GameFrameX.Core.Tests.Runtime;

/// <summary>
/// C41：原生 Startup 仍支持无参构造激活。
/// </summary>
public class StartupActivationTests
{
    [Fact]
    public void Startup_HasParameterlessConstructorForNativeActivation()
    {
        var startup = Activator.CreateInstance(typeof(Startup));

        Assert.NotNull(startup);
    }
}