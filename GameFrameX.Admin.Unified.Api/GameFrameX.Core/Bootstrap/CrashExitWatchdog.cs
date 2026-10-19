namespace GameFrameX.Core.Bootstrap
{
    /// <summary>
    /// 崩溃退出兜底 watchdog（C381 / GFX-873）。
    /// <para>背景：.NET 10 linux/arm64 容器存在运行时异常传播路径僵死——逃逸 Program.Main 的未处理异常
    /// 不终止进程，主线程 100% CPU 卡在异常 unwind 且运行时崩溃 banner 从未打印，容器 restart 策略
    /// 依赖进程退出而永不触发（一次瞬态崩溃升级为永久 unhealthy）。本组件在崩溃路径上武装超时强退：
    /// 宽限期内进程未自行终止（正常崩溃传播毫秒级完成）则由专用后台线程直写 stderr 崩溃证据后
    /// <see cref="Environment.FailFast(string)"/> 强退，保证容器重启策略可用。</para>
    /// <para>设计依据：专用线程而非线程池——运行时僵死态下线程池线程不可依赖；
    /// FailFast 而非 Exit——Exit 需执行 ProcessExit / finally / finalizer，僵死态可能再次卡死。</para>
    /// <para>三个武装信号（2026-09-27 最小宿主探针实测，notes.md）：①Program.Main 的 catch（异常路径）；
    /// ②IHostApplicationLifetime.ApplicationStopped（StopHost / BackgroundService 启动失败时
    /// Run 正常返回、无异常，仅此信号可覆盖该类僵死）；③AppDomain.UnhandledException
    /// （后台线程致命异常 + 主线程崩溃的打印阶段）。启动失败（host 从未 NotifyStarted）时
    /// lifetime token 不触发，②③互补兜底。</para>
    /// </summary>
    public sealed class CrashExitWatchdog
    {
        /// <summary>默认强退宽限期（15 秒）：覆盖正常崩溃传播与停机收尾（毫秒级），仅截断病态僵死。</summary>
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private readonly ManualResetEventSlim _disarmed = new ManualResetEventSlim(false);
        private readonly Action<string> _termination;
        private readonly Thread _watchdogThread;

        /// <summary>强退时输出的崩溃证据消息（reason + 异常全文），stderr 直写，不经应用日志管道。</summary>
        public string TerminationMessage { get; }

        /// <summary>
        /// 武装一道崩溃退出兜底：宽限期 <paramref name="timeout"/> 内进程未终止（未 <see cref="Disarm"/>）
        /// 则执行 <paramref name="termination"/>。
        /// </summary>
        /// <param name="terminationMessage">强退时输出的崩溃证据消息。</param>
        /// <param name="timeout">强退宽限期。</param>
        /// <param name="termination">终止动作；缺省为 stderr 直写 + <see cref="Environment.FailFast(string)"/>，测试注入替身以免杀死测试进程。</param>
        public CrashExitWatchdog(string terminationMessage, TimeSpan timeout, Action<string>? termination = null)
        {
            TerminationMessage = terminationMessage;
            _termination = termination ?? DefaultTermination;
            _watchdogThread = new Thread(Watch)
            {
                IsBackground = true,
                Name = "GameFrameX.CrashExitWatchdog",
            };
            _watchdogThread.Start(timeout);
        }

        /// <summary>
        /// 崩溃路径武装入口（信号 ①/②共用）：武装默认宽限期的 watchdog。
        /// 信号 ① 调用方捕获逃逸 <c>app.Run()</c> 的异常后先 Arm 再原样重抛——重抛保留默认崩溃语义
        /// （banner + 退出码 + 运行时 dump），watchdog 仅在默认路径未在宽限期内终止进程时介入；
        /// 信号 ② 在 <c>ApplicationStopped</c> 触发时调用（host 已停，进程理应随即退出）。
        /// </summary>
        /// <param name="reason">崩溃原因描述（host 名 + 场景），写入强退 marker。</param>
        /// <param name="exception">逃逸的未处理异常，全文写入强退 marker（僵死时运行时 banner 可能永远打不出来，这里是唯一落盘证据）；无异常对象时传 null。</param>
        public static CrashExitWatchdog Arm(string reason, Exception? exception = null)
        {
            var terminationMessage = "[CrashExitWatchdog] " + reason + "，进程在 " + DefaultTimeout.TotalSeconds
                + " 秒内未自行终止（判定为崩溃/停机路径僵死），强制退出以恢复容器重启策略。" + Environment.NewLine;
            if (exception != null)
            {
                terminationMessage += "Exception: " + exception + Environment.NewLine;
            }

            return new CrashExitWatchdog(terminationMessage, DefaultTimeout);
        }

        /// <summary>
        /// 崩溃路径武装入口（信号 ③）：进程级注册 <see cref="AppDomain.UnhandledException"/> 兜底——
        /// 后台线程致命异常与主线程崩溃的打印阶段（异常到达线程顶、运行时 fatal report 尚未输出）在此武装。
        /// 三 host Main 最先调用（早于任何可能崩溃的构建代码）；事件仅在异常即将终止进程时触发，无健康路径副作用。
        /// </summary>
        public static void EnableDomainUnhandledExceptionFallback()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                Arm("AppDomain 未处理异常（后台线程或打印阶段兜底）", eventArgs.ExceptionObject as Exception);
            };
        }

        /// <summary>解除武装：宽限期内进程将正常退出 / 测试清理时调用，watchdog 不再触发终止动作。</summary>
        public void Disarm()
        {
            _disarmed.Set();
        }

        private void Watch(object? timeout)
        {
            if (_disarmed.Wait((TimeSpan)timeout!))
            {
                return;
            }

            _termination(TerminationMessage);
        }

        private static void DefaultTermination(string terminationMessage)
        {
            // 崩溃兜底路径：stderr 直写崩溃证据（容器管道可能已关闭，写失败不得拦住强退）
            try
            {
                Console.Error.WriteLine(terminationMessage);
                Console.Error.Flush();
            }
            catch
            {
                // 忽略写出失败：FailFast 强退优先
            }

            Environment.FailFast(terminationMessage);
        }
    }
}
