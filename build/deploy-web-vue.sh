#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# Web Vue 部署脚本
#
# 通过 HTTP POST 触发远程部署 API
# 所有配置通过环境变量传入 (由 GitHub Actions Secrets 提供)
# =============================================================================

if [ -z "${DEPLOY_API_URL:-}" ]; then
  echo "[SKIP] DEPLOY_API_URL 未配置，跳过部署"
  exit 0
fi

# =============================================================================
# 打印使用到的环境变量
# =============================================================================
echo "[DEPLOY] 环境变量:"
echo "  DEPLOY_API_URL                  = ${DEPLOY_API_URL}"
echo "  IMAGE_TAG                       = ${IMAGE_TAG}"
echo "  DEPLOY_AUTH_USERNAME            = ${DEPLOY_AUTH_USERNAME}"
echo "  DEPLOY_AUTH_PASSWORD            = ****"
echo "  DEPLOY_AUTH_REGISTRY            = ${DEPLOY_AUTH_REGISTRY}"
echo "  DEPLOY_AUTH_EMAIL               = ${DEPLOY_AUTH_EMAIL}"
echo "  DEPLOY_NETWORK_NAME             = ${DEPLOY_NETWORK_NAME}"
echo "  DEPLOY_NETWORK_GATEWAY          = ${DEPLOY_NETWORK_GATEWAY}"
echo "  DEPLOY_WEB_VUE_IP_ADDRESS       = ${DEPLOY_WEB_VUE_IP_ADDRESS}"
echo "  DEPLOY_WEB_VUE_CONTAINER_NAME   = ${DEPLOY_WEB_VUE_CONTAINER_NAME}"
echo "  DEPLOY_WEB_VUE_PORTS            = $(echo "${DEPLOY_WEB_VUE_PORTS}" | jq -c .)"
echo "  DEPLOY_WEB_VUE_VOLUMES          = $(echo "${DEPLOY_WEB_VUE_VOLUMES}" | jq -c .)"
echo "  DEPLOY_WEB_VUE_ENVIRONMENTS     = $(echo "${DEPLOY_WEB_VUE_ENVIRONMENTS}" | jq -c .)"
echo ""

# 验证 JSON 环境变量格式
for var_name in DEPLOY_WEB_VUE_PORTS DEPLOY_WEB_VUE_VOLUMES DEPLOY_WEB_VUE_ENVIRONMENTS; do
  if ! echo "${!var_name}" | jq . > /dev/null 2>&1; then
    echo "[ERROR] ${var_name} 不是有效的 JSON" >&2
    echo "  值: $(echo "${!var_name}" | jq -c . 2>&1 || echo "${!var_name}")" >&2
    exit 1
  fi
done

TAG="${IMAGE_TAG##*:}"
IMAGE_NAME="${IMAGE_TAG%:*}"
SUBNET="${DEPLOY_NETWORK_GATEWAY%.*}.0"

# 使用 jq 压缩多行 JSON 环境变量并构建请求体
jq -n \
  --arg imageName "${IMAGE_NAME}" \
  --arg tag "${TAG}" \
  --arg subnet "${SUBNET}" \
  --arg username "${DEPLOY_AUTH_USERNAME}" \
  --arg password "${DEPLOY_AUTH_PASSWORD}" \
  --arg registry "${DEPLOY_AUTH_REGISTRY}" \
  --arg email "${DEPLOY_AUTH_EMAIL}" \
  --arg networkName "${DEPLOY_NETWORK_NAME}" \
  --arg gateway "${DEPLOY_NETWORK_GATEWAY}" \
  --arg ipAddress "${DEPLOY_WEB_VUE_IP_ADDRESS}" \
  --arg containerName "${DEPLOY_WEB_VUE_CONTAINER_NAME}" \
  --argjson ports "${DEPLOY_WEB_VUE_PORTS}" \
  --argjson volumes "${DEPLOY_WEB_VUE_VOLUMES}" \
  --argjson environments "${DEPLOY_WEB_VUE_ENVIRONMENTS}" \
  '{
    "ImageName": $imageName,
    "Tag": $tag,
    "IsRecreate": true,
    "Auth": {
      "Username": $username,
      "Password": $password,
      "Registry": $registry,
      "Email": $email
    },
    "NetworkConfig": {
      "Name": $networkName,
      "Subnet": $subnet,
      "Gateway": $gateway,
      "IpAddress": $ipAddress,
      "IsRecreate": false
    },
    "ContainerName": $containerName,
    "ports": $ports,
    "volumes": $volumes,
    "environments": $environments
  }' > /tmp/deploy-body.json

echo "[DEPLOY] 部署 web-vue"
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
  echo "[DEPLOY] web-vue 部署成功"
else
  echo "[DEPLOY] web-vue 部署失败 (HTTP ${HTTP_CODE})" >&2
  exit 1
fi
