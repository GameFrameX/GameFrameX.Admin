using System.Data;
using System.Globalization;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal sealed class LegacyRiskPolicyMigration
{
    private const string ApplicationTable = "tenant_game_app_entity";
    private const string PolicyTable = "tenant_client_api_policy_entity";
    private readonly long _applicationId;
    private readonly bool _signatureRequired;
    private readonly bool _deviceProofRequired;
    private readonly string? _riskPolicy;
    private bool _targetNeedsRiskColumns;

    private LegacyRiskPolicyMigration(DataRow row)
    {
        _applicationId = Convert.ToInt64(row["id"], CultureInfo.InvariantCulture);
        _signatureRequired = Convert.ToBoolean(row["signature_required"], CultureInfo.InvariantCulture);
        _deviceProofRequired = Convert.ToBoolean(row["device_proof_required"], CultureInfo.InvariantCulture);
        _riskPolicy = row["risk_policy"] is DBNull ? null : Convert.ToString(row["risk_policy"], CultureInfo.InvariantCulture);
    }

    internal static LegacyRiskPolicyMigration? Prepare(ISqlSugarClient source, ISqlSugarClient target)
    {
        if (!source.DbMaintenance.IsAnyTable(ApplicationTable, false))
        {
            return null;
        }

        var rows = source.Ado.GetDataTable($"SELECT id, signature_required, device_proof_required, risk_policy FROM {ApplicationTable} WHERE is_deleted = false ORDER BY id");
        if (rows.Rows.Count == 0)
        {
            return null;
        }

        var migration = new LegacyRiskPolicyMigration(rows.Rows[0]);
        foreach (DataRow row in rows.Rows)
        {
            migration.RequireSame(row, $"旧应用 {row["id"]}");
        }

        // 在建表补列前核对已经存在的风险值，不能把用户设置误判为新增列默认值。
        migration.ValidateExistingPolicy(source, "源库");
        migration.ValidateExistingPolicy(target, "目标库");
        migration._targetNeedsRiskColumns = target.DbMaintenance.IsAnyTable(PolicyTable, false)
            && !target.DbMaintenance.GetColumnInfosByTableName(PolicyTable, false).Any(column => column.DbColumnName == "risk_policy");
        return migration;
    }

    internal void CompleteLegacyTarget(ISqlSugarClient target)
    {
        if (!_targetNeedsRiskColumns)
        {
            return;
        }

        target.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
        target.Updateable<TenantClientApiPolicyEntity>()
            .SetColumns(row => new TenantClientApiPolicyEntity
            {
                SignatureRequired = _signatureRequired,
                DeviceProofRequired = _deviceProofRequired,
                RiskPolicy = _riskPolicy!,
            }).Where(row => row.IsDeleted == false && row.IsEnabled == true).ExecuteCommand();
    }

    internal void EnrichCopiedPolicy(string tableName, DataTable rows)
    {
        if (!string.Equals(tableName, PolicyTable, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var values = new (string Name, Type Type, object Value)[]
        {
            ("signature_required", typeof(bool), _signatureRequired),
            ("device_proof_required", typeof(bool), _deviceProofRequired),
            ("risk_policy", typeof(string), (object?)_riskPolicy ?? DBNull.Value),
        };
        foreach (var value in values)
        {
            if (rows.Columns.Contains(value.Name))
            {
                continue;
            }

            rows.Columns.Add(value.Name, value.Type);
            foreach (DataRow row in rows.Rows)
            {
                row[value.Name] = value.Value;
            }
        }
    }

    internal void Apply(ISqlSugarClient target)
    {
        target.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
        var policies = target.Queryable<TenantClientApiPolicyEntity>().Where(row => row.IsDeleted == false).ToList();
        var enabled = policies.Where(row => row.IsEnabled == true).ToArray();
        if (policies.Count > 0 && enabled.Length != 1)
        {
            throw new InvalidOperationException("旧应用风险配置需要唯一启用策略；现有策略均禁用或有多条启用记录，拒绝自动变更开关。");
        }

        if (policies.Count == 0)
        {
            // 旧实例可以只有应用风险设置而无运行策略；沿用应用主键，不生成随机迁移身份。
            target.Insertable(new TenantClientApiPolicyEntity
            {
                Id = _applicationId,
                IsEnabled = true,
                SignatureRequired = _signatureRequired,
                DeviceProofRequired = _deviceProofRequired,
                RiskPolicy = _riskPolicy!,
            }).ExecuteCommand();
        }
        else
        {
            target.Updateable<TenantClientApiPolicyEntity>()
                .SetColumns(row => new TenantClientApiPolicyEntity
                {
                    SignatureRequired = _signatureRequired,
                    DeviceProofRequired = _deviceProofRequired,
                    RiskPolicy = _riskPolicy!,
                }).Where(row => row.Id == enabled[0].Id).ExecuteCommand();
        }

        Console.WriteLine($"旧应用 {_applicationId} 风险配置带入：SignatureRequired={_signatureRequired}, DeviceProofRequired={_deviceProofRequired}, RiskPolicy={System.Text.Json.JsonSerializer.Serialize(_riskPolicy)}");
    }

    private void ValidateExistingPolicy(ISqlSugarClient database, string description)
    {
        if (!database.DbMaintenance.IsAnyTable(PolicyTable, false))
        {
            return;
        }

        var columns = database.DbMaintenance.GetColumnInfosByTableName(PolicyTable, false)
            .Select(column => column.DbColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var count = new[] { "signature_required", "device_proof_required", "risk_policy" }.Count(columns.Contains);
        if (count == 0)
        {
            return;
        }

        if (count != 3)
        {
            throw new InvalidOperationException($"{description}策略的风险字段不完整，拒绝自动迁移。");
        }

        var rows = database.Ado.GetDataTable($"SELECT id, signature_required, device_proof_required, risk_policy FROM {PolicyTable} WHERE is_deleted = false AND is_enabled = true");
        foreach (DataRow row in rows.Rows)
        {
            RequireSame(row, $"{description}策略 {row["id"]}");
        }
    }

    private void RequireSame(DataRow row, string description)
    {
        var candidate = new LegacyRiskPolicyMigration(row);
        if (_signatureRequired != candidate._signatureRequired || _deviceProofRequired != candidate._deviceProofRequired
            || !string.Equals(_riskPolicy, candidate._riskPolicy, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"风险配置冲突：旧应用 {_applicationId} ({_signatureRequired}, {_deviceProofRequired}, {System.Text.Json.JsonSerializer.Serialize(_riskPolicy)}) 与 {description} ({candidate._signatureRequired}, {candidate._deviceProofRequired}, {System.Text.Json.JsonSerializer.Serialize(candidate._riskPolicy)})；拒绝迁移，未覆盖风险配置。");
        }
    }
}
