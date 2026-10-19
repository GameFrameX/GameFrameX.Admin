using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.Local;

/// <summary>
/// 本地磁盘存储选项。无需云凭证，文件写入应用 wwwroot 下受控根目录。
/// 该 Provider 不暴露可配置字段。
/// </summary>
[ObjectStorageProvider("local", "本地存储")]
public class LocalObjectStorageOptionSetting : IObjectStorageOptionSetting
{
}