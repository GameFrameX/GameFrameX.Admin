// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Client.Api.Services;

/// <summary>
/// Ip 2 Location Manager
/// </summary>
public static class Ip2LocationManager
{
    private const string DefaultDatabasePath = "./IP2LOCATION-LITE-DB11.IPV6.BIN/IP2LOCATION-LITE-DB11.IPV6.BIN";

    /// <summary>
    ///
    /// </summary>
    public static IP2Location.Component Ip2Location { get; private set; } = null!;

    /// <summary>
    ///
    /// </summary>
    public static bool IsEnabled
    {
        get { return Ip2Location != null; }
    }

    /// <summary>
    ///
    /// </summary>
    public static void Init()
    {
        var databasePath = Environment.GetEnvironmentVariable("IP2LOCATION_DB_PATH");
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            databasePath = DefaultDatabasePath;
        }

        var fileInfo = new FileInfo(databasePath);
        if (!fileInfo.Exists)
        {
            Console.WriteLine("IP2Location 数据库未启用，缺少文件：" + fileInfo.FullName);
            Console.WriteLine("如需启用 IP 地理位置解析，请从 https://lite.ip2location.com 下载 IP2LOCATION-LITE-DB11.IPV6.BIN。");
            Console.WriteLine("默认放置路径：" + new FileInfo(DefaultDatabasePath).FullName);
            Console.WriteLine("Docker 可挂载到 /app/IP2LOCATION-LITE-DB11.IPV6.BIN/IP2LOCATION-LITE-DB11.IPV6.BIN，或通过 IP2LOCATION_DB_PATH 指定路径。");
            return;
        }

        var location = new IP2Location.Component();
        location.Open(fileInfo.FullName);
        Ip2Location = location;
        Console.WriteLine("IP2Location 数据库已加载：" + fileInfo.FullName);
    }
}