<template>
  <div class="gameCommodityEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameCommodityEntity"
            @query="onQuery"
            
            @add="openAddGameCommodityEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameCommodityEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-status="{ data }">
            <Tag :value="data.status" />
          </template>
          <template #body-categoryId="{ data }">
            <span>{{ getCategoryName(data.categoryId) }}</span>
          </template>
          <template #body-cost="{ data }">
            <Tag v-if="data.cost" :value="t('message.gameBusiness.yes')" />
            <Tag v-else :value="t('message.gameBusiness.no')" severity="danger" />
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameCommodityEntity">
import {ref, computed, watch} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import { useCrudTable } from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/goodsCenter/gameCommodityEntity/component/EditDialog.vue'
import { tenantGameCommodityEntityPagePost, tenantGameCommodityEntityDeletePost } from '../../../../api/tenantGameCommodityEntity'
import { tenantGameGoodsCatalogueEntitySelectPost } from '../../../../api/tenantGameGoodsCatalogueEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editTitle = ref('')

// 类别名称本地映射：page 响应只带 categoryId，select 接口提供 {id, value(名称)} 字典
const catalogueOptions = ref<any[]>([])
const getCategoryName = (categoryId: any) => {
  const found = catalogueOptions.value.find((item: any) => item.id === categoryId)
  return found ? found.value : categoryId
}
const loadCatalogueOptions = async (appId?: number | string) => {
  const list = await tenantGameGoodsCatalogueEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  catalogueOptions.value = list.data.data ?? []
}

const searchFields = computed(() => [
  { key: 'status', label: t('message.gameBusiness.area.status'), type: 'input', placeholder: t('message.gameBusiness.user.pleaseSelectStatus'), advanced: true },
  { key: 'number', label: t('message.gameBusiness.commodity.number'), type: 'input', placeholder: t('message.gameBusiness.commodity.pleaseEnterNumber'), advanced: true },
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: { pageApi: tenantGameCommodityEntityPagePost, deleteApi: tenantGameCommodityEntityDeletePost },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameCommodityEntity = () => {
  editTitle.value = t('message.gameBusiness.commodity.addCommodity')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameCommodityEntity = (row: any) => {
  editTitle.value = t('message.gameBusiness.commodity.editCommodity')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGameCommodityEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'status', header: t('message.gameBusiness.area.status'), width: '140px', bodyKey: 'status'},
  {field: 'categoryId', header: t('message.gameBusiness.commodity.category'), width: '120px', bodyKey: 'categoryId'},
  {field: 'cost', header: t('message.gameBusiness.commodity.costRmb'), width: '120px', bodyKey: 'cost'},
  {field: 'number', header: t('message.gameBusiness.commodity.number'), width: '140px'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '140px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '140px'},
])

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

watch(() => queryParams.value.appId, (newVal) => {
  loadCatalogueOptions(newVal)
}, { immediate: true })
</script>
