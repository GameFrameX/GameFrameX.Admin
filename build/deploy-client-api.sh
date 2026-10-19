#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# Client-API 部署脚本
#
# 通过 HTTP POST 触发远程部署 API
# 所有配置通过环境变量传入 (由 GitHub Actions Secrets 提供)
# =============================================================================

if [ -z "${DEPLOY_API_URL:-}" ]; then
  echo "[SKIP] DEPLOY_API_URL 未配置，跳过部署"
  exit 0
fi

TAG="${IMAGE_TAG##*:}"
IMAGE_NAME="${IMAGE_TAG%:*}"

echo "[DEPLOY] 环境变量:"
echo "  IMAGE_TAG=${IMAGE_TAG}"
echo "  IMAGE_NAME=${IMAGE_NAME}"
echo "  TAG=${TAG}"
echo "  DEPLOY_API_URL=${DEPLOY_API_URL}"
echo "  DEPLOY_AUTH_USERNAME=${DEPLOY_AUTH_USERNAME:-}"
echo "  DEPLOY_AUTH_PASSWORD=****"
echo "  DEPLOY_AUTH_REGISTRY=${DEPLOY_AUTH_REGISTRY:-}"
echo "  DEPLOY_AUTH_EMAIL=${DEPLOY_AUTH_EMAIL:-}"
echo "  DEPLOY_NETWORK_NAME=${DEPLOY_NETWORK_NAME:-}"
echo "  DEPLOY_NETWORK_GATEWAY=${DEPLOY_NETWORK_GATEWAY:-}"
echo "  DEPLOY_CLIENT_IP_ADDRESS=${DEPLOY_CLIENT_IP_ADDRESS:-}"
echo "  DEPLOY_CLIENT_CONTAINER_NAME=${DEPLOY_CLIENT_CONTAINER_NAME:-}"
echo "  DEPLOY_CLIENT_PORTS=${DEPLOY_CLIENT_PORTS:-}"
echo "  DEPLOY_CLIENT_VOLUMES=${DEPLOY_CLIENT_VOLUMES:-}"
echo "  DEPLOY_CLIENT_ENVIRONMENTS=$(echo "${DEPLOY_CLIENT_ENVIRONMENTS:-}" | jq -c 'with_entries(.value = "****")' 2>/dev/null || true)"

validate_json() {
  local name="$1" value="$2"
  if ! echo "$value" | jq . > /dev/null 2>&1; then
    echo "[ERROR] ${name} 不是合法 JSON: ${value}" >&2
    exit 1
  fi
}

[ -z "${DEPLOY_CLIENT_PORTS:-}" ] && DEPLOY_CLIENT_PORTS="{}"
[ -z "${DEPLOY_CLIENT_VOLUMES:-}" ] && DEPLOY_CLIENT_VOLUMES="{}"
[ -z "${DEPLOY_CLIENT_ENVIRONMENTS:-}" ] && DEPLOY_CLIENT_ENVIRONMENTS="{}"

validate_json "DEPLOY_CLIENT_PORTS" "${DEPLOY_CLIENT_PORTS}"
validate_json "DEPLOY_CLIENT_VOLUMES" "${DEPLOY_CLIENT_VOLUMES}"
validate_json "DEPLOY_CLIENT_ENVIRONMENTS" "${DEPLOY_CLIENT_ENVIRONMENTS}"

DEPLOY_CLIENT_PORTS=$(echo "${DEPLOY_CLIENT_PORTS}" | jq -c .)
DEPLOY_CLIENT_VOLUMES=$(echo "${DEPLOY_CLIENT_VOLUMES}" | jq -c .)
DEPLOY_CLIENT_ENVIRONMENTS=$(echo "${DEPLOY_CLIENT_ENVIRONMENTS}" | jq -c .)

jq -n \
  --arg image_name "${IMAGE_NAME}" \
  --arg tag "${TAG}" \
  --arg username "${DEPLOY_AUTH_USERNAME}" \
  --arg password "${DEPLOY_AUTH_PASSWORD}" \
  --arg registry "${DEPLOY_AUTH_REGISTRY}" \
  --arg email "${DEPLOY_AUTH_EMAIL}" \
  --arg network_name "${DEPLOY_NETWORK_NAME}" \
  --arg subnet "${DEPLOY_NETWORK_GATEWAY%.*}.0" \
  --arg gateway "${DEPLOY_NETWORK_GATEWAY}" \
  --arg ip "${DEPLOY_CLIENT_IP_ADDRESS}" \
  --arg container "${DEPLOY_CLIENT_CONTAINER_NAME}" \
  --argjson ports "${DEPLOY_CLIENT_PORTS}" \
  --argjson volumes "${DEPLOY_CLIENT_VOLUMES}" \
  --argjson environments "${DEPLOY_CLIENT_ENVIRONMENTS}" \
'{
  ImageName: $image_name,
  Tag: $tag,
  IsRecreate: true,
  Auth: {
    Username: $username,
    Password: $password,
    Registry: $registry,
    Email: $email
  },
  NetworkConfig: {
    Name: $network_name,
    Subnet: $subnet,
    Gateway: $gateway,
    IpAddress: $ip,
    IsRecreate: false
  },
  ContainerName: $container,
  ports: $ports,
  volumes: $volumes,
  environments: $environments
}' > /tmp/deploy-body.json

echo "[DEPLOY] 部署 client-api"
echo "[DEPLOY] 请求地址: ${DEPLOY_API_URL}"
echo "[DEPLOY] 请求参数:"
jq '.Auth.Password = "****"' /tmp/deploy-body.json
echo ""

HTTP_CODE=$(curl -s --max-time 180 -o /tmp/deploy-resp.txt -w "%{http_code}" -X POST "${DEPLOY_API_URL}" \
  -H "Content-Type: application/json; charset=UTF-8" \
  -d @/tmp/deploy-body.json)

echo "[DEPLOY] HTTP ${HTTP_CODE}"
echo "[DEPLOY] 响应内容:"
cat /tmp/deploy-resp.txt
echo ""

if [ "${HTTP_CODE}" -ge 200 ] && [ "${HTTP_CODE}" -lt 300 ]; then
  echo "[DEPLOY] client-api 部署成功"
else
  echo "[DEPLOY] client-api 部署失败 (HTTP ${HTTP_CODE})" >&2
  exit 1
fi
