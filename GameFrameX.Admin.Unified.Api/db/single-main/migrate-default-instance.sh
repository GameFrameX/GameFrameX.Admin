#!/usr/bin/env bash
# 默认自托管实例旧租户库 → 主库；停机、备份后运行，源库保持只读。
# 主库必须只有默认租户 1300000000001，源库最多一个应用；其他存量多租户库不支持。
# 环境变量：GFX_MIGRATION_DATABASE_TYPE=PostgreSQL（或 Sqlite），
# GFX_MIGRATION_SOURCE / GFX_MIGRATION_TARGET 为源/目标连接串，不打印凭据。
# --dry-run：真实执行退场列删除/业务索引重建/建表/复制/冲突核对，事务回滚；--execute：事务提交。
# 已有同主键且相同的记录跳过；不同则整体回滚，不覆盖。源/目标退场表打印残留清单，不自动 DROP。
# 确认迁移与备份、重复执行完成后可手动 DROP；system_tenant_entity 可能含加密的旧连接串，按敏感凭据处理。
# --self-test-postgres：使用显式 GFX_TEST_PG_CONNECTION_STRING，创建并删除临时源/目标库验证真实 PostgreSQL。
# --redact-logs-dry-run / --redact-logs：仅需目标连接；预览受影响行数 / 事务清理主库既有日志，重跑幂等。
# --self-test：仅临时 SQLite 文件，验证回滚、保真、幂等、冲突和多租户拒绝。
set -euo pipefail
SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ "$#" -ne 1 ]; then
    echo '必须显式指定 --dry-run、--execute、--self-test 或 --self-test-postgres' >&2
    exit 64
fi
dotnet run --project "${SCRIPT_DIRECTORY}/migration/migration.csproj" -- "$1"
