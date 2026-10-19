#!/bin/sh
# 容器启动时根据环境变量动态生成 config.js
# 未传入的变量保留构建时的默认值

CONFIG_FILE=/usr/share/nginx/html/config/config.js

# 读取现有默认值（构建时写入）。nginx 运行镜像不包含 Node，保持这里为 POSIX shell。
get_default() {
  key="$1"
  sed -n "s/.*\"$key\"[[:space:]]*:[[:space:]]*\"\\([^\"]*\\)\".*/\\1/p" "$CONFIG_FILE" | head -n 1
}

DEFAULT_VITE_PORT="$(get_default VITE_PORT)"
DEFAULT_VITE_OPEN="$(get_default VITE_OPEN)"
DEFAULT_VITE_OPEN_CDN="$(get_default VITE_OPEN_CDN)"
DEFAULT_VITE_PUBLIC_PATH="$(get_default VITE_PUBLIC_PATH)"
DEFAULT_VITE_API_URL="$(get_default VITE_API_URL)"

# 环境变量优先，否则用默认值
VITE_PORT="${VITE_PORT:-$DEFAULT_VITE_PORT}"
VITE_OPEN="${VITE_OPEN:-$DEFAULT_VITE_OPEN}"
VITE_OPEN_CDN="${VITE_OPEN_CDN:-$DEFAULT_VITE_OPEN_CDN}"
VITE_PUBLIC_PATH="${VITE_PUBLIC_PATH:-$DEFAULT_VITE_PUBLIC_PATH}"
VITE_API_URL="${VITE_API_URL:-$DEFAULT_VITE_API_URL}"

# 写入 config.js
cat > "$CONFIG_FILE" << EOF
window.__env__ = {
  "VITE_PORT": "$VITE_PORT",
  "VITE_OPEN": "$VITE_OPEN",
  "VITE_OPEN_CDN": "$VITE_OPEN_CDN",
  "VITE_PUBLIC_PATH": "$VITE_PUBLIC_PATH",
  "VITE_API_URL": "$VITE_API_URL"
}
EOF

echo "[entrypoint] VITE_API_URL = $VITE_API_URL"

exec nginx -g 'daemon off;'
