namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 某 Provider 的完整配置 Schema：标准名、展示标签、字段列表。
/// 前端据此渲染动态表单，后端 NormalizeConfigJson 据此做白名单与必填校验。
/// </summary>
public sealed class ObjectStorageConfigSchema
{
    public string Provider { get; set; } = null!;

    public string Label { get; set; } = null!;

    public List<ObjectStorageConfigField> Fields { get; set; } = null!;
}