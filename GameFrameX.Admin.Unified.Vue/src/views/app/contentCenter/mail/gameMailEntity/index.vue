<template>
  <div class="gameMailEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameMailEntity"
            @query="onQuery"

            @add="openAddGameMailEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameMailEntity"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       actionColumnWidth="360px"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-isTranslate="{ data }">
            <Tag :value="t('message.gameBusiness.yes')" v-if="data.isTranslate"/>
            <Tag :value="t('message.gameBusiness.no')" severity="info" v-else/>
          </template>
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-isAllPost="{ data }">
            <Tag :value="t('message.gameBusiness.yes')" v-if="data.isAllPost"/>
            <Tag :value="t('message.gameBusiness.no')" severity="danger" v-else/>
          </template>
          <template #body-status="{ data }">
            <Tag :value="getEnumDesc(data.status, getEnumStatusData_Index)"/>
          </template>
          <template #body-mailType="{ data }">
            <Tag :value="getEnumDesc(data.mailType, getEnumMailTypeData_Index)"/>
          </template>
          <template #body-expirationTime="{ data }">{{ formatUnix(data.expirationTime) }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameMailEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"></editDialog>
        <infoDrawer ref="infoDrawerRef"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameMailEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type { CrudAction, CrudColumnConfig } from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import {showConfirm, showSuccess} from '../../../../../utils/message.ts'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/contentCenter/mail/gameMailEntity/component/EditDialog.vue'
import infoDrawer from '/@/views/app/contentCenter/mail/gameMailEntity/component/InfoDrawer.vue'
import { tenantGameMailEntityPagePost, tenantGameMailEntityDeletePost, tenantGameMailEntitySendPost } from '../../../../../api/tenantGameMailEntity'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';
import commonFunction from '../../../../../utils/commonFunction.ts'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const route = useRoute()
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumStatusData_Index = ref<any>([])
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumMailTypeData_Index = ref<any>([])
const {getEnumDesc} = commonFunction()

const editDialogRef = ref()
const infoDrawerRef = ref()
const editGameMailEntityTitle = ref('')

const {resolveInitialAppId} = useCurrentApp()

// Area list (loaded by current app scope)
const areaList = ref<any>([])
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.value
    }
  }
  return areaId
}
const getAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  areaList.value = res.data.data ?? []
}

const searchFields = computed(() => [
  { key: 'areaId', label: t('message.gameBusiness.mail.areaId'), type: 'select', options: areaList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.order.pleaseSelectArea'), advanced: true },
  { key: 'status', label: t('message.gameBusiness.mail.mailStatus'), type: 'select', options: getEnumStatusData_Index.value, optionLabel: 'describe', optionValue: 'value', placeholder: t('message.gameBusiness.mail.pleaseSelectMailStatus'), advanced: true },
  { key: 'mailType', label: t('message.gameBusiness.mail.mailType'), type: 'select', options: getEnumMailTypeData_Index.value, optionLabel: 'describe', optionValue: 'value', placeholder: t('message.gameBusiness.mail.pleaseSelectMailTypeDefault'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete
} = useCrudTable({
  api: {pageApi: tenantGameMailEntityPagePost, deleteApi: tenantGameMailEntityDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

queryParams.value.appId = resolveInitialAppId(route.query)

// Load enum data
const loadEnums = async () => {
  getEnumStatusData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameMailStatus' })).data.data ?? []
  getEnumMailTypeData_Index.value = (await systemEnumEnumDataListGet({ EnumName: 'GameMailType' })).data.data ?? []
}
loadEnums()

// Open add dialog
const openAddGameMailEntity = () => {
  editGameMailEntityTitle.value = t('message.gameBusiness.mail.addMail')
  editDialogRef.value.openDialog({})
}

// Open edit dialog
const openEditGameMailEntity = (row: any) => {
  editGameMailEntityTitle.value = t('message.gameBusiness.mail.editMail')
  editDialogRef.value.openDialog(row)
}

// Open detail drawer
const getGameMailEntity = (row: any) => {
  infoDrawerRef.value.openDialog(row)
}

// Send mail
const onSendGameMailEntity = async (row: any) => {
  const confirmed = await showConfirm(t('message.gameBusiness.mail.confirmSendMail'), t('message.common.message.tip'))
  if (!confirmed) return
  await tenantGameMailEntitySendPost(row)
  onQuery()
  showSuccess(t('message.common.message.success'))
}

// Copy as new mail
const onCopyGameMailEntity = (row: any) => {
  editGameMailEntityTitle.value = t('message.gameBusiness.mail.copyMail')
  const copiedRow = {...row}
  delete copiedRow.id
  copiedRow.areaId = undefined
  copiedRow.playerIdListJson = '[]'
  copiedRow.expirationDate = undefined
  copiedRow.status = 0
  editDialogRef.value.openDialog(copiedRow)
}

const actions = {
  edit: { onClick: openEditGameMailEntity, visible: (row: any) => row.status === 0 },
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const customActions: CrudAction[] = [
  {
    auth: 'send',
    label: t('message.gameBusiness.send'),
    icon: 'fa-solid fa-pen-to-square',
    onClick: onSendGameMailEntity,
    visible: (row: any) => row.status === 0,
  },
  {
    auth: 'add',
    label: t('message.gameBusiness.copy'),
    icon: 'fa-solid fa-copy',
    severity: 'warn',
    onClick: onCopyGameMailEntity,
  },
  {
    auth: 'delete',
    label: t('message.gameBusiness.detail'),
    icon: 'fa-solid fa-circle-info',
    onClick: getGameMailEntity,
  },
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'title', header: t('message.gameBusiness.mail.title'), width: '140px'},
  {field: 'subTitle', header: t('message.gameBusiness.mail.subTitle'), width: '140px'},
  {field: 'body', header: t('message.gameBusiness.detailedContent'), width: '140px'},
  {field: 'isTranslate', header: t('message.gameBusiness.mail.isTranslate'), width: '140px', bodyKey: 'isTranslate'},
  {field: 'areaId', header: t('message.gameBusiness.mail.areaId'), width: '140px', bodyKey: 'areaId'},
  {field: 'playerIdListJson', header: t('message.gameBusiness.mail.playerIdList'), width: '200px'},
  {field: 'isAllPost', header: t('message.gameBusiness.mail.sendToAll'), width: '105px', bodyKey: 'isAllPost'},
  {field: 'status', header: t('message.gameBusiness.mail.mailStatus'), width: '140px', bodyKey: 'status'},
  {field: 'mailType', header: t('message.gameBusiness.mail.mailType'), width: '140px', bodyKey: 'mailType'},
  {field: 'expirationTime', header: t('message.gameBusiness.mail.expirationDate'), width: '200px', bodyKey: 'expirationTime'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
])

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
