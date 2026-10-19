using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GameFrameX.Core.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Core.Logging;

/// <summary>
/// Monitor 日志采集中间件（C363 重建，字段面等价历史框架 LoggingMonitor）：action 执行后产出携带
/// <c>loggingMonitor</c> context 的日志消息并投递调度队列，由 <see cref="LoggingDispatchService"/> 分发到
/// 文件 / 数据库（<see cref="DatabaseLoggingWriter"/> 三表分流 + C156 分表路由）/ ES 写入器。
/// <para>语义依据：lesson 2026-07-11 三表分流（userInfo/logout 仅进 visit、异常进 exception、其余进 operation 且受
/// sys_oplog 门控）依赖本中间件产出的 <c>actionName</c>（路由段，如 userInfo/logout）与 <c>actionTypeName</c>
/// （方法名去 Async 后缀，如 Login）双字段；数据校验失败（validation）不落库。</para>
/// <para>豁免规则：方法或类标注 <see cref="SuppressMonitorAttribute"/> 时整条跳过；参数级标注跳过该参数。
/// Order=-2000 保证在 AdminResultFilter 等业务过滤器外层包住完整执行耗时。</para>
/// </summary>
internal sealed class MonitorLoggingFilter : IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 过滤器排序：外层最先执行（负向最小），与历史框架 LoggingMonitor 的 FilterOrder 对齐。
    /// </summary>
    public const int FilterOrder = -2000;

    /// <summary>
    /// 日志分类名。与历史框架 ILogger&lt;LoggingMonitor&gt;（namespace System.Logging）产出的分类一致，
    /// ElasticSearchLoggingWriter 的 WriteFilter 按该分类名过滤。
    /// </summary>
    public const string LogName = "System.Logging.LoggingMonitor";

    private const string MonitorTitle = "Logging Monitor";

    private static readonly ConcurrentDictionary<MethodInfo, MonitorMethodMetadata> MethodMetadataCache = new();
    private static readonly ConcurrentDictionary<string, JsonSerializerOptions> SerializerOptionsCache = new();

    private readonly ILoggingDispatchQueue _dispatchQueue;
    private readonly MonitorLoggingRuntimeState _runtimeState;
    private readonly MonitorLoggingEntry _entry;

    public MonitorLoggingFilter(ILoggingDispatchQueue dispatchQueue,
        MonitorLoggingRuntimeState runtimeState,
        MonitorLoggingEntry entry)
    {
        _dispatchQueue = dispatchQueue;
        _runtimeState = runtimeState;
        _entry = entry;
    }

    /// <summary>过滤器排序（<see cref="FilterOrder"/>）。</summary>
    public int Order => FilterOrder;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if ((context.ActionDescriptor as ControllerActionDescriptor) is not { } actionDescriptor)
        {
            await next();
            return;
        }

        var actionMethod = actionDescriptor.MethodInfo;
        var metadata = MethodMetadataCache.GetOrAdd(actionMethod, BuildMethodMetadata);
        var settings = _runtimeState.GetSettings();

        // [SuppressMonitor]（方法/类级）或全局开关关闭：完全跳过采集（与历史框架同语义）
        if (metadata.HasSuppressMonitor || !settings.GlobalEnabled)
        {
            await next();
            return;
        }

        var httpContext = context.HttpContext;
        var capture = new MonitorCaptureState
        {
            HttpContext = httpContext,
            // 参数在 action 执行前绑定完成，先做序列化快照（action 内部可能改动入参对象）
            Parameters = CaptureParameters(context, metadata),
            ControllerName = ResolveControllerName(actionDescriptor, actionMethod),
            ActionRouteName = ResolveActionRouteSegment(actionDescriptor),
            ActionTypeName = TrimAsyncSuffix(actionMethod.Name),
            DisplayTitle = metadata.DisplayTitle ?? string.Empty,
            HttpMethod = httpContext.Request.Method,
            RequestUrl = BuildRequestUrl(httpContext.Request),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            RemoteIpAddress = httpContext.GetRemoteIpAddressToIPv4(),
        };

        var timeOperation = Stopwatch.StartNew();
        ActionExecutedContext? executedContext = null;
        Exception? executionException = null;
        try
        {
            executedContext = await next();
            executionException = executedContext.Exception;
        }
        catch (Exception exception)
        {
            // async 过滤器管线以任务异常形态上抛时兜底采集；重抛保证异常过滤器（AdminResultFilter.OnExceptionAsync）照常渲染
            timeOperation.Stop();
            capture.ElapsedMilliseconds = timeOperation.ElapsedMilliseconds;
            EnqueueMonitorLog(BuildLogMessage(capture, executedContext, exception, validation: null));
            throw;
        }

        timeOperation.Stop();
        capture.ElapsedMilliseconds = timeOperation.ElapsedMilliseconds;

        // 数据校验失败（[ApiController] 自动 400 短路）：validation 非空 → DatabaseLoggingWriter 跳过落库
        JsonObject? validation = null;
        if (!context.ModelState.IsValid)
        {
            validation = new JsonObject
            {
                ["message"] = "数据验证失败",
                ["statusCode"] = StatusCodes.Status400BadRequest,
            };
        }

        EnqueueMonitorLog(BuildLogMessage(capture, executedContext, executionException, validation));
    }

    /// <summary>
    /// 组装单条 Monitor 日志消息（loggingMonitor JSON 写入 Message 与 Context）。
    /// </summary>
    private LogMessage BuildLogMessage(MonitorCaptureState capture,
        ActionExecutedContext? executedContext,
        Exception? executionException,
        JsonObject? validation)
    {
        var settings = _runtimeState.GetSettings();
        var responseHttpContext = executedContext?.HttpContext ?? capture.HttpContext;
        var bufferWriter = new ArrayBufferWriter<byte>(4096);
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            writer.WriteStartObject();
            writer.WriteString("title", MonitorTitle);
            writer.WriteString("controllerName", capture.ControllerName);
            writer.WriteString("actionName", capture.ActionRouteName);
            writer.WriteString("actionTypeName", capture.ActionTypeName);
            writer.WriteString("displayTitle", capture.DisplayTitle);
            writer.WriteString("httpMethod", capture.HttpMethod);
            writer.WriteString("requestUrl", capture.RequestUrl);
            writer.WriteString("userAgent", capture.UserAgent);
            writer.WriteString("remoteIPv4", capture.RemoteIpAddress);
            writer.WriteNumber("timeOperationElapsedMilliseconds", capture.ElapsedMilliseconds);

            WriteAuthorizationClaims(writer, responseHttpContext.User);
            WriteParameters(writer, capture.Parameters);
            WriteReturnInformation(writer, executedContext?.Result, responseHttpContext, settings);
            WriteException(writer, executionException);
            WriteValidation(writer, validation);
            writer.WriteEndObject();
        }

        var monitorJson = Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
        var logLevel = LogLevel.Information;
        if (executionException != null)
        {
            // 失败调用按 Error 级输出：DB Sink 最小级别语义放行，Writer 落 exception 表并填 Exception 字段
            logLevel = LogLevel.Error;
        }
        else if (validation != null)
        {
            logLevel = settings.BahLogLevel;
        }

        return new LogMessage
        {
            LogLevel = logLevel,
            LogName = LogName,
            Message = monitorJson,
            Exception = executionException,
            TraceId = Activity.Current?.Id ?? capture.HttpContext.TraceIdentifier,
        };
    }

    private void EnqueueMonitorLog(LogMessage logMessage)
    {
        logMessage.Context["loggingMonitor"] = logMessage.Message;
        _dispatchQueue.Enqueue(logMessage);
    }

    /// <summary>
    /// 参数值转 JsonNode：文件参数只留元数据、byte[] 只留长度、基元/字符串直写、复杂对象按忽略规则序列化。
    /// </summary>
    private JsonNode? ConvertValueToJsonNode(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is IFormFile formFile)
        {
            return new JsonObject
            {
                ["fileName"] = formFile.FileName,
                ["length"] = formFile.Length,
                ["contentType"] = formFile.ContentType,
            };
        }

        if (value is byte[] byteArray)
        {
            return new JsonObject
            {
                ["length"] = byteArray.Length,
            };
        }

        if (value is string textValue)
        {
            return JsonValue.Create(textValue);
        }

        if (value is bool boolValue)
        {
            return JsonValue.Create(boolValue);
        }

        if (value is int intValue)
        {
            return JsonValue.Create(intValue);
        }

        if (value is long longValue)
        {
            return JsonValue.Create(longValue);
        }

        if (value is double doubleValue)
        {
            return JsonValue.Create(doubleValue);
        }

        if (value is decimal decimalValue)
        {
            return JsonValue.Create(decimalValue);
        }

        if (value is DateTime dateTimeValue)
        {
            return JsonValue.Create(dateTimeValue);
        }

        return TrySerializeObject(value, out var json) ? JsonNode.Parse(json) : null;
    }

    private bool TrySerializeObject(object value, out string json)
    {
        try
        {
            json = JsonSerializer.Serialize(value, value.GetType(), GetCachedSerializerOptions());
            return true;
        }
        catch (Exception exception)
        {
            json = $"Serialize Error: {exception.Message}";
            return false;
        }
    }

    private JsonSerializerOptions GetCachedSerializerOptions()
    {
        var options = _entry.Options;
        var cacheKey = $"{string.Join(",", options.IgnorePropertyNames)}|{string.Join(",", options.IgnorePropertyTypes.Select(type => type.FullName))}";
        return SerializerOptionsCache.GetOrAdd(cacheKey, _ => new JsonSerializerOptions
        {
            // 循环引用跳过、中文不转义：历史框架 LoggingMonitor 的序列化口径
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new MonitorLoggingJsonTypeInfoResolver(options.IgnorePropertyNames, options.IgnorePropertyTypes),
        });
    }

    private List<MonitorParameterSnapshot> CaptureParameters(ActionExecutingContext context, MonitorMethodMetadata metadata)
    {
        var snapshots = new List<MonitorParameterSnapshot>(context.ActionArguments.Count);
        foreach (var (name, value) in context.ActionArguments)
        {
            if (metadata.SuppressedParameterNames.Contains(name))
            {
                continue;
            }

            snapshots.Add(new MonitorParameterSnapshot
            {
                Name = name,
                Value = ConvertValueToJsonNode(value),
            });
        }

        return snapshots;
    }

    private static void WriteAuthorizationClaims(Utf8JsonWriter writer, ClaimsPrincipal user)
    {
        writer.WritePropertyName("authorizationClaims");
        if (user.Identity?.IsAuthenticated != true)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var claim in user.Claims)
        {
            writer.WriteStartObject();
            writer.WriteString("type", claim.Type);
            writer.WriteString("value", claim.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteParameters(Utf8JsonWriter writer, List<MonitorParameterSnapshot> snapshots)
    {
        writer.WritePropertyName("parameters");
        writer.WriteStartArray();
        foreach (var snapshot in snapshots)
        {
            writer.WriteStartObject();
            writer.WriteString("name", snapshot.Name);
            writer.WritePropertyName("value");
            if (snapshot.Value != null)
            {
                snapshot.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteNullValue();
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private void WriteReturnInformation(Utf8JsonWriter writer, IActionResult? result, HttpContext responseHttpContext, MonitorRuntimeSettings settings)
    {
        writer.WritePropertyName("returnInformation");
        writer.WriteStartObject();
        // 与历史框架同口径：action 过滤器阶段读取（结果尚未执行，成功路径为 200 语义）
        writer.WriteNumber("httpStatusCode", responseHttpContext.Response.StatusCode);

        writer.WritePropertyName("value");
        if (settings.WithReturnValue)
        {
            var returnValueNode = ConvertReturnValueToJsonNode(result);
            if (returnValueNode == null)
            {
                writer.WriteNullValue();
            }
            else if (settings.ReturnValueThreshold > 0)
            {
                var serialized = returnValueNode.ToJsonString();
                if (serialized.Length > settings.ReturnValueThreshold)
                {
                    // 超阈值截断为字符串，避免截断后的残缺 JSON 破坏行格式
                    writer.WriteStringValue(serialized[..settings.ReturnValueThreshold]);
                }
                else
                {
                    returnValueNode.WriteTo(writer);
                }
            }
            else
            {
                returnValueNode.WriteTo(writer);
            }
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WriteEndObject();
    }

    private JsonNode? ConvertReturnValueToJsonNode(IActionResult? result)
    {
        object? value = result switch
        {
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            ContentResult contentResult => contentResult.Content,
            _ => null,
        };

        if (value == null)
        {
            if (result is FileResult fileResult)
            {
                return new JsonObject
                {
                    ["fileName"] = fileResult.FileDownloadName,
                    ["contentType"] = fileResult.ContentType,
                };
            }

            return null;
        }

        return ConvertValueToJsonNode(value);
    }

    private static void WriteException(Utf8JsonWriter writer, Exception? exception)
    {
        writer.WritePropertyName("exception");
        if (exception == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("type", exception.GetType().FullName ?? exception.GetType().Name);
        writer.WriteString("message", exception.Message);
        writer.WriteString("stackTrace", exception.StackTrace);
        writer.WriteEndObject();
    }

    private static void WriteValidation(Utf8JsonWriter writer, JsonObject? validation)
    {
        writer.WritePropertyName("validation");
        if (validation == null)
        {
            writer.WriteNullValue();
            return;
        }

        validation.WriteTo(writer);
    }

    private static string ResolveControllerName(ControllerActionDescriptor descriptor, MethodInfo actionMethod)
    {
        if (descriptor.RouteValues.TryGetValue("controller", out var routeController) && !string.IsNullOrEmpty(routeController))
        {
            return routeController;
        }

        return actionMethod.DeclaringType?.Name ?? string.Empty;
    }

    /// <summary>
    /// 从特性路由模板解析 action 路由段（如 api/systemAuth/userInfo → userInfo）。
    /// 三表分流（lesson 2026-07-11）以该值为判据：userInfo/logout 命中进 visit 表。
    /// ponytail: 模板解析只认字面段、忽略尾部参数段（{id}），与 AdminApiRouteRules 生成的常规模板一致；
    /// 非常规模板回退到 RouteValues["action"]。
    /// </summary>
    private static string ResolveActionRouteSegment(ControllerActionDescriptor descriptor)
    {
        var template = (descriptor as ControllerActionDescriptor)?.AttributeRouteInfo?.Template;
        if (!string.IsNullOrWhiteSpace(template))
        {
            var segments = template.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (var index = segments.Length - 1; index >= 0; index--)
            {
                var segment = segments[index];
                var parameterStart = segment.IndexOf('{');
                if (parameterStart > 0)
                {
                    return segment[..parameterStart];
                }

                if (parameterStart < 0)
                {
                    return segment;
                }
            }
        }

        return descriptor.RouteValues.TryGetValue("action", out var routeAction) && routeAction != null ? routeAction : string.Empty;
    }

    /// <summary>
    /// 方法名去掉 Async 后缀作为 actionTypeName（DatabaseLoggingWriter 的登录租户归因判定 actionTypeName == "Login"
    /// 的配套口径：#189 路由锁定重命名把方法统一加了 Async 后缀，去后缀后与历史框架时代判定值一致）。
    /// </summary>
    private static string TrimAsyncSuffix(string methodName)
    {
        return methodName.EndsWith("Async", StringComparison.Ordinal) ? methodName[..^"Async".Length] : methodName;
    }

    private static string BuildRequestUrl(HttpRequest request)
    {
        var rawUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        return Uri.UnescapeDataString(rawUrl);
    }

    /// <summary>
    /// action 执行前采集的请求面快照（参数序列化在 action 改动入参前完成）。
    /// </summary>
    private sealed class MonitorCaptureState
    {
        public HttpContext HttpContext { get; set; } = null!;

        public List<MonitorParameterSnapshot> Parameters { get; set; } = new();

        public string ControllerName { get; set; } = string.Empty;

        public string ActionRouteName { get; set; } = string.Empty;

        public string ActionTypeName { get; set; } = string.Empty;

        public string DisplayTitle { get; set; } = string.Empty;

        public string HttpMethod { get; set; } = string.Empty;

        public string RequestUrl { get; set; } = string.Empty;

        public string UserAgent { get; set; } = string.Empty;

        public string RemoteIpAddress { get; set; } = string.Empty;

        public long ElapsedMilliseconds { get; set; }
    }

    private sealed class MonitorParameterSnapshot
    {
        public string Name { get; set; } = string.Empty;

        public JsonNode? Value { get; set; }
    }

    private sealed class MonitorMethodMetadata
    {
        public bool HasSuppressMonitor { get; set; }

        public string? DisplayTitle { get; set; }

        public HashSet<string> SuppressedParameterNames { get; set; } = new(StringComparer.Ordinal);
    }

    private static MonitorMethodMetadata BuildMethodMetadata(MethodInfo method)
    {
        var metadata = new MonitorMethodMetadata
        {
            HasSuppressMonitor = method.IsDefined(typeof(SuppressMonitorAttribute), true)
                                 || method.DeclaringType?.IsDefined(typeof(SuppressMonitorAttribute), true) == true,
            DisplayTitle = method.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName,
        };

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.IsDefined(typeof(SuppressMonitorAttribute), false))
            {
                metadata.SuppressedParameterNames.Add(parameter.Name ?? string.Empty);
            }
        }

        return metadata;
    }
}
