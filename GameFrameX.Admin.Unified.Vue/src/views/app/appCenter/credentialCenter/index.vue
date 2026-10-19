<template>
  <div class="credential-center-container">
    <Card>
      <template #content>
        <div class="workspace-header">
          <div class="workspace-title">
            <h2>{{ t('gameBusiness.credentialCenter.title') }}</h2>
            <div class="workspace-meta">
              <Tag v-if="current" :severity="current.isEnabled === false ? 'danger' : 'success'"
                   :value="current.isEnabled === false ? t('gameBusiness.credentialCenter.appDisabled') : t('gameBusiness.credentialCenter.appEnabled')"/>
              <span v-if="current">App ID: {{ current.id }}</span>
              <span v-if="overview">· {{ overview.appName }}</span>
            </div>
          </div>
          <div class="workspace-actions">
            <Select v-model="selectedAppId" :options="visibleAppOptions" optionLabel="name" optionValue="id"
                    filter :placeholder="t('gameBusiness.appWorkspace.selectApp')" class="app-select" @change="onAppChange"/>
          </div>
        </div>
      </template>
    </Card>

    <!-- 用途矩阵：常驻页面顶部，数据来自后端常量（Response.UsageMatrix），禁前端硬编码。 -->
    <h3 class="section-title">{{ t('gameBusiness.credentialCenter.usageMatrix.title') }}</h3>
    <Card>
      <template #content>
        <div class="usage-matrix-wrapper">
          <DataTable :value="matrixRows" showGridlines stripedRows rowKey="member" size="small" dataKey="member"
                     class="usage-matrix-table">
            <Column field="categoryTitle" :header="t('gameBusiness.credentialCenter.usageMatrix.columns.category')" style="min-width: 8rem"/>
            <Column field="memberTitle" :header="t('gameBusiness.credentialCenter.usageMatrix.columns.member')" style="min-width: 14rem"/>
            <Column field="algorithmForm" :header="t('gameBusiness.credentialCenter.usageMatrix.columns.algorithm')" style="min-width: 14rem"/>
            <Column field="consumers" :header="t('gameBusiness.credentialCenter.usageMatrix.columns.consumers')" style="min-width: 14rem"/>
            <Column field="operations" :header="t('gameBusiness.credentialCenter.usageMatrix.columns.operations')" style="min-width: 14rem"/>
          </DataTable>
        </div>
      </template>
    </Card>

    <!-- ① 应用身份凭证：AppSecret 配置态 / 最近重置 / 签名开关。 -->
    <h3 class="section-title">{{ t('gameBusiness.credentialCenter.sections.identity.title') }}</h3>
    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-identity">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.identity.appSecret') }}</span>
            <strong :class="overview?.identity?.appSecretConfigured ? 'ok' : 'warn'">
              <Tag :severity="overview?.identity?.appSecretConfigured ? 'success' : 'danger'"
                   :value="overview?.identity?.appSecretConfigured ? t('gameBusiness.credentialCenter.sections.identity.configured') : t('gameBusiness.credentialCenter.sections.identity.notConfigured')"/>
            </strong>
            <small class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.identity.signatureRequired') }}
              <Tag :severity="overview?.identity?.signatureRequired ? 'warn' : 'info'"
                   :value="overview?.identity?.signatureRequired ? t('gameBusiness.credentialCenter.required') : t('gameBusiness.credentialCenter.optional')"/>
            </small>
            <small v-if="overview?.identity?.appSecretLastRotatedTime" class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.identity.lastRotated') }} {{ formatUnix(overview.identity.appSecretLastRotatedTime) }}
            </small>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.identity.reset')" icon="fa-solid fa-rotate" size="small" severity="danger"
                      :disabled="loading || !current" v-auth="`${CREDENTIAL_PERM_PREFIX}:resetSecret`"
                      @click="onResetAppSecret"/>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- ② 传输签名凭证：HMAC 密钥环 + 开放访问凭证。 -->
    <h3 class="section-title">{{ t('gameBusiness.credentialCenter.sections.transport.title') }}</h3>
    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-transport-hmac">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.transport.hmac') }}</span>
            <strong :class="hmacActiveCount > 0 ? 'ok' : 'warn'">
              {{ t('gameBusiness.credentialCenter.sections.transport.activeCount') }} {{ hmacActiveCount }}
            </strong>
            <small v-if="overview?.transport?.latestHmacKid" class="summary-sub font-mono">
              {{ t('gameBusiness.credentialCenter.sections.transport.latestKid') }} {{ overview.transport.latestHmacKid }}
            </small>
            <small v-if="overview?.transport?.hmacNearestExpireTime" class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.transport.nearestExpire') }} {{ formatUnix(overview.transport.hmacNearestExpireTime) }}
            </small>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.transport.addHmac')" icon="fa-solid fa-plus" size="small" severity="secondary"
                      :disabled="loading || !current" v-auth="`${PERMS.HMAC}:add`"
                      @click="openHmacAdd"/>
              <Button :label="t('gameBusiness.credentialCenter.sections.transport.rotateHmac')" icon="fa-solid fa-rotate" size="small"
                      :disabled="loading || !current" v-auth="`${PERMS.HMAC}:rotate`"
                      @click="openHmacRotate"/>
              <Button :label="t('gameBusiness.credentialCenter.sections.transport.manageHmac')" link size="small"
                      @click="go(`/app/appCenter/tenantAppSignatureKeyEntity`)"/>
            </div>
          </div>
        </template>
      </Card>

      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-transport-openaccess">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.transport.openAccess') }}</span>
            <strong :class="overview && overview.transport.openAccessActiveCount > 0 ? 'ok' : 'warn'">
              {{ t('gameBusiness.credentialCenter.sections.transport.activeCount') }} {{ overview?.transport?.openAccessActiveCount ?? 0 }}
            </strong>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.transport.manageOpenAccess')" link size="small"
                      @click="go(`/app/appCenter/gameAppOpenAccess`)"/>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- ③ 票据与令牌密钥：票据密钥 / 会话密钥 / 令牌签名密钥。 -->
    <h3 class="section-title">{{ t('gameBusiness.credentialCenter.sections.ticketToken.title') }}</h3>
    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-ticket">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.ticketToken.ticketKey') }}</span>
            <strong :class="ticketActiveCount > 0 ? 'ok' : 'warn'">
              {{ t('gameBusiness.credentialCenter.sections.ticketToken.activeCount') }} {{ ticketActiveCount }}
            </strong>
            <small v-if="overview?.ticketToken?.ticketKeyNearestExpireTime" class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.transport.nearestExpire') }} {{ formatUnix(overview.ticketToken.ticketKeyNearestExpireTime) }}
            </small>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.ticketToken.rotateTicket')" icon="fa-solid fa-rotate" size="small"
                      :disabled="loading || !current" v-auth="`${PERMS.TICKET_KEY}:rotate`"
                      @click="openTicketRotate"/>
              <Button :label="t('gameBusiness.credentialCenter.sections.ticketToken.manageTicket')" link size="small"
                      @click="go(`/app/appCenter/tenantGameLoginTicketKeyEntity`)"/>
            </div>
          </div>
        </template>
      </Card>

      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-session">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.ticketToken.sessionKey') }}</span>
            <strong :class="overview && overview.ticketToken.sessionKeyActiveCount > 0 ? 'ok' : 'warn'">
              {{ t('gameBusiness.credentialCenter.sections.ticketToken.activeCount') }} {{ overview?.ticketToken?.sessionKeyActiveCount ?? 0 }}
            </strong>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.ticketToken.rotateSession')" icon="fa-solid fa-rotate" size="small"
                      :disabled="loading || !current" v-auth="`${PERMS.SESSION_KEY}:rotate`"
                      @click="openSessionRotate"/>
              <Button :label="t('gameBusiness.credentialCenter.sections.ticketToken.manageSession')" link size="small"
                      @click="go(`/app/appCenter/tenantSessionKeyEntity`)"/>
            </div>
          </div>
        </template>
      </Card>

      <Card>
        <template #content>
          <div class="summary-item" data-testid="card-token-policy">
            <span class="summary-label">{{ t('gameBusiness.credentialCenter.sections.ticketToken.tokenPolicy') }}</span>
            <strong :class="overview?.ticketToken?.tokenSigningKeyConfigured ? 'ok' : 'warn'">
              <Tag :severity="overview?.ticketToken?.tokenSigningKeyConfigured ? 'success' : 'danger'"
                   :value="overview?.ticketToken?.tokenSigningKeyConfigured ? t('gameBusiness.credentialCenter.sections.identity.configured') : t('gameBusiness.credentialCenter.sections.identity.notConfigured')"/>
              <span class="ml-2">{{ t('gameBusiness.credentialCenter.sections.ticketToken.tokenLifetime') }} {{ overview?.ticketToken?.tokenLifetimeMinutes ?? 0 }} min</span>
            </strong>
            <small v-if="overview?.ticketToken?.policyScope && overview.ticketToken.policyScope !== 'none'" class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.ticketToken.policyScope') }} {{ policyScopeLabel }}
            </small>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.ticketToken.managePolicy')" link size="small"
                      @click="go(`/app/appCenter/tenantClientApiPolicyEntity`)"/>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- ④ 出站对接凭证：每行支付/邮件/SMS/微信/广告回调/对象存储 路由来自后端 Response.Outbound[].routePath，前端不写死。 -->
    <h3 class="section-title">{{ t('gameBusiness.credentialCenter.sections.outbound.title') }}</h3>
    <div class="summary-grid">
      <Card v-for="row in (overview?.outbound ?? [])" :key="row.kind">
        <template #content>
          <div class="summary-item" :data-testid="`card-outbound-${row.kind}`">
            <span class="summary-label">{{ row.title }}</span>
            <strong :class="row.configuredCount > 0 ? 'ok' : 'warn'">
              <Tag :severity="row.configuredCount > 0 ? 'success' : 'danger'"
                   :value="row.configuredCount > 0 ? t('gameBusiness.credentialCenter.sections.outbound.configured') : t('gameBusiness.credentialCenter.sections.outbound.notConfigured')"/>
            </strong>
            <small v-if="row.scope !== 'none'" class="summary-sub">
              {{ t('gameBusiness.credentialCenter.sections.outbound.scope') }} {{ outboundScopeLabel(row.scope) }}
            </small>
            <div class="summary-actions">
              <Button :label="t('gameBusiness.credentialCenter.sections.outbound.goConfig')" link size="small"
                      :disabled="!row.routePath"
                      @click="go(row.routePath)"/>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- 内嵌既有对话框：HMAC Add/Rotate、SessionKey Add/Rotate、TicketKey Rotate、AppSecret Reset SecretDialog -->
    <HmacAddDialog ref="hmacAddRef" @added="refresh"/>
    <HmacRotateDialog ref="hmacRotateRef" @rotated="onRotateSuccess"/>
    <SessionAddDialog ref="sessionAddRef" @added="refresh"/>
    <SessionRotateDialog ref="sessionRotateRef" @rotated="onRotateSuccess"/>
    <TicketRotateDialog ref="ticketRotateRef" @rotate="onTicketRotate"/>
    <SecretDialog ref="secretDialogRef"/>
  </div>
</template>

<script lang="ts" setup name="credentialCenter">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { t } from '../../../../i18n/index.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { normalizeAppId, visibleApps, type AppWorkspaceId } from '../../../../utils/appWorkspace.ts'
import { showConfirm, showSuccess } from '../../../../utils/message.ts'
import { tenantGameAppEntityResetSecretPost } from '../../../../api/tenantGameAppEntity'
import {
  tenantAppCredentialOverviewGetGet,
  type TenantAppCredentialOverviewResponse,
  type UsageMatrixResponse,
} from '../../../../api/tenantAppCredentialOverview'
import HmacAddDialog from '../tenantAppSignatureKeyEntity/component/AddDialog.vue'
import HmacRotateDialog from '../tenantAppSignatureKeyEntity/component/RotateDialog.vue'
import SessionAddDialog from '../tenantSessionKeyEntity/component/AddDialog.vue'
import SessionRotateDialog from '../tenantSessionKeyEntity/component/RotateDialog.vue'
import TicketRotateDialog from '../tenantGameLoginTicketKeyEntity/component/RotateDialog.vue'
import SecretDialog from '../../../tenant/appCenter/gameAppEntity/component/SecretDialog.vue'

const route = useRoute()
const router = useRouter()
const { appOptions, loadApps, currentAppId, setCurrentApp, currentApp } = useCurrentApp()

// 权限码前缀（与既有实体页保持一致）；聚合只读端点权限由后端菜单挂载时分配。
const CREDENTIAL_PERM_PREFIX = 'tenantGameAppEntity'
const PERMS = {
  HMAC: 'tenantAppSignatureKeyEntity',
  SESSION_KEY: 'tenantSessionKeyEntity',
  TICKET_KEY: 'tenantGameLoginTicketKeyEntity',
} as const

const visibleAppOptions = computed(() => visibleApps(appOptions.value))
const current = computed(() => currentApp())
const selectedAppId = ref<AppWorkspaceId | undefined>(undefined)

const loading = ref(false)
const overview = ref<TenantAppCredentialOverviewResponse | null>(null)
const matrixRows = computed(() => (overview.value?.usageMatrix ?? []) as UsageMatrixResponse[])

const hmacActiveCount = computed(() => overview.value?.transport?.hmacActiveKeyCount ?? 0)
const ticketActiveCount = computed(() => overview.value?.ticketToken?.ticketKeyActiveCount ?? 0)

const policyScopeLabel = computed(() => {
  const scope = overview.value?.ticketToken?.policyScope
  if (scope === 'app') {
    return t('gameBusiness.credentialCenter.scope.app')
  }
  if (scope === 'tenant-default') {
    return t('gameBusiness.credentialCenter.scope.tenantDefault')
  }
  return '-'
})

const outboundScopeLabel = (scope: string): string => {
  if (scope === 'app') {
    return t('gameBusiness.credentialCenter.scope.app')
  }
  if (scope === 'tenant-default') {
    return t('gameBusiness.credentialCenter.scope.tenantDefault')
  }
  if (scope === 'tenant') {
    return t('gameBusiness.credentialCenter.scope.tenant')
  }
  return '-'
}

const hmacAddRef = ref<InstanceType<typeof HmacAddDialog> | null>(null)
const hmacRotateRef = ref<InstanceType<typeof HmacRotateDialog> | null>(null)
const sessionAddRef = ref<InstanceType<typeof SessionAddDialog> | null>(null)
const sessionRotateRef = ref<InstanceType<typeof SessionRotateDialog> | null>(null)
const ticketRotateRef = ref<InstanceType<typeof TicketRotateDialog> | null>(null)
const secretDialogRef = ref<InstanceType<typeof SecretDialog> | null>(null)

const refresh = async () => {
  const id = selectedAppId.value
  if (id === undefined) {
    overview.value = null
    return
  }
  loading.value = true
  try {
    const res = await tenantAppCredentialOverviewGetGet(Number(id))
    overview.value = (res.data?.data ?? null) as TenantAppCredentialOverviewResponse | null
  } catch {
    // 全局拦截器已提示
  } finally {
    loading.value = false
  }
}

const ensureSelectedApp = async () => {
  await loadApps()
  const queryId = normalizeAppId(route.query.appId)
  const candidate = queryId ?? currentAppId.value
  const valid = visibleAppOptions.value.find((app) => String(app.id) === String(candidate))
  if (!valid) {
    selectedAppId.value = undefined
    overview.value = null
    return
  }
  const appId = normalizeAppId(valid.id)
  setCurrentApp(appId)
  selectedAppId.value = valid.id as AppWorkspaceId
  await refresh()
}

const onAppChange = async () => {
  const appId = normalizeAppId(selectedAppId.value)
  if (appId === undefined) {
    return
  }
  setCurrentApp(appId)
  await router.replace({ path: '/app/appCenter/credentialCenter', query: { appId } })
  await refresh()
}

const go = (path: string) => {
  router.push({ path, query: selectedAppId.value !== undefined ? { appId: String(selectedAppId.value) } : {} })
}

const onRotateSuccess = async (reveal?: any) => {
  // 轮换成功：弹窗内自带一次性回显；回到本页仅刷新聚合状态。
  if (reveal && (reveal as any).kid) {
    showSuccess(t('gameBusiness.credentialCenter.rotateSuccess'))
  }
  await refresh()
}

const onTicketRotate = async (payload: { appId: number, expireTime: number | null }) => {
  // TicketKey RotateDialog 不会自动调 API，本页代理调实体 rotate 接口；返回响应一次性回显。
  const { tenantGameLoginTicketKeyEntityRotatePost } = await import('../../../../api/tenantGameLoginTicketKeyEntity')
  await tenantGameLoginTicketKeyEntityRotatePost(payload as any)
  await refresh()
}

const onResetAppSecret = async () => {
  const id = selectedAppId.value
  if (id === undefined) {
    return
  }
  const confirmed = await showConfirm(
    t('gameBusiness.credentialCenter.sections.identity.resetConfirm'),
    t('common.message.tip'),
  )
  if (!confirmed) {
    return
  }
  const res = await tenantGameAppEntityResetSecretPost({ id: String(id) })
  const data = res?.data?.data
  if (data) {
    secretDialogRef.value?.openDialog(data)
  }
  await refresh()
}

const openHmacAdd = () => {
  hmacAddRef.value?.open()
}

const openHmacRotate = () => {
  if (selectedAppId.value === undefined) {
    return
  }
  hmacRotateRef.value?.open({ appId: selectedAppId.value })
}

const openSessionRotate = () => {
  if (selectedAppId.value === undefined) {
    return
  }
  sessionRotateRef.value?.open({ appId: selectedAppId.value })
}

const openTicketRotate = () => {
  if (selectedAppId.value === undefined) {
    return
  }
  ticketRotateRef.value?.open({ appId: selectedAppId.value })
}

watch(() => route.query.appId, () => {
  ensureSelectedApp()
})

onMounted(() => {
  ensureSelectedApp()
})
</script>

<style scoped lang="scss">
.credential-center-container {
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
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.summary-item {
  min-height: 60px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;

  .summary-label {
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

.summary-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
}

.usage-matrix-wrapper {
  overflow-x: auto;

  .usage-matrix-table {
    min-width: 720px;
  }
}

@media (max-width: 640px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .usage-matrix-wrapper .usage-matrix-table {
    min-width: 600px;
  }
}
</style>
