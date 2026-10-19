namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储配置脱敏常量。Page 整体覆盖；Detail 字段级替换；提交该值表示"保留原值"。
/// </summary>
public static class ObjectStorageConfigMask
{
    /// <summary>敏感字段掩码占位符。</summary>
    public const string Placeholder = "******";
}