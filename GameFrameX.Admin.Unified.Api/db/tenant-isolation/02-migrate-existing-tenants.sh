#!/usr/bin/env bash
# 02-migrate-existing-tenants.sh
# GameFrameX Admin 租户库完全隔离 · P2 存量租户库迁移（GFX-727 / C318）
#
# 以主库 system_tenant_entity 注册行为权威驱动，逐租户迁移到目标形态：
#   1. 租户独立 role：不存在则 CREATE ROLE（随机密码）；已存在则重置密码（存量明文密码视为已泄漏形态）
#   2. 租户库内 REASSIGN OWNED BY gameframex TO <tenant_user>（把平台 superuser 名下的表/序列等对象移交租户账号）
#   3. ALTER DATABASE <tenant_db> OWNER TO <tenant_user>
#   4. REVOKE CONNECT ON DATABASE <tenant_db> FROM PUBLIC + GRANT CONNECT,TEMPORARY TO <tenant_user>
#   5. 生成新连接串（继承原串 Host/Port/Database 等，仅替换 Username/Password）→ enc:v1: 加密回写 connection
#   6. 默认租户（--default-tenant-id）额外打印明文新串，供部署侧 GFX_DEFAULT_TENANT_DB_* 显式配置
#      （REVOKE PUBLIC 后主库 gameframex 账号无法再连默认租户库，推导连接必须同步切换，见 README）
#
# 环境变量（必须）：
#   PGUSER / PGPASSWORD          —— 平台 superuser 凭据（REASSIGN OWNED BY 需要 superuser）
#   GFX_TENANT_CONN_ENC_KEY      —— 与三 host 共享的加密密钥（C317 语义）
# 可选环境变量：
#   PSQL_CMD                     —— psql 调用包装，默认裸 psql；宿主机无客户端时例如
#                                  PSQL_CMD="docker exec -i gameframex-postgres psql"
#   TENANT_DB_PREFIX / TENANT_USER_PREFIX / MAIN_DB / DEFAULT_TENANT_ID / PLATFORM_ROLE —— 见下方默认值
# 用法：
#   ./02-migrate-existing-tenants.sh --dry-run   # 预演：只打印将执行的语句（密码掩码），不落任何变更
#   ./02-migrate-existing-tenants.sh             # 真执行
#
# 连接参数说明：使用 docker exec 形态（PSQL_CMD）时连接目标由包装决定，PGHOST/PGPORT 可省；
#   使用裸 psql 时需 PGHOST/PGPORT/PGUSER/PGPASSWORD 指向目标实例。
#
# 幂等性：任意步骤中断后重跑安全（密码重置 + REASSIGN 无对象时无操作 + UPDATE 幂等）。
# 敏感提示：默认租户的明文新串会打印到 stdout，请重定向到受控文件，勿入版本库/工单。

set -euo pipefail

TENANT_DB_PREFIX="${TENANT_DB_PREFIX:-gameframex_admin_tenant_}"
TENANT_USER_PREFIX="${TENANT_USER_PREFIX:-gameframex_admin_tenant_user_}"
MAIN_DB="${MAIN_DB:-gameframex_admin}"
DEFAULT_TENANT_ID="${DEFAULT_TENANT_ID:-1300000000001}"
PLATFORM_ROLE="${PLATFORM_ROLE:-gameframex}"
DRY_RUN=0
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENC_TOOL_PROJECT="$SCRIPT_DIR/encrypt-connection"

for arg in "$@"; do
    if [ "$arg" = "--dry-run" ]; then
        DRY_RUN=1
    else
        echo "未知参数：${arg}（仅支持 --dry-run；前缀/库名等经环境变量配置）" >&2
        exit 64
    fi
done

if [ -z "${PGUSER:-}" ] || [ -z "${PGPASSWORD:-}" ]; then
    echo "错误：PGUSER / PGPASSWORD 必须设置（平台 superuser 凭据）。" >&2
    exit 64
fi
if [ -z "${GFX_TENANT_CONN_ENC_KEY:-}" ]; then
    echo "错误：GFX_TENANT_CONN_ENC_KEY 必须设置，且与三 host 共享同一密钥（C317 语义）。" >&2
    exit 64
fi

# psql 客户端包装：按空格拆成数组执行（支持 docker exec 复合形态）
read -ra PSQL_CMD_ARR <<< "${PSQL_CMD:-psql}"
for bin in openssl dotnet "${PSQL_CMD_ARR[0]}"; do
    if ! command -v "$bin" > /dev/null 2>&1; then
        echo "错误：缺少依赖命令 ${bin}。" >&2
        exit 65
    fi
done

# 统一 SQL 执行入口：$1=人读描述（掩码形态），$2=目标库名，$3=真实 SQL
run_sql() {
    if [ "$DRY_RUN" = "1" ]; then
        echo "[dry-run] [$2] $1"
    else
        echo "[exec] [$2] $1"
        # < /dev/null：隔离 stdin，防止 docker exec -i 之类包装吞掉外层循环的 heredoc 输入
        "${PSQL_CMD_ARR[@]}" -X -v ON_ERROR_STOP=1 -d "$2" -c "$3" < /dev/null
    fi
}

psql_scalar() {
    "${PSQL_CMD_ARR[@]}" -X -v ON_ERROR_STOP=1 -d "$1" -t -A -c "$2" < /dev/null
}

MODE_DESC="$([ "$DRY_RUN" = "1" ] && echo dry-run || echo execute)"
echo "== 存量租户库迁移开始（mode=${MODE_DESC}，user=${PGUSER}，main-db=${MAIN_DB}）=="

# 预检：迁移入口必须以 superuser 运行（REASSIGN OWNED BY 平台账号需要）
IS_SUPERUSER="$(psql_scalar postgres "SELECT rolsuper FROM pg_roles WHERE rolname = current_user")"
if [ "$IS_SUPERUSER" != "t" ]; then
    echo "错误：迁移必须以 superuser 执行（当前 ${PGUSER} rolsuper=${IS_SUPERUSER}）。" >&2
    exit 1
fi

# 前置：connection 列宽对齐实体声明（EntityStringLength.Length512）。存量表若建于 C281 列宽翻倍之前，
# 实际列宽仍为 varchar(256)，enc:v1: 密文（≈1.33×明文 + salt/nonce/tag 开销 + 前缀）会超宽导致回写失败。
CONN_LEN="$(psql_scalar "$MAIN_DB" "SELECT COALESCE(character_maximum_length, 0) FROM information_schema.columns WHERE table_name = 'system_tenant_entity' AND column_name = 'connection'")"
if [ "${CONN_LEN:-0}" -lt 512 ]; then
    run_sql "ALTER TABLE system_tenant_entity ALTER COLUMN connection TYPE varchar(512)（列宽对齐实体 Length512）" "$MAIN_DB" \
        "ALTER TABLE system_tenant_entity ALTER COLUMN connection TYPE varchar(512)"
fi

TENANT_ROWS="$(psql_scalar "$MAIN_DB" "SELECT id || '|' || COALESCE(connection, '') FROM system_tenant_entity WHERE is_deleted = false ORDER BY id")"
if [ -z "$TENANT_ROWS" ]; then
    echo "主库无注册租户（is_deleted=false），无可迁移对象。"
    exit 0
fi

FAILED_IDS=""
while IFS='|' read -r TENANT_ID OLD_CONN; do
    TENANT_DB="${TENANT_DB_PREFIX}${TENANT_ID}"
    TENANT_USER="${TENANT_USER_PREFIX}${TENANT_ID}"
    echo ""
    echo "---- 租户 ${TENANT_ID}（库 ${TENANT_DB} / role ${TENANT_USER}）----"

    # 库必须存在且与注册行连接串中的库名一致，否则视为数据漂移，人工介入
    DB_EXISTS="$(psql_scalar postgres "SELECT count(*) FROM pg_database WHERE datname = '${TENANT_DB}'")"
    if [ "$DB_EXISTS" != "1" ]; then
        echo "!! 租户 ${TENANT_ID} 注册库 ${TENANT_DB} 不存在，跳过（请人工核查数据漂移）。" >&2
        FAILED_IDS="${FAILED_IDS} ${TENANT_ID}"
        continue
    fi
    if ! printf '%s' "$OLD_CONN" | grep -q "Database=${TENANT_DB}"; then
        echo "!! 租户 ${TENANT_ID} 连接串库名与注册前缀推导不一致，跳过（connection 已掩码）。" >&2
        FAILED_IDS="${FAILED_IDS} ${TENANT_ID}"
        continue
    fi

    NEW_PASSWORD="$(openssl rand -hex 24)"
    MASKED_PASSWORD='Password=<random 48-hex>'

    ROLE_EXISTS="$(psql_scalar postgres "SELECT count(*) FROM pg_roles WHERE rolname = '${TENANT_USER}'")"
    if [ "$ROLE_EXISTS" = "1" ]; then
        run_sql "ALTER ROLE ${TENANT_USER} PASSWORD '<random 重置>'" postgres \
            "ALTER ROLE \"${TENANT_USER}\" WITH PASSWORD '${NEW_PASSWORD}'"
    else
        run_sql "CREATE ROLE ${TENANT_USER} LOGIN PASSWORD '<random>'" postgres \
            "CREATE ROLE \"${TENANT_USER}\" WITH LOGIN PASSWORD '${NEW_PASSWORD}'"
    fi

    # 平台账号名下业务对象显式转移（表/分区表/序列/视图/物化视图/外部表；索引随表自动转移）。
    # 不用 REASSIGN OWNED BY：平台账号通常是集群 bootstrap superuser（POSTGRES_USER 初始化账号），
    # 其名下含 information_schema / pg_catalog 系统对象，REASSIGN 会报
    # "cannot reassign ownership of objects ... required by the database system" 而整体失败。
    run_sql "转移 ${PLATFORM_ROLE} 名下业务对象 OWNER TO ${TENANT_USER}（public 等业务 schema，DO 块幂等）" "$TENANT_DB" \
        "DO \$transfer\$ DECLARE r record; BEGIN FOR r IN SELECT n.nspname AS nsp, c.relname AS rel, c.relkind AS kind FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname NOT IN ('pg_catalog','information_schema','pg_toast') AND n.nspname NOT LIKE 'pg\\_%' AND pg_get_userbyid(c.relowner)='${PLATFORM_ROLE}' AND c.relkind IN ('r','p','S','v','m','f') LOOP IF r.kind='S' THEN EXECUTE format('ALTER SEQUENCE %I.%I OWNER TO %I', r.nsp, r.rel, '${TENANT_USER}'); ELSE EXECUTE format('ALTER TABLE %I.%I OWNER TO %I', r.nsp, r.rel, '${TENANT_USER}'); END IF; END LOOP; END \$transfer\$;"

    # 转移后就地断言：业务 schema 内平台账号名下对象残留必须为 0（系统 schema 对象不在此口径）
    RESIDUAL="$(psql_scalar "$TENANT_DB" "SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname NOT IN ('pg_catalog','information_schema','pg_toast') AND n.nspname NOT LIKE 'pg_%' AND pg_get_userbyid(c.relowner)='${PLATFORM_ROLE}' AND c.relkind IN ('r','p','S','v','m','f')")"
    if [ "$RESIDUAL" != "0" ]; then
        echo "!! 租户 ${TENANT_ID} 对象转移后仍有 ${RESIDUAL} 个业务对象属于 ${PLATFORM_ROLE}，中止该租户后续步骤。" >&2
        FAILED_IDS="${FAILED_IDS} ${TENANT_ID}"
        continue
    fi

    run_sql "ALTER DATABASE ${TENANT_DB} OWNER TO ${TENANT_USER}" postgres \
        "ALTER DATABASE \"${TENANT_DB}\" OWNER TO \"${TENANT_USER}\""

    run_sql "REVOKE CONNECT ON DATABASE ${TENANT_DB} FROM PUBLIC + GRANT CONNECT,TEMPORARY TO ${TENANT_USER}" postgres \
        "REVOKE CONNECT ON DATABASE \"${TENANT_DB}\" FROM PUBLIC; GRANT CONNECT, TEMPORARY ON DATABASE \"${TENANT_DB}\" TO \"${TENANT_USER}\""

    # 新连接串：继承原串其余参数，仅替换/追加 Username 与 Password
    NEW_CONN="$(printf '%s' "$OLD_CONN" | sed -e "s/Username=[^;]*/Username=${TENANT_USER}/i" -e "s/Password=[^;]*/Password=${NEW_PASSWORD}/i")"
    if ! printf '%s' "$NEW_CONN" | grep -qi "Username=${TENANT_USER}"; then
        NEW_CONN="${NEW_CONN%;};Username=${TENANT_USER}"
    fi
    if ! printf '%s' "$NEW_CONN" | grep -qi "Password="; then
        NEW_CONN="${NEW_CONN%;};Password=${NEW_PASSWORD}"
    fi

    if [ "$DRY_RUN" = "1" ]; then
        echo "[dry-run] [${MAIN_DB}] UPDATE system_tenant_entity SET connection=enc(<新串>) WHERE id=${TENANT_ID}"
        echo "[dry-run] 新连接串形态：$(printf '%s' "$NEW_CONN" | sed -E 's/Password=[^;]*/Password=***/i')"
    else
        ENC_CONN="$(printf '%s' "$NEW_CONN" | dotnet run --project "$ENC_TOOL_PROJECT")"
        if ! printf '%s' "$ENC_CONN" | grep -q '^enc:v1:'; then
            echo "!! 租户 ${TENANT_ID} 加密工具输出异常（非 enc:v1: 前缀），跳过回写，连接串保持原值。" >&2
            FAILED_IDS="${FAILED_IDS} ${TENANT_ID}"
            continue
        fi
        run_sql "UPDATE system_tenant_entity SET connection=<enc:v1: 密文 ${#ENC_CONN} 字符> WHERE id=${TENANT_ID}" "$MAIN_DB" \
            "UPDATE system_tenant_entity SET connection = '${ENC_CONN}' WHERE id = ${TENANT_ID}"
    fi

    if [ "$TENANT_ID" = "$DEFAULT_TENANT_ID" ]; then
        echo ""
        echo "================ 默认租户专用输出 ================"
        echo "默认租户库已切换为独立账号。REVOKE PUBLIC 后主库账号无法再连该库，"
        echo "三 host 必须显式配置（替换原推导连接，明文 env 为部署常态）："
        echo "  GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL"
        if [ "$DRY_RUN" = "1" ]; then
            echo "  GFX_DEFAULT_TENANT_DB_CONNECTION_STRING=$(printf '%s' "$NEW_CONN" | sed -E 's/Password=[^;]*/Password=***/i')"
            echo "（dry-run：真实执行时此处输出含新密码的明文串，供部署侧立即配置。）"
        else
            echo "  GFX_DEFAULT_TENANT_DB_CONNECTION_STRING=${NEW_CONN}"
            echo "（以上明文串包含新密码，仅本次输出，请立即配置到部署侧受控文件。）"
        fi
        echo "=================================================="
    fi
done << ROWS
$TENANT_ROWS
ROWS

echo ""
if [ -n "$FAILED_IDS" ]; then
    echo "== 迁移结束（含失败租户:${FAILED_IDS}，请人工核查后重跑或处理）=="
    exit 1
fi
echo "== 迁移全部完成。后续步骤：三 host 滚动重启（释放旧连接缓存）+ 跑 03-verify-permission-matrix.sql 自检 =="
