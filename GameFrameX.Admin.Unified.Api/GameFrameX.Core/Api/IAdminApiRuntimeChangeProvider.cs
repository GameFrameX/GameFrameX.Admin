namespace GameFrameX.Core.Api;

/// <summary>
/// Admin API 运行时程序集变更通知。
/// <para>用于插件编译/卸载后刷新 ASP.NET Core MVC action descriptor，不依赖 历史框架 动态 API 刷新器。</para>
/// </summary>
public interface IAdminApiRuntimeChangeProvider
{
    void AddAssembliesWithNotifyChanges(Assembly assembly);

    void RemoveAssembliesWithNotifyChanges(string assemblyName);
}