<template>
  <div class="gameBannerAnnouncementEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameBannerAnnouncementEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddGameBannerAnnouncementEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameBannerAnnouncementEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-areaId="{ data }">
            {{ getAreaName(data.areaId) }}
          </template>
          <template #body-startTime="{ data }">{{ formatUnix(data.startTime) }}</template>
          <template #body-endTime="{ data }">{{ formatUnix(data.endTime) }}</template>
          <template #body-isActive="{ data }">
            <Tag v-if="data.isActive" :value="t('message.gameBusiness.yes')" />
            <Tag v-else :value="t('message.gameBusiness.no')" severity="danger" />
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameBannerAnnouncementEntity">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import { useCrudTable } from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/contentCenter/announcement/gameBannerAnnouncementEntity/component/EditDialog.vue'
import { tenantGameBannerAnnouncementEntityPagePost, tenantGameBannerAnnouncementEntityDeletePost } from '../../../../../api/tenantGameBannerAnnouncementEntity'
import { tenantGameAreaEntitySelectStringPost } from '../../../../../api/tenantGameAreaEntity'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editTitle = ref('')

const areaList = ref<any>([])
const areaListOptions = computed(() =>
  areaList.value.map((item: any) => ({ id: item.id, label: `[${item.value}]` }))
)
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i]
    if (item.id == areaId) {
      return item.value
    }
  }
  return areaId
}
const loadAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  areaList.value = res.data.data ?? []
}

const searchFields = computed(() => [
  { key: 'areaId', label: t('message.gameBusiness.mail.areaId'), type: 'select', options: areaListOptions.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.mail.areaId'), advanced: true },
  { key: 'content', label: t('message.gameBusiness.announcement.content'), type: 'input', placeholder: t('message.gameBusiness.announcement.pleaseEnterContent'), advanced: true },
  { key: 'priority', label: t('message.gameBusiness.priority'), type: 'number', placeholder: t('message.gameBusiness.pleaseEnterPriority'), advanced: true },
  { key: 'interval', label: t('message.gameBusiness.bannerAnnouncement.interval'), type: 'number', placeholder: t('message.gameBusiness.bannerAnnouncement.pleaseEnterInterval'), advanced: true },
  { key: 'repeatCount', label: t('message.gameBusiness.bannerAnnouncement.repeatCount'), type: 'number', placeholder: t('message.gameBusiness.bannerAnnouncement.pleaseEnterRepeatCount'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: { pageApi: tenantGameBannerAnnouncementEntityPagePost, deleteApi: tenantGameBannerAnnouncementEntityDeletePost },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.areaId = undefined
  loadAreaList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.areaId = undefined
  await onQuery()
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameBannerAnnouncementEntity = () => {
  editTitle.value = t('message.gameBusiness.bannerAnnouncement.addBannerAnnouncement')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameBannerAnnouncementEntity = (row: any) => {
  editTitle.value = t('message.gameBusiness.bannerAnnouncement.editBannerAnnouncement')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGameBannerAnnouncementEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'areaId', header: t('message.gameBusiness.mail.areaId'), width: '140px', bodyKey: 'areaId'},
  {field: 'content', header: t('message.gameBusiness.announcement.content'), width: '140px'},
  {field: 'priority', header: t('message.gameBusiness.priority'), width: '140px'},
  {field: 'startTime', header: t('message.gameBusiness.startTime'), width: '140px', bodyKey: 'startTime'},
  {field: 'endTime', header: t('message.gameBusiness.endTime'), width: '140px', bodyKey: 'endTime'},
  {field: 'interval', header: t('message.gameBusiness.bannerAnnouncement.interval'), width: '140px'},
  {field: 'repeatCount', header: t('message.gameBusiness.bannerAnnouncement.repeatCount'), width: '140px'},
  {field: 'isActive', header: t('message.gameBusiness.isActive'), width: '120px', bodyKey: 'isActive'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

onMounted(async () => {
  await loadAreaList(queryParams.value.appId)
})
</script>
