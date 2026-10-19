<template>
  <div class="gameAreaEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAreaEntity"
            :defaultSearchKey="false"
            @query="onQuery"

            @add="openAddGameAreaEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameAreaEntity"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       actionColumnWidth="250px"
                       @page="onPage" showIdColumn :idColumnHeader="t('message.gameBusiness.mail.areaId')" idColumnWidth="170px">
          <template #body-isDefault="{ data }">
            <Tag v-if="data.isDefault" :value="t('message.gameBusiness.yes')"/>
            <Tag v-else severity="danger" :value="t('message.gameBusiness.no')"/>
          </template>
          <template #body-isRecommend="{ data }">
            <Tag v-if="data.isRecommend" :value="t('message.gameBusiness.yes')"/>
            <Tag v-else severity="danger" :value="t('message.gameBusiness.no')"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="isEnabled(data)" :value="t('message.gameBusiness.yes')"/>
            <Tag v-else severity="danger" :value="t('message.gameBusiness.no')"/>
          </template>
          <template #body-serverStatus="{ data }">
            <Tag :value="getEnumDesc(data.serverStatus, getEnumServerStatusData_Index)"/>
          </template>
          <template #body-language="{ data }">
            <Tag :value="getEnumDesc(data.language, getEnumLanguageData_Index)"/>
          </template>
          <template #body-creationTime="{ data }">{{ formatUnix(data.creationTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameAreaEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameAreaEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import editDialog from '/@/views/app/opsCenter/gameAreaEntity/component/EditDialog.vue'
import {isEnabled} from '../../../../utils/isEnabled.ts'
import { tenantGameAreaEntityDisableDeletePost, tenantGameAreaEntityOpenPost, tenantGameAreaEntityPagePost, tenantGameAreaEntityRestartPost, tenantGameAreaEntityStopPost } from '../../../../api/tenantGameAreaEntity'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import {systemEnumEnumDataListGet} from '../../../../api/systemEnum';
import commonFunction from '../../../../utils/commonFunction.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'

// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumLanguageData_Index = ref<any>([])
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumServerStatusData_Index = ref<any>([])
const {getEnumDesc} = commonFunction()

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const languageOptions = computed(() =>
    getEnumLanguageData_Index.value.map((item: any) => ({
      label: `[${item.value}] ${item.describe}`,
      value: item.value
    }))
)

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.name'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterName'), advanced: true },
  { key: 'language', label: t('message.gameBusiness.area.language'), type: 'select', options: languageOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.area.pleaseSelectLanguage'), advanced: true },
])

const editDialogRef = ref()
const editGameAreaEntityTitle = ref('')

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {pageApi: tenantGameAreaEntityPagePost, deleteApi: tenantGameAreaEntityDisableDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

// Load enum data
const loadEnums = async () => {
  getEnumLanguageData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameLanguage' })).data.data ?? []
  getEnumServerStatusData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameServerStatus' })).data.data ?? []
}
loadEnums()

// Load app dictionary for app context column/filter (shared, idempotent — GFX-78/C96)
// Open add dialog
const openAddGameAreaEntity = () => {
  editGameAreaEntityTitle.value = t('message.gameBusiness.area.addArea')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

// Open edit dialog
const openEditGameAreaEntity = (row: any) => {
  editGameAreaEntityTitle.value = t('message.gameBusiness.area.editArea')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

// Open game area
const onOpenGameAreaEntity = async (row: any) => {
  await tenantGameAreaEntityOpenPost({id: row.id})
}

// Stop game area
const stopGameAreaEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.area.confirmStopServer'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameAreaEntityStopPost(row)
  onQuery()
  showSuccess(t('message.gameBusiness.area.stopSuccess'))
}

// Restart game area
const restartGameAreaEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.area.confirmRestartServer'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameAreaEntityRestartPost(row)
  onQuery()
  showSuccess(t('message.gameBusiness.area.restartSuccess'))
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '140px'},
  {field: 'isDefault', header: t('message.gameBusiness.isDefault'), width: '120px', bodyKey: 'isDefault'},
  {field: 'isRecommend', header: t('message.gameBusiness.isRecommend'), width: '120px', bodyKey: 'isRecommend'},
  {field: 'classifyIdName', header: t('message.gameBusiness.area.classifyName'), width: '120px'},
  {field: 'isEnabled', header: t('message.gameBusiness.isEnabled'), width: '120px', bodyKey: 'isEnabled'},
  {field: 'serverStatus', header: t('message.gameBusiness.area.status'), width: '140px', bodyKey: 'serverStatus'},
  {field: 'capacity', header: t('message.gameBusiness.area.maxPlayers'), width: '90px'},
  {field: 'currentLoad', header: t('message.gameBusiness.area.currentPlayers'), width: '105px'},
  {field: 'creationTime', header: t('message.gameBusiness.area.enabledDate'), width: '140px', bodyKey: 'creationTime'},
  {field: 'serverVersion', header: t('message.gameBusiness.area.serverVersion'), width: '140px'},
  {field: 'serverType', header: t('message.gameBusiness.area.serverType'), width: '150px'},
  {field: 'language', header: t('message.gameBusiness.area.language'), width: '140px', bodyKey: 'language'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '140px', bodyKey: 'updateTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
])

const actions = {
  edit: { onClick: openEditGameAreaEntity, visible: (row: any) => !!row.serverContainerId },
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const customActions: CrudAction[] = [
  {
    auth: 'open',
    label: t('message.gameBusiness.openServer'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onOpenGameAreaEntity,
    visible: (row: any) => !row.serverContainerId,
  },
  {
    auth: 'restart',
    label: t('message.gameBusiness.restart'),
    icon: 'fa-solid fa-rotate-right',
    onClick: restartGameAreaEntity,
    visible: (row: any) => !!row.serverContainerId && row.isDelete !== true,
  },
  {
    auth: 'stop',
    label: t('message.gameBusiness.stop'),
    icon: 'fa-solid fa-play',
    onClick: stopGameAreaEntity,
    visible: (row: any) => !!row.serverContainerId && row.isDelete !== true,
  },
]

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
