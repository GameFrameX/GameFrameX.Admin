using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Web / Client / Hub）共享的 DataProtection 密钥环持久化装配。
/// <para>默认密钥环落在用户主目录（容器内 /home/app/.aspnet/DataProtection-Keys），容器重建即全部丢失，
/// 且启动报 "Storing keys in a directory ... may not be persisted outside of the container" 告警。
/// 本扩展把密钥环固定到 <c>{ContentRoot}/data/DataProtection-Keys</c>——容器内即挂载卷 /app/data
/// （三 host Dockerfile 均已 chown 非 root 用户），与 data/logs 文件日志同一持久化口径。</para>
/// </summary>
public static class HostBootstrapDataProtectionExtensions
{
    /// <summary>
    /// 注册 DataProtection 并把密钥环持久化到 <c>{ContentRoot}/data/DataProtection-Keys</c>。
    /// <para>目录随首次写键自动创建；<see cref="WebHostApplicationBuilder.Environment"/>.ApplicationName
    /// 作 SetApplicationName，多 host 共享同一挂载目录时密钥环按 host 隔离。</para>
    /// </summary>
    /// <param name="builder">host 构建器，提供 ContentRootPath 与 ApplicationName。</param>
    public static void UseHostDataProtection(this WebApplicationBuilder builder)
    {
        var keyDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "data", "DataProtection-Keys"));
        ProbeKeyRingWritable(keyDirectory);
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(keyDirectory)
            .SetApplicationName(builder.Environment.ApplicationName);
    }

    /// <summary>
    /// 启动期密钥环目录可写性预检：不可写时打高声错误 + 修复指引后放行（不 fail-fast，不回退临时密钥）。
    /// <para>放行理由：三 host 鉴权均为 JWT/HMAC，不消费 DataProtection，存储故障不应阻断业务进程
    /// （与 data/logs 文件日志"日志 I/O 永不影响业务线程"同哲学）；回退 EphemeralProvider 会把
    /// "容器重建丢密钥"的原始问题静默藏回，禁止。已持久化的旧密钥仍可读取使用，仅新密钥生成/轮换失败
    /// （框架在密钥环刷新时仍会重试并另行记 ERR，本预检只负责把修复动作说清楚）。</para>
    /// </summary>
    /// <param name="keyDirectory">密钥环目录。</param>
    private static void ProbeKeyRingWritable(DirectoryInfo keyDirectory)
    {
        try
        {
            keyDirectory.Create(); // 幂等：已存在 no-op；父目录不可写时在此抛 UnauthorizedAccessException
            var probeFilePath = Path.Combine(keyDirectory.FullName, $".write-probe-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFilePath, nameof(HostBootstrapDataProtectionExtensions));
            File.Delete(probeFilePath);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            // 组合期 DI 未接线，static Serilog 是 SilentLogger 会吞日志——按 HostMainDbConnectionResolver 房规走 Console
            Console.Error.WriteLine($"[Error] DataProtection 密钥环目录不可写，新密钥将无法生成/轮换（已有密钥仍可读取；host 继续运行，业务不受影响）。" +
                $" Path={keyDirectory.FullName} Error={exception.Message}" +
                $" 修复指引：检查该目录挂载——宿主 bind mount 属主须匹配容器 app 用户（镜像 $APP_UID），或改用 named volume（首挂自动继承镜像内 chown）；移除只读挂载(:ro)；SELinux 环境追加 :z 标签。");
            Console.Error.Flush();
        }
    }
}
