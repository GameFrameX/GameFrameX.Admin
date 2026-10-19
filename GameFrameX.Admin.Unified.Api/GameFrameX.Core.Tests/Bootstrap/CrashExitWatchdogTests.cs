using GameFrameX.Core.Bootstrap;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// CrashExitWatchdog 契约测试（C381 / GFX-873）：未处理异常在运行时传播路径僵死时，
/// 宽限期内进程未终止则由 watchdog 强退，保证容器 restart 策略可触发。
/// <para>测试一律注入终止动作——真实默认动作是 Environment.FailFast，会杀死测试进程。</para>
/// </summary>
public sealed class CrashExitWatchdogTests
{
    [Fact]
    public void Watchdog_WithoutDisarm_InvokesTerminationAfterTimeout()
    {
        using (var terminationInvoked = new ManualResetEventSlim(false))
        {
            string? terminatedMessage = null;
            var watchdog = new CrashExitWatchdog("测试强退原因 expected-crash-reason", TimeSpan.FromMilliseconds(200), message =>
            {
                terminatedMessage = message;
                terminationInvoked.Set();
            });

            Assert.True(terminationInvoked.Wait(TimeSpan.FromSeconds(10)), "宽限期结束后必须触发终止动作");
            Assert.NotNull(terminatedMessage);
            Assert.Contains("expected-crash-reason", terminatedMessage);
        }
    }

    [Fact]
    public void Watchdog_DisarmedBeforeTimeout_DoesNotInvokeTermination()
    {
        using (var terminationInvoked = new ManualResetEventSlim(false))
        {
            var watchdog = new CrashExitWatchdog("不应触发强退", TimeSpan.FromMilliseconds(300), _ => terminationInvoked.Set());

            watchdog.Disarm();

            Assert.False(terminationInvoked.Wait(TimeSpan.FromSeconds(2)), "Disarm 后不得触发终止动作");
        }
    }

    [Fact]
    public void Arm_BuildsTerminationMessageContainingReasonAndException()
    {
        var watchdog = CrashExitWatchdog.Arm("arm-入口测试原因", new InvalidOperationException("arm-入口测试异常消息"));

        try
        {
            Assert.Contains("arm-入口测试原因", watchdog.TerminationMessage);
            Assert.Contains("arm-入口测试异常消息", watchdog.TerminationMessage);
            Assert.Contains("InvalidOperationException", watchdog.TerminationMessage);
        }
        finally
        {
            // 防止默认终止动作（FailFast）在测试进程内被武装
            watchdog.Disarm();
        }
    }

    [Fact]
    public void Arm_WithoutException_BuildsMessageContainingReasonOnly()
    {
        // 信号②（ApplicationStopped）武装时无异常对象——host 已停、进程未退出即强退
        var watchdog = CrashExitWatchdog.Arm("host-已停止-信号测试原因");

        try
        {
            Assert.Contains("host-已停止-信号测试原因", watchdog.TerminationMessage);
            Assert.DoesNotContain("Exception:", watchdog.TerminationMessage);
        }
        finally
        {
            watchdog.Disarm();
        }
    }
}
