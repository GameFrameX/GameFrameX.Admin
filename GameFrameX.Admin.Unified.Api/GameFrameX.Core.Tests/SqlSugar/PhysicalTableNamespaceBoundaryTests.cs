using System.Reflection;
using GameFrameX.Client.Api.Integrations.Advertisement.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

public class PhysicalTableNamespaceBoundaryTests
{
    private static readonly string[] JsonRenameAttributeNames =
    {
        "System.Text.Json.Serialization.JsonPropertyNameAttribute",
        "Newtonsoft.Json.JsonPropertyAttribute",
        "System.Runtime.Serialization.DataMemberAttribute",
    };

    [Fact]
    public void AllEntities_HavePhysicalNamespacePrefix()
    {
        var failures = EntityTypes()
                       .Select(type => new
                       {
                           Type = type,
                           TableName = SqlSugarEntityBoundary.GetPhysicalTableName(type, type.GetCustomAttribute<SugarTable>()?.TableName ?? type.Name),
                       })
                       .Where(item => !HasAllowedPrefix(item.TableName))
                       .Select(item => $"{item.Type.FullName} -> {item.TableName}")
                       .ToList();

        Assert.Empty(failures);
    }

    [Fact]
    public void AllEntities_DoNotCarryLegacySysPrefixAfterBoundaryPrefix()
    {
        var failures = EntityTypes()
                       .Select(type => new
                       {
                           Type = type,
                           TableName = SqlSugarEntityBoundary.GetPhysicalTableName(type, type.GetCustomAttribute<SugarTable>()?.TableName ?? type.Name),
                       })
                       .Where(item => item.TableName.Contains("_sys_", StringComparison.Ordinal))
                       .Select(item => $"{item.Type.FullName} -> {item.TableName}")
                       .ToList();

        Assert.Empty(failures);
    }

    [Fact]
    public void PhysicalTablePrefix_IsRestrictedByDatabaseOwnership()
    {
        Assert.True(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.System, "system_user"));
        Assert.True(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.System, "global_menu"));
        Assert.False(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.System, "tenant_user"));

        Assert.True(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.Tenant, "tenant_user"));
        Assert.False(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.Tenant, "system_user"));
        Assert.False(SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership.Tenant, "global_menu"));
    }

    [Fact]
    public void EntityDerivedClasses_DoNotRedeclareBaseProperties()
    {
        var failures = EntityTypes()
                       .SelectMany(type => RedeclaredBaseProperties(type).Select(property => $"{type.FullName}.{property}"))
                       .ToList();

        Assert.Empty(failures);
    }

    [Fact]
    public void Dtos_DoNotUseJsonRenameAttributes()
    {
        var failures = DtoTypes()
                       .Where(type => !IsThirdPartyContractMirror(type))
                       .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                               .Where(HasJsonRenameAttribute)
                                               .Select(property => $"{type.FullName}.{property.Name}"))
                       .ToList();

        Assert.Empty(failures);
    }

    /// <summary>
    /// 第三方契约镜像豁免（C292 / GFX-698）：镜像 DTO 属性 PascalCase 化后必须用 JsonPropertyName 锁定第三方 wire 名，
    /// 「自家 DTO 属性名即 wire 名」约束不适用。豁免区：Client.Api Integrations 集成区（C220 约定）、
    /// Hub.Api WebHook 镜像区、zlpay 网关回传镜像类型。
    /// </summary>
    private static bool IsThirdPartyContractMirror(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        return ns.StartsWith("GameFrameX.Client.Api.Integrations.", StringComparison.Ordinal)
               || ns.StartsWith("GameFrameX.Hub.Api.Dto.WebHook", StringComparison.Ordinal)
               || type.FullName == "GameFrameX.Client.Api.GameWebShop.Dto.TenantGameWebShopEntityBuyNotifyRequest";
    }

    private static IEnumerable<Type> EntityTypes()
    {
        return typeof(SystemUserBase).Assembly.GetTypes()
                                     .Where(type => type is { IsClass: true, IsAbstract: false, }
                                                    && type.IsDefined(typeof(SugarTable), false));
    }

    private static IEnumerable<Type> DtoTypes()
    {
        var assemblies = new[]
        {
            typeof(BaseSelectPageResponse).Assembly,
            typeof(SystemAuthService).Assembly,
            typeof(AdvertisementPangleGlobalGameEntityRequest).Assembly,
            LoadAssembly("GameFrameX.Application"),
            LoadAssembly("GameFrameX.Application.Game"),
            LoadAssembly("GameFrameX.Application.System"),
            LoadAssembly("GameFrameX.Hub.Api"),
        }.Where(assembly => assembly != null).Distinct();

        return assemblies.SelectMany(assembly => assembly!.GetTypes())
                         .Where(type => type is { IsClass: true, IsAbstract: false, }
                                        && (type.Name.EndsWith("Request", StringComparison.Ordinal)
                                            || type.Name.EndsWith("Response", StringComparison.Ordinal)
                                            || type.Name.EndsWith("Dto", StringComparison.Ordinal)
                                            || (type.Namespace?.Contains(".Dto", StringComparison.Ordinal) ?? false)));
    }

    private static Assembly? LoadAssembly(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<string> RedeclaredBaseProperties(Type type)
    {
        var baseNames = new HashSet<string>(StringComparer.Ordinal);
        var current = type.BaseType;
        while (current != null && current != typeof(object))
        {
            foreach (var property in current.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                baseNames.Add(property.Name);
            }

            current = current.BaseType;
        }

        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                   .Where(property => baseNames.Contains(property.Name))
                   .Select(property => property.Name);
    }

    private static bool HasJsonRenameAttribute(PropertyInfo property)
    {
        return property.CustomAttributes.Any(attribute =>
        {
            if (!JsonRenameAttributeNames.Contains(attribute.AttributeType.FullName, StringComparer.Ordinal))
            {
                return false;
            }

            if (attribute.AttributeType.FullName == "System.Runtime.Serialization.DataMemberAttribute")
            {
                return attribute.NamedArguments.Any(argument => argument.MemberName == "Name");
            }

            return true;
        });
    }

    private static bool HasAllowedPrefix(string tableName)
    {
        return tableName.StartsWith("system_", StringComparison.Ordinal)
               || tableName.StartsWith("tenant_", StringComparison.Ordinal)
               || tableName.StartsWith("global_", StringComparison.Ordinal);
    }
}