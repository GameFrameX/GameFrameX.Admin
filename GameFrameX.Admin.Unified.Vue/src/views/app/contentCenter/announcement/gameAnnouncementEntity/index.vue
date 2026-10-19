<template>
  <div class="gameAnnouncementEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAnnouncementEntity"
            :defaultSearchKey="false"
            @query="onQuery"

            @add="openAddGameAnnouncementEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameAnnouncementEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-language="{ data }">
            {{ getLanguageName(data.language) }}
          </template>
          <template #body-startTime="{ data }">{{ formatUnix(data.startTime) }}</template>
          <template #body-endTime="{ data }">{{ formatUnix(data.endTime) }}</template>
          <template #body-isActive="{ data }">
            <Tag v-if="data.isActive" :value="t('message.gameBusiness.yes')" />
            <Tag v-else :value="t('message.gameBusiness.no')" severity="danger" />
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameAnnouncementEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameAnnouncementEntity">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import editDialog from '/@/views/app/contentCenter/announcement/gameAnnouncementEntity/component/EditDialog.vue'
import { tenantGameAnnouncementEntityPagePost, tenantGameAnnouncementEntityDeletePost } from '../../../../../api/tenantGameAnnouncementEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../api/tenantGameAreaEntity'
import { globalLanguageSelectPost } from '../../../../../api/globalLanguage'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import { useCrudTable } from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage } from '../../../../../composables'
import { useCurrentApp } from '../../../../../composables/useCurrentApp.ts'

const route = useRoute()
const editDialogRef = ref()
const editGameAnnouncementEntityTitle = ref('')

const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'language', label: t('message.gameBusiness.language'), type: 'select', options: languageOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.gameBusiness.pleaseSelectLanguage'), advanced: true },
  { key: 'areaId', label: t('message.gameBusiness.mail.areaId'), type: 'select', options: areaListOptions.value, optionLabel: 'label', optionValue: 'id', placeholder: t('message.gameBusiness.mail.pleaseSelectAreaId'), advanced: true },
  { key: 'title', label: t('message.gameBusiness.announcement.title'), type: 'input', placeholder: t('message.gameBusiness.announcement.pleaseEnterTitle') },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantGameAnnouncementEntityPagePost(params),
    deleteApi: (params) => tenantGameAnnouncementEntityDeletePost(params),
  },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameAnnouncementEntity = () => {
  editGameAnnouncementEntityTitle.value = t('message.gameBusiness.announcement.addAnnouncement')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameAnnouncementEntity = (row: any) => {
  editGameAnnouncementEntityTitle.value = t('message.gameBusiness.announcement.editAnnouncement')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGameAnnouncementEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.mail.areaId'), width: '140px', bodyKey: 'areaId'},
  {field: 'language', header: t('message.gameBusiness.language'), width: '140px', bodyKey: 'language'},
  {field: 'title', header: t('message.gameBusiness.mail.title'), width: '140px'},
  {field: 'content', header: t('message.gameBusiness.announcement.content'), width: '200px'},
  {field: 'startTime', header: t('message.gameBusiness.startTime'), width: '200px', bodyKey: 'startTime'},
  {field: 'endTime', header: t('message.gameBusiness.endTime'), width: '200px', bodyKey: 'endTime'},
  {field: 'priority', header: t('message.gameBusiness.priority'), width: '140px'},
  {field: 'isActive', header: t('message.gameBusiness.isActive'), width: '120px', bodyKey: 'isActive'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '140px', bodyKey: 'updateTime'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

const areaList = ref<any>([])
const languageOptions = ref<any[]>([])
const languageMap = computed(() => {
  const map: Record<string, string> = {}
  languageOptions.value.forEach((item: any) => {
    map[item.value] = item.label
  })
  return map
})
const areaListOptions = computed(() =>
  areaList.value.map((item: any) => ({ id: item.id, label: `[${item.value}]` }))
)
const getAreaName = (areaId?: number) => {
  if (!areaId) {
    return t('message.gameBusiness.announcement.allAreas')
  }
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.value
    }
  }
  return areaId
}
const getLanguageName = (language?: string) => languageMap.value[language || 'default'] || language || 'default'

const loadAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  areaList.value = res.data.data ?? []
}
const loadLanguageList = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  const languages = (res.data.data ?? []).map((item: any) => ({
    value: item.value,
    label: item.label ? `${item.label} (${item.value})` : item.value,
  }))
  languageOptions.value = [
    {value: 'default', label: t('message.gameBusiness.announcement.defaultLanguage')},
    ...languages.filter((item: any) => item.value !== 'default'),
  ]
}

// Reset dependent filter selections when current app changes
watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
  await loadLanguageList()
})
</script>
