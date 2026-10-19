using System.Dynamic;
using System.Reflection;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.System.Dictionary;
using GameFrameX.Core.Api;
using GameFrameX.Core.Service.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.Architecture;

// C352（GFX-770）：Admin API 端点参数形状契约测试。规范正文：gfx-doc/agent-handbook/admin-api-action-style.md §4 参数绑定（单一事实来源）。
// 规则（GFX-770 全量反射扫描方法论固化）：
//   合规形状 —— 0 参数；单简单类型（基元/enum/string/Guid/decimal/日期时间，GET 路径段特例）；
//              单 IFormFile / IFormFile 集合（multipart 文件上传原生形态，不可对象化）；
//              单 Request DTO（类型名以 Request/Input 结尾；泛型取泛型定义名，如 DataExportRequest<TQuery>）
//   违规形状 —— 参数 ≥ 2（散参数 / body+query 混合绑定）；
//              入参为 object/dynamic/ExpandoObject/IDynamicMetaObjectProvider（动态参数零容忍）；
//              入参为 *Entity（Entity 直传：TenantId/IsDeleted 等系统字段进入请求契约构成越权攻击面，C228 入参侧对偶）
// 扫描范围：三程序集（Application.System / Application.Game / Core.Service）全部 concrete IAdminApiController 实现，
//          方法枚举含继承链（覆盖 GenericCrudService<,,,,> 泛型族基类 action 的实例化入参），
//          过滤 [NonAction] / IsSpecialName / object 声明；违规按声明类名归因（泛型取泛型定义名）。
// 双向断言：违规 ⊆ 例外清单（防新增）且例外清单 ⊆ 违规（防僵尸条目，参照 AdminActionStyleContractTests 模式）；例外清单只减不增。
public sealed class AdminEndpointParameterShapeContractTests
{
    // ============ 例外清单：issue 4 条整改完成后的存量（GFX-770 清单外新确认，后续独立 change 清零） ============

    private static readonly HashSet<string> ParameterShapeLegacy = new(StringComparer.Ordinal)
    {
        // 裸 List<long> 入参（批量删除参数配置；整改须收敛 BatchDeleteConfigRequest 并同步前端）
        "SystemConfigService.BatchDeleteConfigAsync",
        // 文件 + query 元数据混合绑定（文件上传特例；整改需配套前端 multipart 适配）
        "TenantFileService.UploadFileAsync",
        // 3 散参（message/title/isHtml，平台系统邮件发送）
        "SystemMessageService.SendEmailAsync",
        // Entity 直传入参（TenantUserEntity，与 GFX-770 已整改的 TenantWechatUser 同性质越权攻击面）
        "TenantUserService.UpdateBaseInfoAsync",
    };

    [Fact]
    public void AdminEndpoints_ParameterShape_MatchesContract()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateAdminServiceTypes())
        {
            // 含继承链：GetMethods 不带 DeclaredOnly，泛型基类（GenericCrudService 族）action 以构造后形态出现
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.IsSpecialName || method.DeclaringType == typeof(object))
                {
                    continue;
                }

                if (method.IsDefined(typeof(NonActionAttribute), false))
                {
                    continue;
                }

                var violation = DescribeParameterShapeViolation(method.GetParameters());
                if (violation != null)
                {
                    actual.Add(GetDeclaringTypeName(method) + "." + method.Name + " —— " + violation);
                }
            }
        }

        AssertNoNewAndNoStale(actual, ParameterShapeLegacy);
    }

    /// <summary>返回违规描述；合规返回 null。规则见类头注释。</summary>
    private static string? DescribeParameterShapeViolation(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
        {
            return null;
        }

        if (parameters.Length >= 2)
        {
            return "散参数/混合绑定（参数数 " + parameters.Length + "，须收敛为单一 Request 对象）";
        }

        var parameterType = parameters[0].ParameterType;

        if (IsDynamicShape(parameterType))
        {
            return "动态参数（object/dynamic/ExpandoObject 零容忍）";
        }

        if (IsFileUploadShape(parameterType))
        {
            return null;
        }

        if (IsSimpleType(parameterType))
        {
            return null;
        }

        var typeName = GetShapeTypeName(parameterType);
        if (typeName.EndsWith("Entity", StringComparison.Ordinal))
        {
            return "Entity 直传入参（系统字段进入请求契约，越权攻击面；须拆 Request DTO）";
        }

        if (typeName.EndsWith("Request", StringComparison.Ordinal) || typeName.EndsWith("Input", StringComparison.Ordinal))
        {
            return null;
        }

        return "入参非对象化 Request DTO（" + typeName + " 不符合 *Request/*Input 命名约定）";
    }

    private static bool IsDynamicShape(Type type)
    {
        return type == typeof(object)
               || type == typeof(ExpandoObject)
               || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
    }

    private static bool IsFileUploadShape(Type type)
    {
        if (typeof(IFormFile).IsAssignableFrom(type))
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        if (definition != typeof(List<>) && definition != typeof(IList<>) && definition != typeof(IEnumerable<>) && definition != typeof(ICollection<>))
        {
            return false;
        }

        return typeof(IFormFile).IsAssignableFrom(type.GetGenericArguments()[0]);
    }

    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
               || underlying.IsEnum
               || underlying == typeof(string)
               || underlying == typeof(decimal)
               || underlying == typeof(Guid)
               || underlying == typeof(DateTime)
               || underlying == typeof(DateTimeOffset)
               || underlying == typeof(TimeSpan);
    }

    /// <summary>形状判型用类型名：泛型（含构造形态）取泛型定义名并剥 arity 后缀，保证 DataExportRequest&lt;TQuery&gt; → DataExportRequest。</summary>
    private static string GetShapeTypeName(Type type)
    {
        var name = type.IsGenericType ? type.GetGenericTypeDefinition().Name : type.Name;
        var tickIndex = name.IndexOf('`');
        return tickIndex < 0 ? name : name[..tickIndex];
    }

    /// <summary>违规归因到声明类：泛型声明类取泛型定义名（GenericCrudService`5 → GenericCrudService），例外清单不随子类实例化漂移。</summary>
    private static string GetDeclaringTypeName(MethodInfo method)
    {
        var declaringType = method.DeclaringType!;
        if (declaringType.IsGenericType)
        {
            return declaringType.GetGenericTypeDefinition().Name;
        }

        return declaringType.Name;
    }

    private static IEnumerable<Type> EnumerateAdminServiceTypes()
    {
        var assemblies = new[]
        {
            typeof(GlobalDictionaryTypeService).Assembly, // GameFrameX.Application.System
            typeof(TenantGamePenaltyEntityService).Assembly, // GameFrameX.Application.Game
            typeof(BaseService<>).Assembly, // GameFrameX.Core
        };
        foreach (var assembly in assemblies.Distinct())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition
                    && typeof(IAdminApiController).IsAssignableFrom(type)
                    && type.Assembly == assembly)
                {
                    yield return type;
                }
            }
        }
    }

    private static void AssertNoNewAndNoStale(HashSet<string> actual, HashSet<string> expected)
    {
        // 例外清单只存 TypeName.MethodName 锚点；actual 带违规描述，比较前先剥后缀
        var actualKeys = actual.Select(StripViolationDetail).ToHashSet(StringComparer.Ordinal);
        var detailed = actual.Order(StringComparer.Ordinal).ToList();

        var newViolations = actualKeys.Except(expected).Order(StringComparer.Ordinal).ToList();
        Assert.True(
            newViolations.Count == 0,
            "Admin 端点参数形状违规（规范 §4：入参必须对象化——单 Request DTO / 单简单类型 / 单文件参数；散参数、动态参数、Entity 直传零容忍）。违规明细:" +
            Environment.NewLine + string.Join(Environment.NewLine, detailed));

        var staleExceptions = expected.Except(actualKeys).Order(StringComparer.Ordinal).ToList();
        Assert.True(
            staleExceptions.Count == 0,
            "参数形状例外清单存在已修复的僵尸条目（存量清理后必须同步删除对应例外项）:" + Environment.NewLine + string.Join(Environment.NewLine, staleExceptions));
    }

    private static string StripViolationDetail(string key)
    {
        var separatorIndex = key.IndexOf(" —— ", StringComparison.Ordinal);
        return separatorIndex < 0 ? key : key[..separatorIndex];
    }
}
