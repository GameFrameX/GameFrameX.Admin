#!/usr/bin/env bash
# 04-demote-main-account.sh
# GameFrameX Admin 租户库完全隔离 · P3 主库账号降权与 superuser 网络封锁（GFX-728 / C319）
#
# 前置：P1（C317 provisioner 独立账号）已合 main；P2（C318 02-migrate-existing-tenants.sh）
#       已对存量注册租户执行完毕（租户角色/owner/enc:v1: 就位）——否则降权会中断建租户与存量租户连接。
#
# 幂等五步（任意步骤中断后重跑安全；已达标步骤自动跳过）：
#   A. postgres 兜底 superuser：本地容器形态 POSTGRES_USER=gameframex 导致不存在 postgres role，
#      降权前若缺失则以随机密码补建（对齐远程形态；否则本地降权后无任何 superuser，且
#      验收「postgres TCP reject / local socket 可登录」无法实测）。随机密码不打印不落盘，
#      local socket（trust/peer）运维登录用不到它。
#   B. ALTER DATABASE <MAIN_DB> OWNER TO <MAIN_ROLE>：降权前确认主库 owner 归应用账号
#      （保 CodeFirst/SeedData DDL 能力；owner 不是自己时改 owner 需要 superuser，故必须在 C 之前）。
#   C. ALTER ROLE <MAIN_ROLE> NOSUPERUSER NOCREATEROLE NOCREATEDB：主账号降权。
#      PG 硬限制：bootstrap superuser（初始化角色，oid=10，通常来自容器 POSTGRES_USER）
#      不允许去掉 SUPERUSER（报 "The bootstrap user must have the SUPERUSER attribute"）。
#      此时仅执行 NOCREATEROLE NOCREATEDB（可达部分），NOSUPERUSER 输出 WARN 并在自检中
#      记 degraded——完整降权需部署形态改造（bootstrap=运维账号、应用账号另建），见 runbook 注。
#   D. pg_hba.conf 顶部插入 reject 规则（host all postgres 0.0.0.0/0 reject + ::/0 reject，
#      置于全部现有规则之前，仅保留 local socket 运维通道）：先备份 → 写入 → pg_reload_conf()
#      → 用 pg_hba_file_rules 校验无 error → 异常回滚备份并再 reload。
#   E. 自检矩阵：三角色 rolsuper/rolcreaterole/rolcreatedb 终态、主库 owner、
#      pg_hba reject 行已解析且无 error、主账号可复连。
#
# 用法：
#   ./04-demote-main-account.sh --dry-run       # 预演：打印将执行的语句/写入行，不落任何变更
#   ./04-demote-main-account.sh --verify-only   # 只跑 E 自检（验证终态）
#   ./04-demote-main-account.sh                 # 真执行 A→E
#
# 环境变量（可选，见默认值）：
#   PSQL_CMD   —— psql 调用包装，默认 "docker exec -i gameframex-postgres psql -U gameframex"
#                （本地容器形态，local trust 免密）；远程/裸 psql 时设为 "psql" 并提供 PGHOST/PGUSER/PGPASSWORD。
#   OPS_PSQL_CMD —— 运维 superuser（postgres）pslq 包装，用于 pg_reload_conf / pg_hba_file_rules
#                （C 步降权后主账号无权 reload；默认同容器 -U postgres 走 local trust）。远程裸 psql
#                时与 PSQL_CMD 一样设为 "psql"（PGUSER=postgres 凭据生效）。
#   HBAC       —— pg_hba.conf 所在主机命令前缀，默认 "docker exec -i gameframex-postgres"；
#                通过 cat 读、stdin cat 写回该前缀指向的目标机。远程服务器 ssh 形态可用
#                HBAC="ssh user@host docker exec <远程pg容器名>" 等可达组合；纯远程安装（无容器）
#                可用 HBAC="ssh user@host sudo -u postgres sh -c"（注意引号转义，建议容器形态）。
#   MAIN_ROLE / MAIN_DB / OPS_ROLE / HBA_PATH   —— 见下方默认值。
#
# 远程 runbook（***REMOVED-HOST-IP*** / PG 18.6，凭证就绪后由运维执行）：
#   1) export PGHOST=***REMOVED-HOST-IP*** PGUSER=postgres PGPASSWORD=<superuser 密码>
#   2) 先确认主账号是否 bootstrap superuser（决定 NOSUPERUSER 可达性）：
#      psql -c "SELECT rolname, oid FROM pg_roles WHERE oid = 10;"  # oid=10 即 bootstrap
#   3) PSQL_CMD="psql" ./04-demote-main-account.sh --dry-run   # 复核将执行的语句
#   4) PSQL_CMD="psql" ./04-demote-main-account.sh             # A/B/C/E 生效（postgres 已存在则 A 跳过）
#   5) pg_hba（D 步）需目标机文件权限：优先把 HBAC 指向远程可达命令前缀重跑本脚本；
#      不可达时按 dry-run 打印的两行 reject 手工插入 pg_hba.conf 顶部，并按 D 步
#      备份→写入→reload→pg_hba_file_rules 校验→异常回滚 的顺序手工执行。
#   6) 重跑 --verify-only 确认矩阵全绿。
#   注：若远程主账号同为 bootstrap superuser（oid=10），NOSUPERUSER 同样不可达——根治需改造部署
#       形态（bootstrap 用独立运维账号、应用账号以普通角色建号后重导数据），超出本脚本范围。

set -euo pipefail

MAIN_ROLE="${MAIN_ROLE:-gameframex}"
MAIN_DB="${MAIN_DB:-gameframex_admin}"
OPS_ROLE="${OPS_ROLE:-postgres}"
HBA_PATH="${HBA_PATH:-/var/lib/postgresql/data/pg_hba.conf}"
HBA_OWNER="${HBA_OWNER:-postgres:root}"
PSQL_CMD="${PSQL_CMD:-docker exec -i gameframex-postgres psql -U gameframex}"
OPS_PSQL_CMD="${OPS_PSQL_CMD:-docker exec -i gameframex-postgres psql -U postgres}"
HBAC="${HBAC:-docker exec -i gameframex-postgres}"
HBA_BACKUP_PREFIX="pg_hba.conf.bak"
DRY_RUN=0
VERIFY_ONLY=0
BOOTSTRAP_LIMIT=0

for arg in "$@"; do
    if [ "$arg" = "--dry-run" ]; then
        DRY_RUN=1
    elif [ "$arg" = "--verify-only" ]; then
        VERIFY_ONLY=1
    else
        echo "未知参数：${arg}（仅支持 --dry-run / --verify-only）" >&2
        exit 64
    fi
done

read -ra PSQL_ARR <<< "$PSQL_CMD"
read -ra OPS_PSQL_ARR <<< "$OPS_PSQL_CMD"
read -ra HBAC_ARR <<< "$HBAC"
for bin in "${PSQL_ARR[0]}" "${OPS_PSQL_ARR[0]}" "${HBAC_ARR[0]}"; do
    if ! command -v "$bin" > /dev/null 2>&1; then
        echo "错误：缺少依赖命令 ${bin}。" >&2
        exit 65
    fi
done

# 统一 SQL 执行入口：$1=人读描述，$2=目标库名，$3=真实 SQL
# < /dev/null：隔离 stdin，防止 docker exec -i 之类包装吞掉外层 heredoc 输入（02 脚本同款教训）
run_sql() {
    if [ "$DRY_RUN" = "1" ]; then
        echo "[dry-run] [$2] $1"
    else
        echo "[exec] [$2] $1"
        "${PSQL_ARR[@]}" -X -v ON_ERROR_STOP=1 -d "$2" -c "$3" < /dev/null
    fi
}

# 标量查询（主账号通道）：$1=目标库名，$2=SQL；stdout 仅一行结果
psql_scalar() {
    "${PSQL_ARR[@]}" -X -A -t -v ON_ERROR_STOP=1 -d "$1" -c "$2" < /dev/null
}

# 标量查询（运维 superuser 通道，postgres）：$1=目标库名，$2=SQL
ops_psql_scalar() {
    "${OPS_PSQL_ARR[@]}" -X -A -t -v ON_ERROR_STOP=1 -d "$1" -c "$2" < /dev/null
}

FAIL_COUNT=0

# 校验辅助：$1=描述，$2=期望值，$3=实际值
expect_eq() {
    if [ "$2" = "$3" ]; then
        echo "  [PASS] $1 = $3"
    else
        echo "  [FAIL] $1 期望=$2 实际=$3"
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
}

WARN_COUNT=0

# 降级校验辅助：实际值与期望不符时记 WARN 不计 FAIL（用于 bootstrap 限制等环境硬限制场景）
# $1=描述，$2=期望值，$3=实际值，$4=限制说明
expect_warn_eq() {
    if [ "$2" = "$3" ]; then
        echo "  [PASS] $1 = $3"
    else
        echo "  [WARN] $1 期望=$2 实际=$3（$4）"
        WARN_COUNT=$((WARN_COUNT + 1))
    fi
}

echo "== C319 / GFX-728 主库账号降权与 pg_hba 封锁 superuser（mode=$( [ "$DRY_RUN" = "1" ] && echo dry-run || echo $( [ "$VERIFY_ONLY" = "1" ] && echo verify-only || echo execute ) )）=="

if [ "$VERIFY_ONLY" = "0" ]; then
    # ---- Step A：postgres 兜底 superuser（缺失才建；幂等）----
    OPS_EXISTS="$(psql_scalar postgres "SELECT count(*) FROM pg_roles WHERE rolname = '${OPS_ROLE}';")"
    if [ "$OPS_EXISTS" = "1" ]; then
        echo "[A] ${OPS_ROLE} role 已存在，跳过建号。"
    else
        RAND_PW="$(openssl rand -base64 24 | tr -d '\n')"
        echo "[A] ${OPS_ROLE} role 不存在，补建 superuser（随机密码，不回显）。"
        run_sql "CREATE ROLE ${OPS_ROLE} SUPERUSER LOGIN（随机密码）" postgres \
            "CREATE ROLE \"${OPS_ROLE}\" WITH LOGIN SUPERUSER PASSWORD '${RAND_PW}';"
    fi

    # ---- Step B：主库 OWNER 归应用账号（降权前执行；幂等）----
    CUR_OWNER="$(psql_scalar postgres "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname = '${MAIN_DB}';")"
    if [ "$CUR_OWNER" = "$MAIN_ROLE" ]; then
        echo "[B] ${MAIN_DB} owner 已是 ${MAIN_ROLE}，跳过。"
    else
        run_sql "ALTER DATABASE ${MAIN_DB} OWNER TO ${MAIN_ROLE}" postgres \
            "ALTER DATABASE \"${MAIN_DB}\" OWNER TO \"${MAIN_ROLE}\";"
    fi

    # ---- Step C：主账号降权（bootstrap superuser 只能去 CREATEROLE/CREATEDB；幂等）----
    ROLE_ATTR="$(psql_scalar postgres "SELECT rolsuper || '/' || rolcreaterole || '/' || rolcreatedb FROM pg_roles WHERE rolname = '${MAIN_ROLE}';")"
    IS_BOOTSTRAP="$(psql_scalar postgres "SELECT count(*) FROM pg_roles WHERE rolname = '${MAIN_ROLE}' AND oid = 10 AND rolsuper;")"
    if [ "$ROLE_ATTR" = "f/f/f" ]; then
        echo "[C] ${MAIN_ROLE} 已是 NOSUPERUSER NOCREATEROLE NOCREATEDB，跳过。"
    elif [ "$IS_BOOTSTRAP" = "1" ]; then
        BOOTSTRAP_LIMIT=1
        echo "[C] ${MAIN_ROLE} 是 bootstrap superuser（oid=10）：NOSUPERUSER 不可达（PG 硬限制），仅执行 NOCREATEROLE NOCREATEDB。"
        if [ "$DRY_RUN" = "0" ]; then
            run_sql "ALTER ROLE ${MAIN_ROLE} NOCREATEROLE NOCREATEDB" postgres \
                "ALTER ROLE \"${MAIN_ROLE}\" NOCREATEROLE NOCREATEDB;"
        fi
        echo "[C][WARN] ${MAIN_ROLE} 保留 SUPERUSER 属环境硬限制；完整降权需部署形态改造（bootstrap=运维账号）。"
    else
        run_sql "ALTER ROLE ${MAIN_ROLE} NOSUPERUSER NOCREATEROLE NOCREATEDB" postgres \
            "ALTER ROLE \"${MAIN_ROLE}\" NOSUPERUSER NOCREATEROLE NOCREATEDB;"
    fi

    # ---- Step D：pg_hba.conf 顶部注入 reject（已注入则跳过；失败回滚）----
    HBA_HAS_REJECT="$("${HBAC_ARR[@]}" cat "$HBA_PATH" | grep -c "^host all ${OPS_ROLE} 0.0.0.0/0 reject$" || true)"
    HBA_HAS_REJECT_V6="$("${HBAC_ARR[@]}" cat "$HBA_PATH" | grep -c "^host all ${OPS_ROLE} ::/0 reject$" || true)"
    if [ "$HBA_HAS_REJECT" -ge 1 ] && [ "$HBA_HAS_REJECT_V6" -ge 1 ]; then
        echo "[D] pg_hba reject 规则已存在，跳过。"
    elif [ "$DRY_RUN" = "1" ]; then
        echo "[dry-run] [$HBA_PATH] 顶部插入两行："
        echo "    host all ${OPS_ROLE} 0.0.0.0/0 reject"
        echo "    host all ${OPS_ROLE} ::/0 reject"
        echo "[dry-run] [${HBA_PATH}] 备份为 ${HBA_PATH}.bak-<timestamp> 后写入，reload 并校验 pg_hba_file_rules"
    else
        STAMP="$(date +%Y%m%d%H%M%S)"
        echo "[D] 备份 ${HBA_PATH} → ${HBA_PATH}.bak-${STAMP}"
        "${HBAC_ARR[@]}" cp "$HBA_PATH" "${HBA_PATH}.bak-${STAMP}"
        "${HBAC_ARR[@]}" cat "$HBA_PATH" | { printf 'host all %s 0.0.0.0/0 reject\nhost all %s ::/0 reject\n' "$OPS_ROLE" "$OPS_ROLE"; cat; } | "${HBAC_ARR[@]}" sh -c "cat > '$HBA_PATH'"
        # 写回可能改变文件属主/权限（如 docker exec 以 root 落盘），postgres 进程读不了会导致 reload 失败、
        # 容器重启起不来；统一回归 postgres 属主 + 600（与镜像初始形态一致）
        "${HBAC_ARR[@]}" sh -c "chown ${HBA_OWNER} '$HBA_PATH' && chmod 600 '$HBA_PATH'"
        echo "[D] 已写入，执行 pg_reload_conf() ..."
        ops_psql_scalar postgres "SELECT pg_reload_conf();" > /dev/null
        sleep 1
        HBA_ERRORS="$(ops_psql_scalar postgres "SELECT count(*) FROM pg_hba_file_rules WHERE error IS NOT NULL;")"
        if [ "$HBA_ERRORS" = "0" ]; then
            echo "[D] pg_hba_file_rules 校验无 error，注入成功。"
        else
            echo "[D] 检测到 ${HBA_ERRORS} 行 hba 解析错误，回滚备份并重新 reload。" >&2
            "${HBAC_ARR[@]}" cp "${HBA_PATH}.bak-${STAMP}" "$HBA_PATH"
            ops_psql_scalar postgres "SELECT pg_reload_conf();" > /dev/null
            exit 70
        fi
    fi
fi

# bootstrap 探测（verify-only 未经过 C 步时补测，只读）
if [ "$BOOTSTRAP_LIMIT" = "0" ]; then
    PROBE_BOOT="$(psql_scalar postgres "SELECT count(*) FROM pg_roles WHERE rolname = '${MAIN_ROLE}' AND oid = 10 AND rolsuper;")"
    if [ "$PROBE_BOOT" = "1" ]; then
        BOOTSTRAP_LIMIT=1
    fi
fi

# ---- Step E：自检矩阵（dry-run 只预演写操作，不做终态断言）----
if [ "$DRY_RUN" = "1" ]; then
    echo "== E 自检矩阵：dry-run 模式跳过（真执行后用 --verify-only 复核）=="
    exit 0
fi
echo "== E 自检矩阵 =="

ATTR_MAIN="$(psql_scalar postgres "SELECT rolsuper || '/' || rolcreaterole || '/' || rolcreatedb FROM pg_roles WHERE rolname = '${MAIN_ROLE}';")"
if [ "$BOOTSTRAP_LIMIT" = "1" ]; then
    expect_warn_eq "${MAIN_ROLE} rolsuper/rolcreaterole/rolcreatedb" "f/f/f" "$ATTR_MAIN" "bootstrap superuser（oid=10）不允许 NOSUPERUSER，已去除 CREATEROLE/CREATEDB"
else
    expect_eq "${MAIN_ROLE} rolsuper/rolcreaterole/rolcreatedb" "f/f/f" "$ATTR_MAIN"
fi

ATTR_PROV="$(psql_scalar postgres "SELECT rolsuper FROM pg_roles WHERE rolname = 'gfx_provisioner';")"
expect_eq "gfx_provisioner rolsuper" "f" "$ATTR_PROV"

ATTR_OPS="$(psql_scalar postgres "SELECT rolsuper FROM pg_roles WHERE rolname = '${OPS_ROLE}';")"
expect_eq "${OPS_ROLE} rolsuper（仅 local socket 运维）" "t" "$ATTR_OPS"

OWNER_NOW="$(psql_scalar postgres "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname = '${MAIN_DB}';")"
expect_eq "${MAIN_DB} owner" "$MAIN_ROLE" "$OWNER_NOW"

HBA_RULE_OK="$(ops_psql_scalar postgres "SELECT count(*) FROM pg_hba_file_rules WHERE user_name[1] = '${OPS_ROLE}' AND auth_method = 'reject' AND (address IN ('0.0.0.0', '0.0.0.0/0', '::', '::/0'));")"
expect_eq "pg_hba reject 规则解析行数（IPv4+IPv6）" "2" "$HBA_RULE_OK"

CONN_MAIN="$(psql_scalar "$MAIN_DB" "SELECT current_user;")"
expect_eq "${MAIN_ROLE} 复连 ${MAIN_DB}（current_user）" "$MAIN_ROLE" "$CONN_MAIN"

if [ "$FAIL_COUNT" = "0" ] && [ "$WARN_COUNT" = "0" ]; then
    echo "== 自检全绿 =="
    exit 0
elif [ "$FAIL_COUNT" = "0" ]; then
    echo "== 自检完成：0 FAIL / ${WARN_COUNT} WARN（环境硬限制降级项，见上）=="
    exit 0
else
    echo "== 自检存在 ${FAIL_COUNT} 项失败 ==" >&2
    exit 1
fi
