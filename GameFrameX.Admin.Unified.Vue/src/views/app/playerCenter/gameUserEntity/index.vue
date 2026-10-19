<template>
  <div class="gameUserEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameUserEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="onAddGameUserEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" showGridlines stripedRows v-block-loading="loading" rowKey="id" style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameUserEntity"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-status="{ data }">
            <Tag :value="getEnumDesc(data.status, getEnumStatusData_Index)" />
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameUserEntityTitle" @reloadTable="onQuery" />
        <mailDialog ref="mailDialogRef" :title="addGameUserEntityTitle" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameuser">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import mailDialog from '/@/views/app/playerCenter/gameUserEntity/component/MailDialog.vue'
import editDialog from '/@/views/app/playerCenter/gameUserEntity/component/EditDialog.vue'
import { tenantGameUserEntityPagePost, tenantGameUserEntityBanPost, tenantGameUserEntityUnsealPost } from '../../../../api/tenantGameUserEntity'
import {systemEnumEnumDataListGet} from '../../../../api/systemEnum';
import commonFunction from '../../../../utils/commonFunction.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'

const route = useRoute()
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumStatusData_Index = ref<any>([])
const {getEnumDesc} = commonFunction()
const {resolveInitialAppId} = useCurrentApp()
const editDialogRef = ref()
const mailDialogRef = ref()
const addGameUserEntityTitle = t('message.gameBusiness.user.addAccount')
const editGameUserEntityTitle = ref('')

const statusOptions = computed(() =>
  getEnumStatusData_Index.value.map((item: any) => ({
    label: `[${item.value}]  ${item.name} ${item.describe}`,
    value: item.value
  }))
)

const searchFields = computed(() => [
  { key: 'id', label: t('message.gameBusiness.user.userId'), type: 'input', placeholder: t('message.gameBusiness.user.pleaseEnterUserId') },
  { key: 'userName', label: t('message.gameBusiness.user.account'), type: 'input', placeholder: t('message.gameBusiness.user.pleaseEnterAccount') },
  { key: 'status', label: t('message.gameBusiness.area.status'), type: 'select', options: statusOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.user.pleaseSelectStatus'), advanced: true },
  { key: 'name', label: t('message.gameBusiness.user.nickname'), type: 'input', placeholder: t('message.gameBusiness.user.pleaseEnterNickname'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery: doQuery,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameUserEntityPagePost(params),
  },
  immediate: false,
})

const onQuery = async () => {
  await doQuery()
  getEnumStatusData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameUserStatus' })).data.data ?? []
}

const onPage = usePrimeTablePage(tableParams, doQuery)

const onAddGameUserEntity = () => {
  mailDialogRef.value.openDialog({})
}

const onBanGameRoleEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.user.confirmBan'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameUserEntityBanPost({id: row.id, appId: queryParams.value.appId})
  onQuery()
  showSuccess(t('message.gameBusiness.user.banSuccess'))
}

const onUnsealGameRoleEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.user.confirmUnban'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameUserEntityUnsealPost({id: row.id, appId: queryParams.value.appId})
  onQuery()
  showSuccess(t('message.gameBusiness.user.unbanSuccess'))
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'userName', header: t('message.gameBusiness.user.account'), width: '200px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
  {field: 'status', header: t('message.gameBusiness.area.status'), width: '140px', bodyKey: 'status'},
  {field: 'name', header: t('message.gameBusiness.user.nickname'), width: '200px'},
])

const customActions: CrudAction[] = [
  {
    auth: 'ban',
    label: t('message.gameBusiness.user.ban'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onBanGameRoleEntity,
    visible: (row: any) => row.status == 2,
  },
  {
    auth: 'unseal',
    label: t('message.gameBusiness.user.unban'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onUnsealGameRoleEntity,
    visible: (row: any) => row.status != 2,
  },
]

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery: onQuery,
})

onQuery()
</script>
