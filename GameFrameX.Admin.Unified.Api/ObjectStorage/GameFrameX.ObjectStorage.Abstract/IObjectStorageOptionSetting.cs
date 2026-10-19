namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储 Provider 选项配置标记接口。
/// 实现 该接口的类型通过 <see cref="ObjectStorageProviderAttribute"/> 声明 Provider 元数据，
/// 并由 <see cref="ObjectStorageProviderRegistry"/> 在启动期自动扫描注册。
/// </summary>
public interface IObjectStorageOptionSetting
{
}