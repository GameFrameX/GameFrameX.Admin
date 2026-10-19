<template>
  <div class="app-workspace-container">
    <Card>
      <template #content>
        <div class="workspace-header">
          <div class="workspace-title">
            <h2>{{ current?.name || t('gameBusiness.appWorkspace.title') }}</h2>
            <div class="workspace-meta">
              <Tag v-if="current" :severity="current.isEnabled === false ? 'danger' : 'success'" :value="current.isEnabled === false ? t('gameBusiness.app.disabled') : t('gameBusiness.app.enabled')"/>
              <span v-if="current">{{ t('gameBusiness.app.appId') }}: {{ current.id }}</span>
            </div>
          </div>
          <div class="workspace-actions">
            <Select
              v-model="selectedAppId"
              :options="visibleAppOptions"
              optionLabel="name"
              optionValue="id"
              filter
              :placeholder="t('gameBusiness.appWorkspace.selectApp')"
              class="app-select"
              @change="onAppChange"
            />
            <Button :label="t('gameBusiness.appWorkspace.manageApps')" severity="secondary" @click="goAppList"/>
          </div>
        </div>
      </template>
    </Card>

    <div class="summary-grid">
      <Card>
        <template #content>
          <div class="summary-item">
            <span>{{ t('gameBusiness.appWorkspace.appStatus') }}</span>
            <strong>{{ current?.isEnabled === false ? t('gameBusiness.app.disabled') : t('gameBusiness.app.enabled') }}</strong>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>{{ t('gameBusiness.appWorkspace.packageSummary') }}</span>
            <strong>{{ packageSummary }}</strong>
          </div>
        </template>
      </Card>
      <Card>
        <template #content>
          <div class="summary-item">
            <span>{{ t('gameBusiness.appWorkspace.releaseSummary') }}</span>
            <strong>{{ t('gameBusiness.appWorkspace.useExistingPages') }}</strong>
          </div>
        </template>
      </Card>
    </div>

    <Card>
      <template #content>
        <div class="link-grid">
          <button v-for="link in links" :key="link.path" class="workspace-link" type="button" @click="go(link.path)">
            <component :is="link.icon" class="link-icon"/>
            <span>{{ link.label }}</span>
          </button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts" name="appWorkspace">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { tenantGameAppPackageListGet } from '../../../../api/tenantGameAppPackage'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import {
  APP_MANAGE_PATH,
  APP_SELECTION_PATH,
  APP_WORKSPACE_PATH,
  type AppWorkspaceId,
  normalizeAppId,
  visibleApps,
} from '../../../../utils/appWorkspace.ts'
import { faBullhorn, faCartShopping, faFolderOpen, faGauge, faGear, faLock, faLockOpen, faShop, faTrophy } from '../../../../utils/icons.ts'

const route = useRoute()
const router = useRouter()
const { appOptions, loadApps, currentAppId, setCurrentApp, currentApp } = useCurrentApp()
const selectedAppId = ref<AppWorkspaceId | undefined>(undefined)
const packageTotal = ref<number | undefined>(undefined)
const packageEnabled = ref<number | undefined>(undefined)

const visibleAppOptions = computed(() => visibleApps(appOptions.value))
const current = computed(() => currentApp())
const packageSummary = computed(() => {
  if (packageTotal.value === undefined) {
    return t('gameBusiness.appWorkspace.loadingSummary')
  }
  return `${packageEnabled.value ?? 0}/${packageTotal.value}`
})
const links = computed(() => [
  { label: t('gameBusiness.appWorkspace.assetPackages'), path: '/app/appCenter/resource/assetPackage', icon: faFolderOpen },
  { label: t('gameBusiness.appWorkspace.packages'), path: '/app/appCenter/gameAppPackage', icon: faFolderOpen },
  { label: t('gameBusiness.appWorkspace.assetPackageVersions'), path: '/app/appCenter/release/assetPackageVersion', icon: faTrophy },
  { label: t('gameBusiness.appWorkspace.remoteConfig'), path: '/app/appCenter/remoteConfig/config', icon: faGear },
  // C375 凭证中心操作台（C376 菜单 seed 落地后，旧 appSecurityWorkspace 由菜单 IA 收敛下线）
  { label: t('gameBusiness.credentialCenter.title'), path: '/app/appCenter/credentialCenter', icon: faLock },
  { label: t('gameBusiness.appWorkspace.security'), path: '/app/appCenter/appSecurityWorkspace', icon: faLockOpen },
  { label: t('gameBusiness.appWorkspace.players'), path: '/app/playerCenter/gameRoleEntity', icon: faGauge },
  { label: t('gameBusiness.appWorkspace.content'), path: '/app/contentCenter/mail/gameMailEntity', icon: faBullhorn },
  { label: t('gameBusiness.appWorkspace.goods'), path: '/app/goodsCenter/gameGoodsEntity', icon: faCartShopping },
  { label: t('gameBusiness.appWorkspace.webShop'), path: '/app/webShopCenter/itemManager', icon: faShop },
])

const loadPackageSummary = async (appId: AppWorkspaceId | undefined) => {
  packageTotal.value = undefined
  packageEnabled.value = undefined
  if (appId === undefined) {
    return
  }
  const res = await tenantGameAppPackageListGet({ appId: appId })
  const rows = (res.data?.data ?? []) as any[]
  packageTotal.value = rows.length
  packageEnabled.value = rows.filter((row) => row.isEnabled !== false).length
}

const ensureSelectedApp = async () => {
  await loadApps()
  const queryId = normalizeAppId(route.query.appId)
  const candidate = queryId ?? currentAppId.value
  const valid = visibleAppOptions.value.find((app) => String(app.id) === String(candidate))
  if (!valid) {
    router.replace(APP_SELECTION_PATH)
    return
  }
  const appId = normalizeAppId(valid.id)
  setCurrentApp(appId)
  selectedAppId.value = appId
  await loadPackageSummary(appId)
}

const onAppChange = async () => {
  const appId = normalizeAppId(selectedAppId.value)
  if (appId === undefined) {
    return
  }
  setCurrentApp(appId)
  await router.replace({ path: APP_WORKSPACE_PATH, query: { appId: appId } })
  await loadPackageSummary(appId)
}

const goAppList = () => {
  router.push(APP_MANAGE_PATH)
}

const go = (path: string) => {
  if (selectedAppId.value === undefined) {
    return
  }
  router.push({ path, query: { appId: selectedAppId.value } })
}

watch(() => route.query.appId, () => {
  ensureSelectedApp()
})

onMounted(() => {
  ensureSelectedApp()
})
</script>

<style scoped lang="scss">
.app-workspace-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.workspace-header,
.workspace-actions,
.workspace-meta {
  display: flex;
  align-items: center;
  gap: 12px;
}

.workspace-header {
  justify-content: space-between;
  flex-wrap: wrap;
}

.workspace-title h2 {
  margin: 0 0 8px;
  font-size: 20px;
}

.workspace-meta {
  color: var(--p-text-muted-color);
}

.app-select {
  min-width: 240px;
}

.summary-grid,
.link-grid {
  display: grid;
  gap: 8px;
}

.summary-grid {
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.summary-item {
  min-height: 60px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 8px;

  span {
    color: var(--p-text-muted-color);
  }

  strong {
    font-size: 18px;
  }
}

.link-grid {
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
}

.workspace-link {
  min-height: 64px;
  padding: 12px;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  background: var(--p-content-background);
  color: var(--p-text-color);
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.workspace-link:hover {
  border-color: var(--p-primary-color);
}

.link-icon {
  color: var(--p-primary-color);
  flex: 0 0 auto;
}

@media (max-width: 640px) {
  .workspace-actions,
  .app-select {
    width: 100%;
  }
}
</style>
