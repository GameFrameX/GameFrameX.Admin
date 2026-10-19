<template>
  <div class="gameRoleEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :labelWidth="'100px'"
            :showAdd="true"
            authPrefix="gameRoleEntity"
            :defaultSearchKey="false"
            @query="onQuery"

            @add="openAddGameRoleEntity"
        >
          <template #field-rechargeMin="{ model }">
            <div class="flex gap-2 w-full">
              <InputNumber v-model="model.rechargeMin" :placeholder="t('message.gameBusiness.role.startValue')" class="w-full" />
              <InputNumber v-model="model.rechargeMax" :placeholder="t('message.gameBusiness.role.endValue')" class="w-full" />
            </div>
          </template>
        </SearchForm>
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameRoleEntity"
                       :actions="actions"
                       :customActions="customActions"
                       actionColumnWidth="300px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-isSandBox="{ data }">
            <Tag v-if="data.isSandBox" :value="t('message.gameBusiness.yes')" severity="danger" />
            <Tag v-else :value="t('message.gameBusiness.no')" />
          </template>
          <template #body-lastLoginTime="{ data }">{{ formatUnix(data.lastLoginTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameRoleEntity">
import {ref, computed, watch, onMounted} from 'vue'
import { useRoute } from 'vue-router'
import { showConfirm, showSuccess } from '../../../../utils/message.ts'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/playerCenter/gameRoleEntity/component/EditDialog.vue'
import { tenantGameRoleEntityPagePost, tenantGameRoleEntitySyncPost, tenantGameRoleEntityUpdateToSandBoxPost, tenantGameRoleEntityUpdateToUnSandBoxPost } from '../../../../api/tenantGameRoleEntity'
import { tenantGameRoleEntitySelectStringPost } from '../../../../api/tenantGameRoleEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'

const route = useRoute()
const editDialogRef = ref()
const editTitle = ref('')

const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'userId', label: t('message.gameBusiness.user.userId'), type: 'input', placeholder: t('message.gameBusiness.role.pleaseEnterUserId') },
  { key: 'Id', label: t('message.gameBusiness.redeemcode.playerId'), type: 'input', placeholder: t('message.gameBusiness.redeemcode.pleaseEnterPlayerId') },
  { key: 'areaId', label: t('message.gameBusiness.role.areaName'), type: 'select', options: areaOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.gameBusiness.order.pleaseSelectArea'), advanced: true },
  { key: 'isSandBox', label: t('message.gameBusiness.role.sandboxUser'), type: 'switch', advanced: true },
  { key: 'rechargeMin', label: t('message.gameBusiness.role.rechargeRange'), type: 'input', advanced: true },
  { key: 'name', label: t('message.gameBusiness.role.roleName'), type: 'input', placeholder: t('message.gameBusiness.role.pleaseEnterRoleName'), advanced: true },
  { key: 'registerTime', label: t('message.gameBusiness.role.registerTimeRange'), type: 'dateRange', placeholder: t('message.gameBusiness.startTime'), advanced: true },
  { key: 'logoutTime', label: t('message.gameBusiness.role.lastLoginTimeRange'), type: 'dateRange', placeholder: t('message.gameBusiness.startTime'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: { pageApi: tenantGameRoleEntityPagePost },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameRoleEntity = () => {
  editTitle.value = t('message.gameBusiness.role.addRole')
  editDialogRef.value.openDialog({})
}

const openEditGameRoleEntity = (row: any) => {
  editTitle.value = t('message.gameBusiness.role.editRole')
  editDialogRef.value.openDialog(row)
}

// 设置为沙盒用户
const onSandBoxGameRoleEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.role.confirmSetSandbox'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameRoleEntityUpdateToSandBoxPost(row)
  onQuery()
  showSuccess(t('message.common.message.success'))
}

// 设置为非沙盒用户
const onUnSandBoxGameRoleEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.role.confirmUnsetSandbox'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameRoleEntityUpdateToUnSandBoxPost(row)
  onQuery()
  showSuccess(t('message.common.message.success'))
}

// 同步
const onSyncGameRoleEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.role.confirmSyncFromServer'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameRoleEntitySyncPost(row)
  onQuery()
  showSuccess(t('message.gameBusiness.role.syncSuccess'))
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.role.areaName'), width: '180px', bodyKey: 'areaId'},
  {field: 'roleId', header: t('message.gameBusiness.redeemcode.playerId'), width: '240px'},
  {field: 'userId', header: t('message.gameBusiness.user.userId'), width: '240px'},
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
  {field: 'allRecharge', header: t('message.gameBusiness.role.allRecharge'), width: '140px'},
  {field: 'currentLevelConfigId', header: t('message.gameBusiness.role.currentLevel'), width: '140px'},
  {field: 'isSandBox', header: t('message.gameBusiness.role.sandboxUser'), width: '105px', bodyKey: 'isSandBox'},
  {field: 'lastLoginTime', header: t('message.gameBusiness.role.lastLoginTime'), width: '200px', bodyKey: 'lastLoginTime'},
])

const actions = {
  edit: openEditGameRoleEntity,
}

const customActions: CrudAction[] = [
  {
    auth: 'unSandbox',
    label: t('message.gameBusiness.role.setToNonSandbox'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onUnSandBoxGameRoleEntity,
    visible: (row: any) => !!row.isSandBox,
  },
  {
    auth: 'sandbox',
    label: t('message.gameBusiness.role.setToSandbox'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onSandBoxGameRoleEntity,
    visible: (row: any) => !row.isSandBox,
  },
  {
    auth: 'sync',
    label: t('message.gameBusiness.sync'),
    icon: 'fa-solid fa-arrows-rotate',
    onClick: onSyncGameRoleEntity,
  },
]

const userList = ref<any>([])
const onUserSearch = async (query: string) => {
  if (query) {
    const res = await tenantGameRoleEntitySelectStringPost({ searchKey: query, pageSize: MAX_SELECT_SIZE })
    userList.value = res.data.data ?? []
  } else {
    userList.value = []
  }
}

const areaList = ref<any>([])
const areaOptions = computed(() =>
  areaList.value.map((item: any) => ({ id: item.id, label: `[${item.value}]` }))
)
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.label
    }
  }
  return areaId
}

const getAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({ appId: appId, pageSize: MAX_SELECT_SIZE })
  areaList.value = res.data.data ?? []
}

// Reset dependent filter selections when current app changes
watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  getAreaList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

onMounted(async () => {
  await getAreaList(queryParams.value.appId)
})
</script>
