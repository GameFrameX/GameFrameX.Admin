<template>
  <div class="gameRewardTemplateEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="!!queryParams.appId"
            authPrefix="gameRewardTemplateEntity"
            @query="onQuery"
            @add="openAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameRewardTemplateEntity"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       actionColumnWidth="220px"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-rewardItemsJson="{ data }">{{ rewardSummary(data.rewardItemsJson) }}</template>
          <template #body-isEnabled="{ data }">
            <Tag :value="t('message.gameBusiness.enable')" severity="success" v-if="isEnabled(data)"/>
            <Tag :value="t('message.gameBusiness.disable')" severity="danger" v-else/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
        <Dialog v-model:visible="snapshotVisible" modal :header="t('message.gameBusiness.rewardTemplate.snapshotPreview')" :style="{ width: 'min(720px, calc(100vw - 32px))' }">
          <Textarea v-model="snapshotJson" readonly autoResize class="w-full font-mono snapshot-textarea"/>
        </Dialog>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameRewardTemplateEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {computed, ref, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import editDialog from '/@/views/app/contentCenter/reward/gameRewardTemplateEntity/component/EditDialog.vue'
import { tenantGameRewardTemplateEntityCopySnapshotPost, tenantGameRewardTemplateEntityDeletePost, tenantGameRewardTemplateEntityPagePost } from '../../../../../api/tenantGameRewardTemplateEntity'
import {parseRewardItems} from '../../../../../components/reward-items-editor/rewardItems.ts'
import { tenantGameRewardTemplateGroupEntitySelectPost } from '../../../../../api/tenantGameRewardTemplateGroupEntity'
import {isEnabled} from '../../../../../utils/isEnabled.ts'
import type {CrudAction, CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import type {AppWorkspaceId} from '../../../../../utils/appWorkspace.ts'

const emptyPage = {data: {result: {items: [], total: 0}}}
const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()
const editDialogRef = ref()
const editTitle = ref('')
const groupList = ref<any[]>([])
const snapshotVisible = ref(false)
const snapshotJson = ref('')

const enabledOptions = computed(() => [
  {label: t('message.gameBusiness.enable'), value: true},
  {label: t('message.gameBusiness.disable'), value: false},
])

const searchFields = computed(() => [
  {key: 'groupId', label: t('message.gameBusiness.rewardTemplate.group'), type: 'select', options: groupList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.rewardTemplate.pleaseSelectGroup'), advanced: true},
  {key: 'isEnabled', label: t('message.gameBusiness.isEnabled'), type: 'select', options: enabledOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.common.form.pleaseSelect'), advanced: true},
])

const {loading, tableData, queryParams, tableParams, onQuery, onDelete} = useCrudTable({
  api: {
    pageApi: (params) => params.appId ? tenantGameRewardTemplateEntityPagePost(params) : Promise.resolve(emptyPage),
    deleteApi: (params) => tenantGameRewardTemplateEntityDeletePost(params),
  },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const loadGroupList = async (appId?: AppWorkspaceId) => {
  groupList.value = appId ? ((await tenantGameRewardTemplateGroupEntitySelectPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []) : []
}

const rewardSummary = (value?: string) => {
  // 载荷经 parseRewardItems 归一为 camelCase；编辑器写入的行 ItemId 恒为 0、道具 Id 在 id，故保留 itemId→id 回退展示
  return parseRewardItems(value)
    .map((u) => `${u.itemId || u.id} x ${u.count}`)
    .join(', ')
}

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.groupId = undefined
  loadGroupList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.groupId = undefined
  await onQuery()
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = t('message.gameBusiness.rewardTemplate.addTemplate')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog()
}

const openEdit = (row: any) => {
  editTitle.value = t('message.gameBusiness.rewardTemplate.editTemplate')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const previewSnapshot = async (row: any) => {
  const response = await tenantGameRewardTemplateEntityCopySnapshotPost({id: row.id, appId: queryParams.value.appId})
  const snapshot = response.data.data
  snapshotJson.value = JSON.stringify(snapshot ? JSON.parse(snapshot.rewardItemsJson || '[]') : [], null, 2)
  snapshotVisible.value = true
}

const actions = {
  edit: openEdit,
  delete: {onClick: onDelete, visible: (row: any) => row.isDelete !== true},
}

const customActions: CrudAction[] = [
  {
    auth: 'CopySnapshot',
    label: t('message.gameBusiness.rewardTemplate.copySnapshot'),
    icon: 'fa-solid fa-copy',
    onClick: previewSnapshot,
  },
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '160px'},
  {field: 'groupName', header: t('message.gameBusiness.rewardTemplate.group'), width: '160px'},
  {field: 'rewardItemsJson', header: t('message.gameBusiness.rewardTemplate.rewardItems'), width: '360px', bodyKey: 'rewardItemsJson'},
  {field: 'sort', header: t('message.gameBusiness.goods.sort'), width: '100px', align: 'center'},
  {field: 'isEnabled', header: t('message.gameBusiness.isEnabled'), width: '100px', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
])

onMounted(() => loadGroupList(queryParams.value.appId))
</script>

<style scoped>
.snapshot-textarea {
  min-height: 260px;
}
</style>
