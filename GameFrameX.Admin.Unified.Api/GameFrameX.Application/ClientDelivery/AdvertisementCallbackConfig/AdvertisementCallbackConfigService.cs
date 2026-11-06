using System.Text.Json.Nodes;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Business;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig.Dto;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Core.Enum.Errors;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig;

/// <summary>
/// 广告回调配置管理服务。
/// <para>只管理各广告平台服务端回调所需的最小配置材料，不实现广告回调接收和发奖处理。</para>
/// <para>敏感字段（SigningSecret / VerificationKeys）输出脱敏，更新回传 <c>******</c> 保留原值。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class AdvertisementCallbackConfigService
    : GenericCrudService<Entity.Business.ClientApiConfig.AdvertisementCallbackConfigEntity,
        AddAdvertisementCallbackConfigRequest, UpdateAdvertisementCallbackConfigRequest,
        AdvertisementCallbackConfigResponse, PageAdvertisementCallbackConfigRequest>
{
    private readonly IAdvertisementCallbackConfigSchemaProvider _schemaProvider;
    private readonly SqlSugarRepository<GameAppPackageEntity> _packageRepository;
    private const string SupportedProviderMessage = "广告平台只支持 TopOn、GoogleAdMob、Pangle、UnityAds";

    public AdvertisementCallbackConfigService(
        SqlSugarRepository<Entity.Business.ClientApiConfig.AdvertisementCallbackConfigEntity> repository,
        IAdvertisementCallbackConfigSchemaProvider schemaProvider,
        SqlSugarRepository<GameAppPackageEntity> packageRepository)
        : base(repository)
    {
        _schemaProvider = schemaProvider;
        _packageRepository = packageRepository;
    }

    protected override ISugarQueryable<AdvertisementCallbackConfigResponse> BuildPageQuery(PageAdvertisementCallbackConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        var provider = NormalizeProvider(input.Provider, false);
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => u.Remark.Contains(searchKey) || u.CallbackUrl.Contains(searchKey))
                         .WhereIF(!string.IsNullOrWhiteSpace(provider), u => u.Provider == provider)
                         .WhereIF(input.PackageId.HasValue, u => u.PackageId == input.PackageId.Value)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<AdvertisementCallbackConfigResponse>();
    }

    /// <summary>
    /// 新增广告回调配置（同一分组的 (PackageId, Provider) 唯一）。
    /// </summary>
    public override async Task<AdvertisementCallbackConfigResponse> AddAsync(AddAdvertisementCallbackConfigRequest input)
    {
        var provider = NormalizeProvider(input.Provider, true);
        var packageId = await ValidatePackageAsync(input.PackageId);
        var configJson = NormalizeConfigJson(provider, input.ConfigJson, null);
        await CheckUniqueScopeAsync(packageId, provider, null);

        var entity = input.Adapt<Entity.Business.ClientApiConfig.AdvertisementCallbackConfigEntity>();
        entity.Provider = provider;
        entity.PackageId = packageId;
        entity.ConfigJson = configJson;
        ApplyConfigSummary(entity, configJson);
        // C371：确定最终明文之后、落库之前加密——汇总列整列密文 + ConfigJson 敏感字段级密文（Encrypt 幂等兼容存量明文/密文）
        entity.SigningSecret = SecretCrypto.Encrypt(entity.SigningSecret) ?? string.Empty;
        entity.VerificationKeys = SecretCrypto.Encrypt(entity.VerificationKeys) ?? string.Empty;
        entity.ConfigJson = AdvertisementCallbackConfigCrypto.EncryptConfigJson(provider, configJson);
        entity.ParameterPurposeJson = BuildParameterPurposeJson(provider);
        entity.IsEnabled = input.IsEnabled;
        await Repository.InsertAsync(entity);
        return entity.Adapt<AdvertisementCallbackConfigResponse>();
    }

    /// <summary>
    /// 更新广告回调配置。敏感字段回传 <c>******</c> 时保留原值。
    /// </summary>
    public override async Task UpdateAsync(UpdateAdvertisementCallbackConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("广告回调配置不存在");
        }

        var provider = NormalizeProvider(input.Provider, true);
        var packageId = await ValidatePackageAsync(input.PackageId);
        var configJson = NormalizeConfigJson(provider, input.ConfigJson, entity.ConfigJson);
        await CheckUniqueScopeAsync(packageId, provider, input.Id);
        entity.Provider = provider;
        entity.PackageId = packageId;
        entity.ConfigJson = configJson;
        ApplyConfigSummary(entity, configJson);
        // C371：确定最终明文之后、落库之前加密——保留语义下 NormalizeConfigJson 回填的可能是存量密文，Encrypt 幂等跳过不换密文
        entity.SigningSecret = SecretCrypto.Encrypt(entity.SigningSecret) ?? string.Empty;
        entity.VerificationKeys = SecretCrypto.Encrypt(entity.VerificationKeys) ?? string.Empty;
        entity.ConfigJson = AdvertisementCallbackConfigCrypto.EncryptConfigJson(provider, configJson);
        entity.ParameterPurposeJson = BuildParameterPurposeJson(provider);
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.Provider,
                            e.PackageId,
                            e.CallbackUrl,
                            e.SigningSecret,
                            e.VerificationKeys,
                            e.ConfigJson,
                            e.ParameterPurposeJson,
                            e.IsEnabled,
                            e.Remark,
                        })
                        .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 分页查询（敏感字段脱敏）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public override async Task<SqlSugarPagedList<AdvertisementCallbackConfigResponse>> PageAsync(PageAdvertisementCallbackConfigRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            MaskSecrets(item, true);
        }

        return result;
    }

    /// <summary>
    /// 详情（敏感字段脱敏）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<AdvertisementCallbackConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh("广告回调配置不存在");
        }

        var output = entity.Adapt<AdvertisementCallbackConfigResponse>();
        MaskSecrets(output, false);
        return output;
    }

    /// <summary>
    /// 获取指定广告 Provider 的动态配置字段 schema。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Schema")]
    public AdvertisementCallbackConfigSchema? GetSchema([FromQuery] string provider)
    {
        var normalized = NormalizeProvider(provider, true);
        return _schemaProvider.GetSchema(normalized);
    }

    /// <summary>
    /// 获取首批支持的广告 Provider 清单。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Providers")]
    public List<AdvertisementCallbackProviderItem> GetProviders()
    {
        return _schemaProvider.GetProviders();
    }

    /// <summary>
    /// 校验并返回配置绑定的分包 Id：必填正数，且分包必须存在并启用（与回调解析的启用校验一致）。
    /// </summary>
    private async Task<long> ValidatePackageAsync(long? packageId)
    {
        if (packageId == null || packageId <= 0)
        {
            throw BusinessError.Oh(AdvertisementCallbackConfigErrorCode.PackageIdRequired);
        }

        var package = await _packageRepository.GetFirstAsync(x => x.Id == packageId.Value && x.IsEnabled == true);
        if (package == null)
        {
            throw BusinessError.Oh(AdvertisementCallbackConfigErrorCode.PackageNotFound);
        }

        return packageId.Value;
    }

    private async Task CheckUniqueScopeAsync(long packageId, string provider, long? excludeId)
    {
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.PackageId == packageId && x.Provider == provider && x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.PackageId == packageId && x.Provider == provider);
        if (exists)
        {
            throw BusinessError.Oh(AdvertisementCallbackConfigErrorCode.PackageProviderAlreadyExists);
        }
    }

    private static string NormalizeProvider(string provider, bool required)
    {
        var value = AdvertisementCallbackConfigSchemaProvider.NormalizeProvider(provider);
        if (value.Length == 0 && !required)
        {
            return string.Empty;
        }

        return value.Length > 0 ? value : throw BusinessError.Oh(SupportedProviderMessage);
    }

    private string NormalizeConfigJson(string provider, string configJson, string? existingConfigJson)
    {
        var schema = _schemaProvider.GetSchema(provider);
        if (schema == null)
        {
            throw BusinessError.Oh(SupportedProviderMessage);
        }

        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw BusinessError.Oh("广告回调配置不能为空");
        }

        JsonObject? current;
        JsonObject? existing = null;
        try
        {
            current = JsonNode.Parse(configJson) as JsonObject;
            if (!string.IsNullOrWhiteSpace(existingConfigJson))
            {
                existing = JsonNode.Parse(existingConfigJson) as JsonObject;
            }
        }
        catch (JsonException ex)
        {
            throw BusinessError.Oh($"广告回调配置JSON格式错误: {ex.Message}");
        }

        if (current == null)
        {
            throw BusinessError.Oh("广告回调配置必须是JSON对象");
        }

        foreach (var field in schema.Fields)
        {
            if (!current.TryGetPropertyValue(field.Key, out var value) || value == null)
            {
                current[field.Key] = field.Default == null ? string.Empty : JsonValue.Create(field.Default);
                value = current[field.Key];
            }

            if (IsSensitiveField(field) && IsMask(value) && existing != null &&
                existing.TryGetPropertyValue(field.Key, out var existingValue) && existingValue != null)
            {
                current[field.Key] = JsonNode.Parse(existingValue.ToJsonString());
                value = current[field.Key];
            }

            if (field.Required && IsEmpty(value))
            {
                throw BusinessError.Oh($"{field.Label}不能为空");
            }
        }

        return current.ToJsonString();
    }

    private static bool IsMask(JsonNode? value)
    {
        return value is JsonValue jsonValue &&
               jsonValue.TryGetValue<string>(out var text) &&
               SensitiveFieldMask.MaskValue.Equals(text, StringComparison.Ordinal);
    }

    private static bool IsEmpty(JsonNode? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text))
        {
            return string.IsNullOrWhiteSpace(text) || SensitiveFieldMask.MaskValue.Equals(text, StringComparison.Ordinal);
        }

        return false;
    }

    private static bool IsSensitiveField(AdvertisementCallbackConfigField field)
    {
        return field.Sensitive || field.Type == "secret";
    }

    private static void ApplyConfigSummary(Entity.Business.ClientApiConfig.AdvertisementCallbackConfigEntity entity, string configJson)
    {
        entity.CallbackUrl = GetConfigString(configJson, "callbackUrl");
        entity.SigningSecret = GetConfigString(configJson, "signingSecret");
        entity.VerificationKeys = GetConfigString(configJson, "verificationKeys");
    }

    private static string BuildParameterPurposeJson(string provider)
    {
        return JsonSerializer.Serialize(new
        {
            provider,
            callbackHandling = "clientApiPublicCallback",
            source = "schema",
        });
    }

    private static string GetConfigString(string configJson, string key)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.TryGetProperty(key, out var value) &&
                   value.ValueKind == JsonValueKind.String
                       ? value.GetString() ?? string.Empty
                       : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private void MaskSecrets(AdvertisementCallbackConfigResponse output, bool maskWholeConfig)
    {
        output.SigningSecret = string.IsNullOrWhiteSpace(output.SigningSecret) ? string.Empty : SensitiveFieldMask.MaskValue;
        output.VerificationKeys = string.IsNullOrWhiteSpace(output.VerificationKeys) ? string.Empty : SensitiveFieldMask.MaskValue;
        if (string.IsNullOrWhiteSpace(output.ConfigJson))
        {
            return;
        }

        if (maskWholeConfig)
        {
            output.ConfigJson = SensitiveFieldMask.MaskValue;
            return;
        }

        var schema = _schemaProvider.GetSchema(output.Provider);
        if (schema == null)
        {
            output.ConfigJson = SensitiveFieldMask.MaskValue;
            return;
        }

        var current = JsonNode.Parse(output.ConfigJson) as JsonObject;
        if (current == null)
        {
            output.ConfigJson = SensitiveFieldMask.MaskValue;
            return;
        }

        foreach (var field in schema.Fields.Where(IsSensitiveField))
        {
            if (current.TryGetPropertyValue(field.Key, out var value) && !IsEmpty(value))
            {
                current[field.Key] = SensitiveFieldMask.MaskValue;
            }
        }

        output.ConfigJson = current.ToJsonString();
    }
}
#pragma warning restore CS8604, CS8629
