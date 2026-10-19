<template>
  <div class="app-entry-container">
    <Card>
      <template #content>
        <div class="entry-header">
          <div>
            <h2>{{ pageTitle }}</h2>
            <p>{{ pageDescription }}</p>
          </div>
          <Button v-if="hasApps" :label="t('gameBusiness.appWorkspace.manageApps')" @click="goAppList"/>
        </div>
        <!-- 三层驾驶舱：租户未选应用时的汇总条（选择器形态才显示） -->
        <div v-if="!isCreateGuide" class="tenant-summary">
          <div class="summary-item">
            <span class="summary-label mono">APPS</span>
            <span class="summary-value">{{ allAppOptions.length }}<template v-if="quotaMax > 0"> / {{ quotaMax }}</template></span>
            <span class="summary-note">{{ t('gameBusiness.appWorkspace.summaryApps') }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label mono">ONLINE</span>
            <span class="summary-value">{{ onlineText }}</span>
            <span class="summary-note">{{ t('gameBusiness.appWorkspace.summaryOnline') }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label mono">ORDERS</span>
            <span class="summary-value">{{ summaryOrders }}</span>
            <span class="summary-note">{{ t('gameBusiness.appWorkspace.summaryOrders') }}</span>
          </div>
        </div>
      </template>
    </Card>

    <Card class="entry-body">
      <template #content>
        <div v-if="loading" class="entry-loading">{{ t('gameBusiness.appWorkspace.loading') }}</div>
        <div v-else-if="allAppOptions.length === 0" class="empty-state">
          <i class="fa-solid fa-circle-plus"></i>
          <h3>{{ t('gameBusiness.appWorkspace.noAppTitle') }}</h3>
          <p>{{ t('gameBusiness.appWorkspace.noAppDescription') }}</p>
          <Button :label="t('gameBusiness.appWorkspace.createApp')" @click="goAppList"/>
        </div>
        <div v-else class="app-grid">
          <button
            v-for="app in allAppOptions"
            :key="String(app.id)"
            class="app-option"
            :class="{ 'is-disabled': app.isEnabled === false }"
            type="button"
            :disabled="app.isEnabled === false"
            @click="enterWorkspace(app)"
          >
            <div class="app-option-main">
              <span class="app-name">{{ app.name }}</span>
              <Tag :severity="app.isEnabled === false ? 'danger' : 'success'" :value="app.isEnabled === false ? t('gameBusiness.app.disabled') : t('gameBusiness.app.enabled')"/>
            </div>
            <div class="app-meta">{{ t('gameBusiness.app.appId') }}: {{ app.id }}</div>
          </button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts" name="appEntry">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import {
  APP_CREATE_GUIDE_PATH,
  APP_MANAGE_PATH,
  normalizeAppId,
  visibleApps,
} from '../../../../utils/appWorkspace.ts'
import { globalPackageCurrentTenantPackageGet } from '../../../../api/globalPackage'
import { tenantGameHomeEntityOverviewPost } from '../../../../api/tenantGameHomeEntity'

const route = useRoute()
const router = useRouter()
const { appOptions, loadApps, setCurrentApp } = useCurrentApp()
const loading = ref(false)

const allAppOptions = computed(() => appOptions.value.filter((app) => normalizeAppId(app.id) !== undefined))
const visibleAppOptions = computed(() => visibleApps(appOptions.value))
const hasApps = computed(() => allAppOptions.value.length > 0)
const isCreateGuide = computed(() => route.path === APP_CREATE_GUIDE_PATH || visibleAppOptions.value.length === 0)
const pageTitle = computed(() => isCreateGuide.value ? t('gameBusiness.appWorkspace.createFirstApp') : t('gameBusiness.appWorkspace.selectApp'))
const pageDescription = computed(() => isCreateGuide.value ? t('gameBusiness.appWorkspace.createGuideDescription') : t('gameBusiness.appWorkspace.selectDescription'))

// ---- 租户汇总条数据（三层驾驶舱）----
// 套餐配额/应用数为真实接口；在线与今日订单走 #211 overview 聚合（失败显示 —/0）
const quotaMax = ref(0)
const onlineCount = ref<number | null>(null)
const summaryOrders = ref(0) // overview 失败时保持 0

const onlineText = computed(() => (onlineCount.value === null ? '—' : onlineCount.value.toLocaleString()))

const loadTenantSummary = async () => {
  try {
    const res = await globalPackageCurrentTenantPackageGet({})
    quotaMax.value = res.data.data?.maxGameAppCount ?? 0
  } catch {
    // 套餐拉取失败时省略配额展示
  }
  try {
    // #211 驾驶舱聚合：在线与今日订单一次拿
    const res = await tenantGameHomeEntityOverviewPost()
    onlineCount.value = res.data.data?.onlineCount ?? null
    summaryOrders.value = res.data.data?.todayOrderCount ?? 0
  } catch {
    onlineCount.value = null
  }
}

const goAppList = () => {
  router.push(APP_MANAGE_PATH)
}

const enterWorkspace = (app: { id?: unknown; isEnabled?: boolean | null }) => {
  const appId = normalizeAppId(app.id)
  if (appId === undefined || app.isEnabled === false) {
    return
  }
  setCurrentApp(appId)
  // 三层驾驶舱：选完应用直达应用驾驶舱；appWorkspace 门面保留为应用管理入口
  router.push({path: '/dashboard/appHome'})
}

onMounted(async () => {
  loading.value = true
  try {
    await loadApps(true)
    await loadTenantSummary()
  } finally {
    loading.value = false
  }
})
</script>

<style scoped lang="scss">
.app-entry-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.entry-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;

  h2 {
    margin: 0 0 6px;
    font-size: 20px;
  }

  p {
    margin: 0;
    color: var(--p-text-muted-color);
  }
}

// 租户汇总条：mono 版头标签 + 读数（与驾驶舱 KPI 同语言，安静形态）
.tenant-summary {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
  margin-top: 16px;
  padding-top: 14px;
  border-top: 1px solid var(--p-content-border-color);

  .summary-item {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
  }

  .mono {
    font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    font-size: 11px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--p-text-muted-color);
  }

  .summary-value {
    font-size: 22px;
    font-weight: 700;
    color: var(--p-text-color);
    font-variant-numeric: tabular-nums;
    line-height: 1.2;
  }

  .summary-note {
    font-size: 12px;
    color: var(--p-text-muted-color);
  }

  @include bp-down(sm) {
    grid-template-columns: 1fr;
    gap: 10px;
  }
}

.entry-body {
  min-height: 280px;
}

.entry-loading,
.empty-state {
  min-height: 220px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  text-align: center;
}

.empty-state {
  i {
    color: var(--p-primary-color);
    font-size: 28px;
  }

  h3,
  p {
    margin: 0;
  }

  p {
    color: var(--p-text-muted-color);
  }
}

.app-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 12px;
}

.app-option {
  min-height: 92px;
  padding: 14px;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  background: var(--p-content-background);
  color: var(--p-text-color);
  text-align: left;
  cursor: pointer;
}

.app-option:hover {
  border-color: var(--p-primary-color);
}

.app-option.is-disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.app-option-main {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.app-name {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-weight: 600;
}

.app-meta {
  margin-top: 12px;
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
