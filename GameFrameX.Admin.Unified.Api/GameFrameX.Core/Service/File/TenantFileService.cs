using GameFrameX.Core.Service.File.Dto;
using GameFrameX.Core.Service.Tenant.ObjectStorage;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Tenant.ObjectStorage;
using GameFrameX.ObjectStorage.Abstract;
using Microsoft.AspNetCore.StaticFiles;

namespace GameFrameX.Core.Service.File;

/// <summary>
/// 系统文件服务。
/// <para>上传/下载/删除全部经由 <see cref="IObjectStorageAdapterFactory"/> 解析当前租户的对象存储配置。</para>
/// <para>业务规则：</para>
/// <list type="bullet">
/// <item>上传必须命中当前租户唯一启用的配置；未启用任何配置时直接报错，不做隐式 fallback。</item>
/// <item>下载/删除按文件记录的 <see cref="TenantFileEntity.StorageConfigId"/> 精确解析（即使该配置已被禁用）。</item>
/// <item><see cref="TenantFileEntity.StorageConfigId"/> 为 null 的历史记录按本地 legacy 行为兜底（wwwroot 下原 FilePath）。</item>
/// <item>大小限额直接按字节比较 <see cref="TenantObjectStorageConfigEntity.MaxFileSizeBytes"/>。</item>
/// <item>MD5 去重 key 收紧为 StorageConfigId + FileMd5 + SizeKb。</item>
/// <item>上传 Provider 失败 → 不写文件记录；删除 Provider 失败 → 保留记录并记录日志。</item>
/// </list>
/// </summary>
[ApiDescriptionSettings(Order = 410)]
public class TenantFileService : IAdminApiController
{
    private static readonly JsonSerializerOptions ContentTypeJsonOptions = new();

    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<TenantFileEntity> _systemFileRepository;
    private readonly SqlSugarRepository<TenantObjectStorageConfigEntity> _storageConfigRepository;
    private readonly IObjectStorageAdapterFactory _adapterFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantFileService> _logger;

    public TenantFileService(CurrentUserContext currentUserContext,
        SqlSugarRepository<TenantFileEntity> systemFileRepository,
        SqlSugarRepository<TenantObjectStorageConfigEntity> storageConfigRepository,
        IObjectStorageAdapterFactory adapterFactory,
        IServiceProvider serviceProvider,
        ILogger<TenantFileService> logger)
    {
        _currentUserContext = currentUserContext;
        _systemFileRepository = systemFileRepository;
        _storageConfigRepository = storageConfigRepository;
        _adapterFactory = adapterFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 获取文件分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取文件分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantFileResponse>> PageAsync(PageFileRequest input)
    {
        var startTimestamp = input.StartTime ?? 0;
        var endTimestamp = input.EndTime ?? long.MaxValue;

        return await _systemFileRepository.AsQueryable()
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.FileName), u => (u.FileName ?? string.Empty).Contains((input.FileName ?? string.Empty).Trim()))
                                   .WhereIF(input.StartTime.HasValue && input.EndTime.HasValue,
                                            u => u.CreatedTime >= startTimestamp && u.CreatedTime <= endTimestamp)
                                   .OrderBy(u => u.CreatedTime, OrderByType.Desc)
                                   .Select<TenantFileResponse>()
                                   .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [DisplayName("上传文件")]
    [ApiDescriptionSettings(Name = "UploadFile")]
    public async Task<FileResponse> UploadFileAsync([Required] IFormFile file, [FromQuery] string? path)
    {
        var systemFile = await HandleUploadFileAsync(file, path);
        return new FileResponse
        {
            Id = systemFile.Id,
            Url = systemFile.Url ?? string.Empty,
            SizeKb = systemFile.SizeKb ?? string.Empty,
            Suffix = systemFile.Suffix ?? string.Empty,
            FilePath = systemFile.FilePath ?? string.Empty,
            FileName = systemFile.FileName ?? string.Empty,
        };
    }

    /// <summary>
    /// 上传文件Base64
    /// </summary>
    /// <param name="strBase64"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<FileResponse> UploadFileFromBase64Async(string strBase64, string fileName, string contentType, string? path)
    {
        if (string.IsNullOrWhiteSpace(strBase64))
        {
            throw BusinessError.Oh(FileErrorCode.FileNotFound);
        }

        var fileData = Convert.FromBase64String(strBase64);
        var ms = new MemoryStream();
        ms.Write(fileData);
        ms.Seek(0, SeekOrigin.Begin);
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"{SnowFlakeIdHelper.Instance.NextId()}.jpg";
        }

        if (string.IsNullOrEmpty(contentType))
        {
            contentType = "image/jpg";
        }

        IFormFile formFile = new FormFile(ms, 0, fileData.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
        return await UploadFileAsync(formFile, path);
    }

    /// <summary>
    /// 上传文件Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("上传文件Base64")]
    [HttpPost]
    [ApiDescriptionSettings(Name = "UploadFileFromBase64")]
    public async Task<FileResponse> UploadFileFromBase64Async(UploadFileFromBase64Request input)
    {
        return await UploadFileFromBase64Async(input.FileDataBase64, input.FileName, input.ContentType, input.Path);
    }

    /// <summary>
    /// 上传多文件
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    [DisplayName("上传多文件")]
    [ApiDescriptionSettings(Name = "UploadFiles")]
    public async Task<List<FileResponse>> UploadFilesAsync([Required] List<IFormFile> files)
    {
        var filelist = new List<FileResponse>();
        foreach (var file in files)
        {
            filelist.Add(await UploadFileAsync(file, ""));
        }

        return filelist;
    }

    /// <summary>
    /// 根据文件Id或Url下载
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据文件Id或Url下载")]
    [ApiDescriptionSettings(Name = "DownloadFile")]
    public async Task<IActionResult> DownloadFileAsync(FileRequest input)
    {
        var file = input.Id > 0 ? await GetFileAsync(input) : await _systemFileRepository.GetFirstAsync(u => u.Url == input.Url);
        if (file == null)
        {
            throw BusinessError.Oh(FileErrorCode.FileNotFound);
        }

        var resolved = await ResolveForExistingFileAsync(file);
        var adapter = resolved.Adapter;
        var context = resolved.Context;
        var objectKey = BuildObjectKey(file);
        var stream = await adapter.OpenReadAsync(context, objectKey, default);
        var fileName = HttpUtility.UrlEncode(file.FileName, Encoding.GetEncoding("UTF-8"));
        return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileName + file.Suffix, };
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除文件")]
    public async Task DeleteFileAsync(DeleteFileRequest input)
    {
        var file = await _systemFileRepository.GetFirstAsync(u => u.Id == input.Id);
        if (file == null)
        {
            return;
        }

        // 删除对象存储对象；Provider 失败时保留记录并记录日志，不删除 DB 行。
        try
        {
            var resolved = await ResolveForExistingFileAsync(file);
            var adapter = resolved.Adapter;
            var context = resolved.Context;
            var objectKey = BuildObjectKey(file);
            await adapter.DeleteAsync(context, objectKey, default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除对象存储文件失败，保留文件记录。FileId={FileId} StorageConfigId={StorageConfigId}", file.Id, file.StorageConfigId);
            throw BusinessError.Oh("文件删除失败：" + ex.Message);
        }

        await _systemFileRepository.DeleteAsync(file);
    }

    /// <summary>
    /// 更新文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新文件")]
    public async Task UpdateFileAsync(FileRequest input)
    {
        var isExist = await _systemFileRepository.IsAnyAsync(u => u.Id == input.Id);
        if (!isExist)
        {
            throw BusinessError.Oh(FileErrorCode.FileNotFound);
        }

        await _systemFileRepository.UpdateAsync(u => new TenantFileEntity { FileName = input.FileName, }, u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task<TenantFileEntity> GetFileAsync([FromQuery] FileRequest input)
    {
        var file = await _systemFileRepository.GetFirstAsync(u => u.Id == input.Id);
        return file ?? throw BusinessError.Oh(FileErrorCode.FileNotFound);
    }

    /// <summary>
    /// 上传文件核心逻辑：解析租户活动配置 → 校验 → 写对象存储 → 写文件记录。
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="savePath">路径覆盖值；为空时使用配置的 PathTemplate</param>
    /// <returns></returns>
    private async Task<TenantFileEntity> HandleUploadFileAsync(IFormFile file, string? savePath)
    {
        if (file == null)
        {
            throw BusinessError.Oh(FileErrorCode.FileNotFound);
        }

        // 当前租户必须存在唯一启用的对象存储配置。命中 0 条 → 未配置；命中 ≥2 条 → 配置异常（不变量违反）。
        // 两者都明确报错，不 fallback、不静默取首条（spec-review P1：Resolver 遇多条视为配置错误）。
        var activeConfigs = await _storageConfigRepository.AsQueryable().Where(c => c.IsEnabled == true).ToListAsync();
        var config = EnsureSingleActiveConfig(activeConfigs);

        // 大小限额直接按字节比较，避免历史 KB/字节混用导致的 1024 倍偏差。
        if (file.Length > config.MaxFileSizeBytes)
        {
            throw BusinessError.Oh(FileErrorCode.FileSizeExceeded);
        }

        // 内容类型校验：AllowedContentTypesJson 为空表示不限制。
        if (!IsContentTypeAllowed(config, file.ContentType))
        {
            throw BusinessError.Oh(FileErrorCode.FileTypeNotAllowed);
        }

        var suffix = ResolveSuffix(file);
        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw BusinessError.Oh(FileErrorCode.FileExtensionInvalid);
        }

        var sizeKb = ((long)(file.Length / 1024.0)).ToString();
        var path = ResolveUploadPath(savePath, config.PathTemplate);

        // MD5 去重 key 收紧为 StorageConfigId + FileMd5 + SizeKb。
        var fileMd5 = string.Empty;
        if (config.EnableMd5)
        {
            using (var md5Stream = file.OpenReadStream())
            {
                fileMd5 = ComputeMd5(md5Stream);
            }

            var dup = await _systemFileRepository.GetFirstAsync(u => u.StorageConfigId == config.Id
                                                              && u.FileMd5 == fileMd5
                                                              && u.SizeKb == sizeKb);
            if (dup != null)
            {
                return dup;
            }
        }

        var newFile = new TenantFileEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            Provider = config.Provider,
            BucketName = string.Empty,
            FileName = Path.GetFileNameWithoutExtension(file.FileName),
            Suffix = suffix,
            SizeKb = sizeKb,
            FilePath = path,
            FileMd5 = fileMd5,
            StorageConfigId = config.Id,
        };

        var finalName = newFile.Id + suffix;
        var objectKey = string.IsNullOrEmpty(path) ? finalName : path + "/" + finalName;

        // 上传 Provider 失败 → 不写文件记录。ConfigJson 敏感字段解密走进程级缓存（C370，禁止每请求 PBKDF2 派生）
        var adapter = _adapterFactory.Resolve(config.Provider);
        var context = _adapterFactory.BuildContext(config.Provider, TenantObjectStorageConfigCrypto.DecryptConfigJson(config.Provider, config.ConfigJson), config.PathTemplate);
        try
        {
            using (var uploadStream = file.OpenReadStream())
            {
                await adapter.PutAsync(context, uploadStream, objectKey, file.ContentType, default);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传对象存储文件失败。Provider={Provider} StorageConfigId={StorageConfigId}", config.Provider, config.Id);
            throw BusinessError.Oh("文件上传失败：" + ex.Message);
        }

        newFile.BucketName = context.Bucket;
        newFile.Url = adapter.BuildPublicUrl(context, objectKey);

        // DB 插入若在 Provider 上传成功后失败，必须补偿删除已上传对象，避免孤儿数据（spec-review 执行提示）。
        // 补偿失败时留下不含凭证的运维线索（objectKey + StorageConfigId），不能对调用方报告上传成功。
        try
        {
            await _systemFileRepository.AsInsertable(newFile).ExecuteCommandAsync();
        }
        catch (Exception dbEx)
        {
            try
            {
                await adapter.DeleteAsync(context, objectKey, default);
            }
            catch (Exception compEx)
            {
                _logger.LogError(compEx, "对象存储补偿删除失败，存在孤儿对象。objectKey={ObjectKey} StorageConfigId={StorageConfigId}", objectKey, config.Id);
            }

            _logger.LogError(dbEx, "对象存储文件记录插入失败，已尝试补偿删除。objectKey={ObjectKey} StorageConfigId={StorageConfigId}", objectKey, config.Id);
            throw BusinessError.Oh("文件上传失败：" + dbEx.Message);
        }

        return newFile;
    }

    /// <summary>
    /// 上传头像
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [DisplayName("上传头像")]
    [ApiDescriptionSettings(Name = "UploadAvatar")]
    public async Task<FileResponse> UploadAvatarAsync([Required] IFormFile file)
    {
        var systemUserRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<TenantUserEntity>>();
        var user = systemUserRepository.GetFirst(u => u.Id == _currentUserContext.UserId);
        // 删除当前用户已有头像
        if (!string.IsNullOrWhiteSpace(user.Avatar))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Avatar);
            await DeleteFileAsync(new DeleteFileRequest { Id = long.Parse(fileId), });
        }

        var res = await UploadFileAsync(file, "Upload/Avatar");
        await systemUserRepository.UpdateAsync(u => new TenantUserEntity { Avatar = $"{res.FilePath}/{res.Name}", }, u => u.Id == user.Id);
        return res;
    }

    /// <summary>
    /// 上传电子签名
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [DisplayName("上传电子签名")]
    [ApiDescriptionSettings(Name = "UploadSignature")]
    public async Task<FileResponse> UploadSignatureAsync([Required] IFormFile file)
    {
        var systemUserRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<TenantUserEntity>>();
        var user = systemUserRepository.GetFirst(u => u.Id == _currentUserContext.UserId);
        // 删除当前用户已有电子签名
        if (!string.IsNullOrWhiteSpace(user.Signature) && user.Signature.EndsWith(".png"))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Signature);
            await DeleteFileAsync(new DeleteFileRequest { Id = long.Parse(fileId), });
        }

        var res = await UploadFileAsync(file, "Upload/Signature");
        await systemUserRepository.UpdateAsync(u => new TenantUserEntity { Signature = $"{res.FilePath}/{res.Name}", }, u => u.Id == user.Id);
        return res;
    }

    /// <summary>
    /// 解析既有文件的 Adapter + Context：StorageConfigId 命中则按精确配置（即使已禁用）；
    /// 历史记录（StorageConfigId 为空）按本地 legacy 兜底。
    /// </summary>
    private async Task<ResolvedFileAdapter> ResolveForExistingFileAsync(TenantFileEntity file)
    {
        if (file.StorageConfigId.HasValue && file.StorageConfigId.Value > 0)
        {
            var config = await _storageConfigRepository.GetFirstAsync(c => c.Id == file.StorageConfigId.Value);
            if (config != null)
            {
                var adapter = _adapterFactory.Resolve(config.Provider);
                var context = _adapterFactory.BuildContext(config.Provider, TenantObjectStorageConfigCrypto.DecryptConfigJson(config.Provider, config.ConfigJson), config.PathTemplate);
                return new ResolvedFileAdapter(adapter, context);
            }
        }

        // legacy 本地兜底：对象键落在文件记录的 FilePath 下。
        var localAdapter = _adapterFactory.Resolve("local");
        var localContext = _adapterFactory.BuildContext("local", "{}", file.FilePath ?? "Upload");
        return new ResolvedFileAdapter(localAdapter, localContext);
    }

    private static string BuildObjectKey(TenantFileEntity file)
    {
        var finalName = file.Id + (file.Suffix ?? string.Empty);
        return string.IsNullOrEmpty(file.FilePath) ? finalName : file.FilePath + "/" + finalName;
    }

    private static string ResolveUploadPath(string? savePath, string pathTemplate)
    {
        var path = savePath;
        if (string.IsNullOrWhiteSpace(savePath))
        {
            path = string.IsNullOrWhiteSpace(pathTemplate) ? "Upload" : pathTemplate;
            var reg = new Regex(@"(\{.+?})");
            foreach (Match match in reg.Matches(path))
            {
                var str = DateTime.Now.ToString(match.Value.Substring(1, match.Value.Length - 2));
                path = path.Replace(match.Value, str);
            }
        }

        // 路径安全：上传路径必须受控相对路径，禁止绝对路径与跨根 ".."，防止逃逸对象存储根。
        // path 至此必非 null：savePath 非空时由 savePath 直接赋值，savePath 空白时由 pathTemplate 或 "Upload" 兜底。
        path ??= "Upload";
        EnsureSafeRelativePath(path);
        return path;
    }

    /// <summary>
    /// 校验当前租户存在且仅存在一条启用的对象存储配置。
    /// <para>命中 0 条：上传明确报错“未配置”，不 fallback 到平台/本地。</para>
    /// <para>命中 ≥2 条：视为唯一活动配置不变量被违反（并发竞态/回填 bug/手工改库），明确报错，不静默取首条。</para>
    /// </summary>
    /// <param name="activeConfigs">当前租户启用的配置列表。</param>
    /// <returns>唯一启用配置。</returns>
    internal static TenantObjectStorageConfigEntity EnsureSingleActiveConfig(List<TenantObjectStorageConfigEntity> activeConfigs)
    {
        if (activeConfigs == null || activeConfigs.Count == 0)
        {
            throw BusinessError.Oh("当前租户未启用对象存储配置，请联系管理员在「对象存储配置」中启用一项");
        }

        if (activeConfigs.Count > 1)
        {
            throw BusinessError.Oh("对象存储配置异常：当前租户存在多个启用配置，请联系管理员仅保留一项");
        }

        return activeConfigs[0];
    }

    /// <summary>
    /// 校验上传路径为受控相对路径：拒绝绝对路径、拒绝跨根 ".." 段。
    /// <para>P0 安全红线：用户可通过 UploadFileAsync 的 path 参数传入任意值，未校验会导致对象键逃逸 wwwroot 根目录。</para>
    /// </summary>
    /// <param name="path"></param>
    private static void EnsureSafeRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (Path.IsPathRooted(path))
        {
            throw BusinessError.Oh("上传路径不能为绝对路径");
        }

        var segments = path.Replace('\\', '/').Split(new[] { '/', }, StringSplitOptions.None);
        foreach (var segment in segments)
        {
            if (segment == "..")
            {
                throw BusinessError.Oh("上传路径不能包含父级引用（..）");
            }
        }
    }

    private static string ResolveSuffix(IFormFile file)
    {
        var suffix = Path.GetExtension(file.FileName).ToLower();
        if (string.IsNullOrWhiteSpace(suffix))
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            suffix = contentTypeProvider.Mappings.FirstOrDefault(u => u.Value == file.ContentType).Key;
            // 修改 image/jpeg 类型返回的 .jpe 后缀
            if (suffix == ".jpe")
            {
                suffix = ".jpg";
            }
        }

        return suffix ?? string.Empty;
    }

    private static bool IsContentTypeAllowed(TenantObjectStorageConfigEntity config, string contentType)
    {
        if (string.IsNullOrWhiteSpace(config.AllowedContentTypesJson))
        {
            return true;
        }

        try
        {
            var allowed = JsonSerializer.Deserialize<List<string>>(config.AllowedContentTypesJson, ContentTypeJsonOptions);
            if (allowed == null || allowed.Count == 0)
            {
                return true;
            }

            return allowed.Contains(contentType);
        }
        catch
        {
            // 配置 JSON 非法时不误杀上传，交由上层其它规则处理。
            return true;
        }
    }

    private static string ComputeMd5(Stream stream)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(stream);
        stream.Position = 0;
        return Convert.ToBase64String(hash);
    }
}