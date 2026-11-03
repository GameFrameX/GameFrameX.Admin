#!/usr/bin/env bash
# 默认自托管实例旧租户库 → 主库；停机、备份后运行，源库保持只读。
# 主库必须只有默认租户 1300000000001，源库最多一个应用；其他存量多租户库不支持。
# 环境变量：GFX_MIGRATION_DATABASE_TYPE=PostgreSQL（或 Sqlite），
# GFX_MIGRATION_SOURCE / GFX_MIGRATION_TARGET 为源/目标连接串，不打印凭据。
# --dry-run：真实执行建表/复制/冲突核对，事务回滚；--execute：事务提交。
# 已有同主键且相同的记录跳过；不同则整体回滚，不覆盖。已退场表打印未迁移清单。
# --self-test：仅临时 SQLite 文件，验证回滚、保真、幂等、冲突和多租户拒绝。
set -euo pipefail
SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ "$#" -ne 1 ]; then
    echo '必须显式指定 --dry-run、--execute 或 --self-test' >&2
    exit 64
fi
dotnet run --project "${SCRIPT_DIRECTORY}/migration/migration.csproj" -- "$1"
