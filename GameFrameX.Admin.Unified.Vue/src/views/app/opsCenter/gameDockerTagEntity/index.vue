<template>
  <div class="gameDockerEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameDockerEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddgameDockerEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameDockerEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editgameDockerEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameDockerEntity">
import {ref, computed, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import editDialog from '/@/views/app/opsCenter/gameDockerTagEntity/component/EditDialog.vue'
import { tenantGameDockerTagEntityPagePost, tenantGameDockerTagEntityDeletePost } from '../../../../api/tenantGameDockerTagEntity'
import {systemEnumEnumDataListGet} from '../../../../api/systemEnum';
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.name'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterName') },
])

// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumLanguageData_Index = ref<any>([])
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumServerStatusData_Index = ref<any>([])
const editDialogRef = ref()
const editgameDockerEntityTitle = ref('')

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery: doQuery,
  onDelete
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameDockerTagEntityPagePost(params),
    deleteApi: (params) => tenantGameDockerTagEntityDeletePost(params),
  },
  immediate: false,
})

const onQuery = async () => {
  await doQuery()
  getEnumLanguageData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameLanguage' })).data.data ?? []
  getEnumServerStatusData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameServerStatus' })).data.data ?? []
}

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddgameDockerEntity = () => {
  editgameDockerEntityTitle.value = t('message.gameBusiness.area.addArea')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditgameDockerEntity = (row: any) => {
  editgameDockerEntityTitle.value = t('message.gameBusiness.area.editArea')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'serverVersion', header: t('message.gameBusiness.area.tag'), width: '140px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '140px'},
])

const actions = {
  edit: openEditgameDockerEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true && row.isDeleted !== true && row.IsDeleted !== true },
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery: onQuery,
})

onMounted(async () => {
  await onQuery()
})
</script>
