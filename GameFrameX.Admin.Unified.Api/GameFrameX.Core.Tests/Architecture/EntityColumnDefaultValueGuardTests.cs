using System.Reflection;

namespace GameFrameX.Core.Tests.Architecture;

// 2026-09-11 列默认值守护测试（password_configured 23502 事故政策收口；GFX-746 / C337 起全量强制）。
// 政策：实体非 nullable 值类型列必须带 SugarColumn.DefaultValue——PG 11+ 加带 DEFAULT 的 NOT NULL 列
// 是零重写元数据操作，存量行直接读默认值，CodeFirst 演化不再触发 23502（两次事故：2026-07 global_menu
// 四列 EnsureColumn 修补、2026-09-11 password_configured）。规范见
// gfx-doc/decisions/2026-09-11-codefirst-column-default-policy.md。
// 322 条存量违规已于 GFX-746（2026-09-11）全量补 DefaultValue 清零，基线快照文件与只减不增豁免机制随之删除；
// 本测试自此严格断言零违规。真正可选的业务列走 IsNullable = true + C# 可空类型（bool?/int?），不进本守护范围。
// DDL 行为回归（存量库加列不炸、老行默认值、存量 NULL 回填）由 CodeFirstSchemaEvolutionPostgresTests 连真实 PG 锁定。
public sealed class EntityColumnDefaultValueGuardTests
{
    [Fact]
    public void NonNullableValueTypeColumns_CarryDefaultValue()
    {
        var violations = CollectViolations();

        Assert.True(
            violations.Count == 0,
            "存在非 nullable 值类型列未带 DefaultValue（2026-09-11 政策：PG 存量库加 NOT NULL 无默认值列必炸 23502）。"
            + "修复方式：必有值列补 DefaultValue = \"...\"；真可选列改 IsNullable = true + 可空类型（bool?/int?）。违规清单：" + Environment.NewLine
            + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// 收集全部 [SugarTable] 实体的违规列：非 nullable 值类型 + 无 DefaultValue + 非 IsNullable 显式可空
    /// + 非主键/自增/忽略/导航属性。行格式：Assembly|TypeFullName|PropertyName。
    /// </summary>
    private static HashSet<string> CollectViolations()
    {
        var violations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assemblyPath in EnumerateEntityAssemblies())
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath));
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (type.GetCustomAttribute(typeof(global::SqlSugar.SugarTable), false) == null)
                {
                    continue;
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (IsViolation(type, property))
                    {
                        violations.Add(assemblyName + "|" + type.FullName + "|" + property.Name);
                    }
                }
            }
        }

        return violations;
    }

    private static bool IsViolation(Type entityType, PropertyInfo property)
    {
        // Foundation 基类审计列（Id/CreatedTime/...）属外部包，不在治理范围（同 NamingAbbreviationGuardTests 口径）
        if (property.DeclaringType == null || property.DeclaringType.Assembly != entityType.Assembly)
        {
            return false;
        }

        var propertyType = property.PropertyType;

        // 仅值类型（bool/int/long/enum/Guid/DateTime/...）；Nullable<T> 与引用类型不在本守护范围
        if (!propertyType.IsValueType || propertyType.IsGenericType)
        {
            return false;
        }

        var sugarColumn = property.GetCustomAttribute(typeof(global::SqlSugar.SugarColumn), false) as global::SqlSugar.SugarColumn;
        if (sugarColumn == null)
        {
            // 无 SugarColumn 特性的值类型属性同样映射为 NOT NULL 列（如基类审计列），纳入守护
            return property.GetCustomAttribute(typeof(global::SqlSugar.Navigate), false) == null;
        }

        if (sugarColumn.IsNullable)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(sugarColumn.DefaultValue))
        {
            return false;
        }

        if (sugarColumn.IsPrimaryKey || sugarColumn.IsIdentity || sugarColumn.IsIgnore)
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<string> EnumerateEntityAssemblies()
    {
        // 实体层程序集（GameFrameX.Entity / GameFrameX.Entity.*）；其余程序集的 [SugarTable] 实体若有同加
        return Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX.Entity*.dll");
    }
}
