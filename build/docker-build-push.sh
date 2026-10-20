#!/bin/bash
# Docker 构建并推送镜像
#
# 用法:
#   ./build/docker-build-push.sh                              # 使用默认配置
#   ./build/docker-build-push.sh --tag v2.4.33                # 指定版本标签
#   ./build/docker-build-push.sh --no-push                    # 只构建不推送
#
#   # 推送到私有仓库（账号密码）
#   ./build/docker-build-push.sh \
#     --registry registry.cn-hangzhou.aliyuncs.com \
#     --namespace myns \
#     --username your_user \
#     --password your_pass \
#     --tag v2.4.33
#
#   # 推送到私有仓库（密码从环境变量读取，更安全）
#   export DOCKER_REGISTRY_PASSWORD=your_pass
#   ./build/docker-build-push.sh \
#     --registry registry.cn-hangzhou.aliyuncs.com \
#     --username your_user \
#     --tag v2.4.33
#
# 默认配置（可通过参数覆盖）:
#   REGISTRY   = ""              (Docker Hub 无前缀，或填写私有仓库地址)
#   NAMESPACE  = "alianblank"
#   IMAGE_NAME = "gameframex-admin-web"
#   TAG        = 当前 git tag 或分支名-日期

set -euo pipefail

# ── 默认配置 ──────────────────────────────────────────────
REGISTRY=""
NAMESPACE="alianblank"
IMAGE_NAME="gameframex-admin-web"
PLATFORMS="linux/amd64" # 默认平台；多平台如 "linux/amd64,linux/arm64"
NO_PUSH=false
USERNAME=""
PASSWORD=""

# ── 颜色输出 ──────────────────────────────────────────────
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

info()  { echo -e "${GREEN}[INFO]${NC} $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*"; exit 1; }

# ── 清理函数 ──────────────────────────────────────────────
LOGGED_IN_REGISTRY=""
cleanup() {
    if [[ -n "$LOGGED_IN_REGISTRY" ]]; then
        info "登出私有仓库: ${LOGGED_IN_REGISTRY}"
        docker logout "$LOGGED_IN_REGISTRY" 2>/dev/null || true
    fi
}
trap cleanup EXIT

# ── 参数解析 ──────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
    case $1 in
        --registry)  REGISTRY="$2";  shift 2 ;;
        --namespace) NAMESPACE="$2"; shift 2 ;;
        --name)      IMAGE_NAME="$2"; shift 2 ;;
        --tag)       TAG="$2";       shift 2 ;;
        --platforms) PLATFORMS="$2"; shift 2 ;;
        --no-push)   NO_PUSH=true;   shift ;;
        --username)  USERNAME="$2";  shift 2 ;;
        --password)  PASSWORD="$2";  shift 2 ;;
        -h|--help)
            echo "用法: $0 [选项]"
            echo ""
            echo "选项:"
            echo "  --registry URL       容器仓库地址 (默认: Docker Hub)"
            echo "  --namespace NAME     命名空间/用户名 (默认: alianblank)"
            echo "  --name NAME          镜像名称 (默认: gameframex-admin-web)"
            echo "  --tag TAG            镜像标签 (默认: 自动生成)"
            echo "  --platforms LIST     目标平台 (默认: linux/amd64)"
            echo "  --no-push            只构建不推送"
            echo "  --username USER      仓库用户名"
            echo "  --password PASS      仓库密码 (建议使用环境变量 DOCKER_REGISTRY_PASSWORD)"
            echo "  -h, --help           显示帮助"
            echo ""
            echo "环境变量:"
            echo "  DOCKER_REGISTRY_PASSWORD  仓库密码 (优先级低于 --password)"
            exit 0 ;;
        *) error "未知参数: $1" ;;
    esac
done

# 密码优先级: --password > 环境变量 DOCKER_REGISTRY_PASSWORD
if [[ -z "$PASSWORD" && -n "${DOCKER_REGISTRY_PASSWORD:-}" ]]; then
    PASSWORD="$DOCKER_REGISTRY_PASSWORD"
fi

# ── 自动生成 Tag ──────────────────────────────────────────
if [[ -z "${TAG:-}" ]]; then
    GIT_TAG=$(git describe --tags --exact-match 2>/dev/null || true)
    if [[ -n "$GIT_TAG" ]]; then
        TAG="$GIT_TAG"
    else
        BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
        DATE=$(date +%Y%m%d)
        SHORT_SHA=$(git rev-parse --short HEAD 2>/dev/null || echo "dev")
        TAG="${BRANCH}-${DATE}-${SHORT_SHA}"
    fi
fi

# ── 拼接完整镜像名 ────────────────────────────────────────
if [[ -n "$REGISTRY" ]]; then
    FULL_IMAGE="${REGISTRY}/${NAMESPACE}/${IMAGE_NAME}"
else
    FULL_IMAGE="${NAMESPACE}/${IMAGE_NAME}"
fi

info "镜像: ${FULL_IMAGE}:${TAG}"
info "额外标签: ${FULL_IMAGE}:latest"

# ── 构建前检查 ────────────────────────────────────────────
if ! command -v docker &>/dev/null; then
    error "未安装 docker"
fi

# 判断是否多平台（包含逗号表示多个平台）
IS_MULTIPLATFORM=false
if [[ "$PLATFORMS" == *,* ]]; then
    IS_MULTIPLATFORM=true
    if ! docker buildx version &>/dev/null; then
        error "多平台构建需要 docker buildx，请先安装或切换到单平台构建"
    fi
fi

# ── 仓库登录 ──────────────────────────────────────────────
NEED_LOGIN=false
if [[ -n "$USERNAME" ]]; then
    NEED_LOGIN=true
fi

if [[ "$NEED_LOGIN" == true ]]; then
    LOGIN_TARGET="${REGISTRY:-docker.io}"
    if [[ -z "$PASSWORD" ]]; then
        error "已指定 --username 但未提供密码。请使用 --password 或设置环境变量 DOCKER_REGISTRY_PASSWORD"
    fi

    info "登录仓库: ${LOGIN_TARGET}"
    echo "$PASSWORD" | docker login "$LOGIN_TARGET" -u "$USERNAME" --password-stdin
    LOGGED_IN_REGISTRY="$LOGIN_TARGET"
fi

# ── 开始构建 ──────────────────────────────────────────────
info "开始构建镜像..."
info "目标平台: $PLATFORMS"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# 单仓整合（2026-10-19）：脚本已上移至仓库根 build/，PROJECT_DIR 现在指向仓库根。
# Vue Dockerfile 与构建源都在 GameFrameX.Admin.Unified.Vue 子目录下，docker build context 同时设为仓库根。
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
DOCKERFILE="${PROJECT_DIR}/GameFrameX.Admin.Unified.Vue/Dockerfile"

BUILD_ARGS=(
    --platform "$PLATFORMS"
    -t "${FULL_IMAGE}:${TAG}"
    -t "${FULL_IMAGE}:latest"
    -f "${DOCKERFILE}"
    "${PROJECT_DIR}"
)

if [[ "$IS_MULTIPLATFORM" == true ]]; then
    # 多平台必须通过 buildx --push（无法加载到本地）
    if [[ "$NO_PUSH" == true ]]; then
        error "多平台构建不支持 --no-push（buildx 多平台产物只能直接推送到仓库）"
    fi
    info "使用 buildx 多平台构建并推送..."
    docker buildx build "${BUILD_ARGS[@]}" --push
    info "多平台构建并推送完成"
else
    # 单平台：加载到本地 docker，后续按需推送
    docker buildx build "${BUILD_ARGS[@]}" --load
    info "构建完成: ${FULL_IMAGE}:${TAG}"

    # ── 推送 ──────────────────────────────────────────────
    if [[ "$NO_PUSH" == true ]]; then
        warn "跳过推送 (--no-push)"
        warn "手动推送命令:"
        echo "  docker push ${FULL_IMAGE}:${TAG}"
        echo "  docker push ${FULL_IMAGE}:latest"
    else
        info "推送镜像..."
        docker push "${FULL_IMAGE}:${TAG}"
        docker push "${FULL_IMAGE}:latest"
        info "推送完成"
    fi
fi

# ── 输出摘要 ──────────────────────────────────────────────
echo ""
info "========== 构建完成 =========="
info "镜像:       ${FULL_IMAGE}:${TAG}"
info "Latest:     ${FULL_IMAGE}:latest"
echo ""
info "运行示例:"
echo "  docker run -d -p 20800:20800 -e VITE_API_URL=https://api.example.com ${FULL_IMAGE}:${TAG}"
