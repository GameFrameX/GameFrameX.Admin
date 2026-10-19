<template>
  <div class="app-security-container">
    <Card>
      <template #content>
        <div class="workspace-header">
          <div class="workspace-title">
            <h2>安全总览</h2>
            <div class="workspace-meta">
              <Tag v-if="current" :severity="current.isEnabled === false ? 'danger' : 'success'" :value="current.isEnabled === false ? '已停用' : '启用中'"/>
              <span v-if="current">应用 ID: {{ current.id }}</span>
            </div>
          </div>
          <div class="workspace-actions">
            <Select
              v-model="selectedAppId"
              :options="visibleAppOptions"
              optionLabel="name"
              optionValue="id"
              filter
              placeholder="选择应用"
              class="app-select"
              @change="onAppChange"
            />
          </div>
        </div>
      </template>
    </Card>

    <h3 class="section-title">凭证与密钥</h3>
    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item">
            <span>应用凭证（AppSecret）</span>
            <strong>
              <Tag v-if="appDetail?.appSecret" severity="success" value="已配置"/>
              <Tag v-else severity="danger" value="未配置"/>
            </strong>
            <small class="summary-sub">
              请求签名要求：<Tag :severity="appDetail?.signatureRequired ? 'warn' : 'info'" :value="appDetail?.signatureRequired ? '强制' : '关闭'"/>
            </small>
            <small v-if="appDetail?.lastRotatedTime" class="summary-sub">最近重置：{{ formatUnix(appDetail.lastRotatedTime) }}</small>
            <Button label="前往应用管理" link size="small" @click="go('/tenant/appCenter/gameAppEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>传输签名密钥环（HMAC）</span>
            <strong :class="ringClass(sigRing)">{{ ringText(sigRing) }}</strong>
            <small v-if="sigRing" class="summary-sub font-mono">最新 kid：{{ sigRing.latestKid || '-' }}</small>
            <small v-if="sigRing?.nearestExpiry" class="summary-sub">最近到期：{{ formatUnix(sigRing.nearestExpiry) }}</small>
            <Button label="管理传输签名密钥" link size="small" @click="go('/app/appCenter/tenantAppSignatureKeyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>会话密钥环（AES 信封）</span>
            <strong :class="ringClass(sessRing)">{{ ringText(sessRing) }}</strong>
            <small v-if="sessRing" class="summary-sub font-mono">最新 kid：{{ sessRing.latestKid || '-' }}</small>
            <small v-if="sessRing?.nearestExpiry" class="summary-sub">最近到期：{{ formatUnix(sessRing.nearestExpiry) }}</small>
            <Button label="管理会话密钥" link size="small" @click="go('/app/appCenter/tenantSessionKeyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>登录票据密钥环（RSA）</span>
            <strong :class="ringClass(ticketRing)">{{ ringText(ticketRing) }}</strong>
            <small v-if="ticketRing" class="summary-sub">算法：{{ ticketRing.latestKid ? (ticketRing.algorithm || 'RS256') : '-' }}</small>
            <small v-if="ticketRing?.nearestExpiry" class="summary-sub">最近到期：{{ formatUnix(ticketRing.nearestExpiry) }}</small>
            <Button label="管理登录票据密钥" link size="small" @click="go('/app/appCenter/tenantGameLoginTicketKeyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>开放访问凭证（机器对机器）</span>
            <strong :class="openAccess.active > 0 ? 'ok' : 'warn'">启用 {{ openAccess.active }} / 共 {{ openAccess.total }}</strong>
            <small class="summary-sub">打包上报 / CI 等 Hub.Api 机器调用凭证</small>
            <Button label="管理开放访问凭证" link size="small" @click="go('/app/appCenter/gameAppOpenAccess')"/>
          </div>
        </template>
      </Card>
    </div>

    <h3 class="section-title">生效策略</h3>
    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item">
            <span>访问令牌策略（租户级）</span>
            <strong v-if="tokenPolicy">有效期 {{ tokenPolicy.lifetimeMinutes ?? 0 }} 分钟</strong>
            <strong v-else class="warn">未配置（登录将 fail-closed）</strong>
            <Button label="前往配置" link size="small" @click="go('/app/appCenter/tenantClientApiPolicyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>准入与票据策略</span>
            <strong v-if="ticketPolicy">{{ ticketPolicy.ticketExpireTime ? `${t('message.client.clientApiPolicyTicketExpireTime')} ${formatUnix(ticketPolicy.ticketExpireTime)}` : t('message.client.clientApiPolicyTicketNeverExpires') }}</strong>
            <strong v-else class="warn">未配置（走平台默认）</strong>
            <small v-if="ticketPolicy" class="summary-sub">{{ scopeLabel(ticketPolicySource) }} · 自动建钥：{{ ticketPolicy.enableKeyAutoCreate ? '开' : '关' }}</small>
            <Button label="前往配置" link size="small" @click="go('/app/appCenter/tenantClientApiPolicyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>订单策略（沙箱充值）</span>
            <strong v-if="orderPolicy">
              <Tag :severity="orderPolicy.enableSandBoxRecharge ? 'warn' : 'info'" :value="orderPolicy.enableSandBoxRecharge ? '沙箱直接发货' : '正常回调发货'"/>
            </strong>
            <strong v-else class="warn">未配置（默认正常发货）</strong>
            <small v-if="orderPolicy" class="summary-sub">{{ scopeLabel(orderPolicySource) }}</small>
            <Button label="前往配置" link size="small" @click="go('/app/appCenter/tenantClientApiPolicyEntity')"/>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>区服输出策略</span>
            <strong v-if="areaPolicy">
              <Tag :severity="areaPolicy.enableReturnHttpManager ? 'warn' : 'info'" :value="areaPolicy.enableReturnHttpManager ? '返回 HttpManager' : '直连区服'"/>
            </strong>
            <strong v-else class="warn">未配置（默认直连）</strong>
            <small v-if="areaPolicy" class="summary-sub">{{ scopeLabel(areaPolicySource) }}</small>
            <Button label="前往配置" link size="small" @click="go('/app/appCenter/tenantClientApiPolicyEntity')"/>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<script lang="ts" setup name="appSecurityWorkspace">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { t } from '../../../../i18n/index.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { normalizeAppId, visibleApps, type AppWorkspaceId } from '../../../../utils/appWorkspace.ts'
import { tenantGameAppEntityDetailGet } from '../../../../api/tenantGameAppEntity'
import { tenantAppSignatureKeyEntityPagePost } from '../../../../api/tenantAppSignatureKeyEntity'
import { tenantSessionKeyEntityPagePost } from '../../../../api/tenantSessionKeyEntity'
import { tenantGameLoginTicketKeyEntityPagePost } from '../../../../api/tenantGameLoginTicketKeyEntity'
import { tenantGameAppOpenAccessPagePost } from '../../../../api/tenantGameAppOpenAccess'
import { tenantClientApiPolicyEntityPagePost } from '../../../../api/tenantClientApiPolicyEntity'

interface KeyRingSummary {
  total: number
  active: number
  latestKid: string | null
  algorithm: string | null
  nearestExpiry: number | null
}

interface ScopedPolicy<T> {
  row: T | null
  source: 'app' | 'default' | null
}

const route = useRoute()
const router = useRouter()
const { appOptions, loadApps, currentAppId, setCurrentApp, currentApp } = useCurrentApp()
const selectedAppId = ref<AppWorkspaceId | undefined>(undefined)

const visibleAppOptions = computed(() => visibleApps(appOptions.value))
const current = computed(() => currentApp())

const appDetail = ref<any>(null)
const sigRing = ref<KeyRingSummary | null>(null)
const sessRing = ref<KeyRingSummary | null>(null)
const ticketRing = ref<KeyRingSummary | null>(null)
const openAccess = ref<{ total: number, active: number }>({ total: 0, active: 0 })
const tokenPolicy = ref<any>(null)
const ticketPolicy = ref<any>(null)
const ticketPolicySource = ref<'app' | 'default' | null>(null)
const orderPolicy = ref<any>(null)
const orderPolicySource = ref<'app' | 'default' | null>(null)
const areaPolicy = ref<any>(null)
const areaPolicySource = ref<'app' | 'default' | null>(null)
// 合并策略行（C378 四表合一）：租户默认行（AppId=0）+ 当前应用级覆盖行
const policyDefaultRow = ref<any>(null)
const policyAppRow = ref<any>(null)

const nowSeconds = () => Math.floor(Date.now() / 1000)

const isActiveKey = (row: any, now: number) =>
    row.isEnabled !== false &&
    (!row.notBeforeTime || row.notBeforeTime <= now) &&
    (!row.expireTime || row.expireTime > now)

const ringText = (ring: KeyRingSummary | null) => {
  if (!ring) {
    return '加载中…'
  }
  return `活跃 ${ring.active} / 共 ${ring.total}`
}

const ringClass = (ring: KeyRingSummary | null) => {
  if (!ring) {
    return ''
  }
  return ring.active > 0 ? 'ok' : 'warn'
}

const scopeLabel = (source: 'app' | 'default' | null) => {
  if (source === 'app') {
    return '来源：应用级覆盖'
  }
  if (source === 'default') {
    return '来源：租户默认（AppId=0）'
  }
  return ''
}

const summarizeRing = (rows: any[]): KeyRingSummary => {
  const now = nowSeconds()
  const activeRows = rows.filter((row) => isActiveKey(row, now))
  const expiries = activeRows.map((row) => row.expireTime).filter((value): value is number => !!value)
  return {
    total: rows.length,
    active: activeRows.length,
    latestKid: rows[0]?.kid ?? null,
    algorithm: rows[0]?.algorithm ?? null,
    nearestExpiry: expiries.length > 0 ? Math.min(...expiries) : null,
  }
}

// 合并策略取数：应用级行优先，无则回落租户默认行（AppId=0），与
// ClientApiTenantOptionsProvider 的 AppId>0 > AppId=0 运行时优先级同口径。
const scopedPolicy = (): ScopedPolicy<any> => {
  if (policyAppRow.value && policyAppRow.value.isEnabled !== false) {
    return {row: policyAppRow.value, source: 'app'}
  }
  if (policyDefaultRow.value && policyDefaultRow.value.isEnabled !== false) {
    return {row: policyDefaultRow.value, source: 'default'}
  }
  return {row: null, source: null}
}

const fetchPolicyRows = async (appId: string) => {
  return (((await tenantClientApiPolicyEntityPagePost({page: 1, pageSize: 100, appId}))?.data?.data?.items ?? []) as any[])
}

const loadSummaries = async (appId: AppWorkspaceId) => {
  const id = String(appId)
  appDetail.value = null
  sigRing.value = null
  sessRing.value = null
  ticketRing.value = null
  openAccess.value = { total: 0, active: 0 }
  tokenPolicy.value = null
  ticketPolicy.value = null
  ticketPolicySource.value = null
  orderPolicy.value = null
  orderPolicySource.value = null
  areaPolicy.value = null
  areaPolicySource.value = null
  policyDefaultRow.value = null
  policyAppRow.value = null

  const pageItems = async (pageApi: (data?: any) => Promise<any>, query: Record<string, unknown> = {}) => {
    return (((await pageApi({ page: 1, pageSize: 100, ...query }))?.data?.data?.items ?? []) as any[])
  }

  const [detail, sigRows, sessRows, ticketRows, accessRows, policyDefaultRows, policyAppRows] = await Promise.all([
    tenantGameAppEntityDetailGet({ id }),
    pageItems(tenantAppSignatureKeyEntityPagePost, { appId: id }),
    pageItems(tenantSessionKeyEntityPagePost, { appId: id }),
    pageItems(tenantGameLoginTicketKeyEntityPagePost, { appId: id }),
    pageItems(tenantGameAppOpenAccessPagePost, { appId: id }),
    fetchPolicyRows('0'),
    fetchPolicyRows(id),
  ])

  appDetail.value = detail?.data?.data ?? null
  sigRing.value = summarizeRing(sigRows)
  sessRing.value = summarizeRing(sessRows)
  ticketRing.value = summarizeRing(ticketRows)
  const now = nowSeconds()
  openAccess.value = {
    total: accessRows.length,
    active: accessRows.filter((row) => row.isEnabled !== false && !row.revokedTime && (!row.expiresTime || row.expiresTime > now)).length,
  }
  // 令牌节仅租户默认行（AppId=0）被运行时消费；其余节应用级覆盖优先回落默认行
  tokenPolicy.value = policyDefaultRows.find((row) => row.isEnabled !== false) ?? null
  policyDefaultRow.value = policyDefaultRows[0] ?? null
  policyAppRow.value = policyAppRows[0] ?? null
  const scoped = scopedPolicy()
  ticketPolicy.value = scoped.row
  ticketPolicySource.value = scoped.source
  orderPolicy.value = scoped.row
  orderPolicySource.value = scoped.source
  areaPolicy.value = scoped.row
  areaPolicySource.value = scoped.source
}

const ensureSelectedApp = async () => {
  await loadApps()
  const queryId = normalizeAppId(route.query.appId)
  const candidate = queryId ?? currentAppId.value
  const valid = visibleAppOptions.value.find((app) => String(app.id) === String(candidate))
  if (!valid) {
    selectedAppId.value = undefined
    return
  }
  const appId = normalizeAppId(valid.id)
  setCurrentApp(appId)
  selectedAppId.value = valid.id as AppWorkspaceId
  await loadSummaries(appId)
}

const onAppChange = async () => {
  const appId = normalizeAppId(selectedAppId.value)
  if (appId === undefined) {
    return
  }
  setCurrentApp(appId)
  await router.replace({ path: '/app/appCenter/appSecurityWorkspace', query: { appId } })
  await loadSummaries(appId)
}

const go = (path: string) => {
  router.push({ path, query: selectedAppId.value !== undefined ? { appId: String(selectedAppId.value) } : {} })
}

watch(() => route.query.appId, () => {
  ensureSelectedApp()
})

onMounted(() => {
  ensureSelectedApp()
})
</script>

<style scoped lang="scss">
.app-security-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.workspace-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
}

.workspace-title h2 {
  margin: 0 0 8px;
  font-size: 20px;
}

.workspace-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--p-text-muted-color);
}

.app-select {
  min-width: 240px;
}

.section-title {
  margin: 12px 0 0;
  font-size: 15px;
}

.summary-grid {
  display: grid;
  gap: 8px;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
}

.summary-item {
  min-height: 60px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;

  span {
    color: var(--p-text-muted-color);
  }

  strong {
    font-size: 16px;

    &.ok {
      color: var(--p-green-600);
    }

    &.warn {
      color: var(--p-orange-500);
    }
  }
}

.summary-sub {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
