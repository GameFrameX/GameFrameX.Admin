<template>
  <div class="gameAnnouncementEntitycontainer">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            authPrefix="gameWebShopOrderEntity"
            @query="onQuery"
            
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameWebShopOrderEntity"
                       :actions="actions"
                       :customActions="customActions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-logo="{ data }">
            <img :src="getWebShopInfo(data.gameShopId)?.logo" :alt="getWebShopInfo(data.gameShopId)?.name || ''"
                 style="width:100px;height:100px;object-fit:fill"
                 v-if="getWebShopInfo(data.gameShopId)?.logo"/>
          </template>
          <template #body-name="{ data }">
            <span>{{ getWebShopInfo(data.gameShopId)?.name }}</span>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="redoDialogRef" :title="editwebShopTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="webShop">
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudAction } from '../../../../components/crud-data-table/types.ts'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { createAction } from '../../../../components/crud-data-table'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/webShopCenter/webShopOrder/component/EditDialog.vue'
import { tenantGameWebShopOrderEntityPagePost, tenantGameWebShopOrderEntityDeletePost } from '../../../../api/tenantGameWebShopOrderEntity'
import { tenantGameWebShopEntitySelectPost } from '../../../../api/tenantGameWebShopEntity'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
])

const redoDialogRef = ref()
const editwebShopTitle = ref('')

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete
} = useCrudTable({
  api: {pageApi: tenantGameWebShopOrderEntityPagePost, deleteApi: tenantGameWebShopOrderEntityDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

const actions = { delete: onDelete }

// Open redo dialog
const openEditwebShop = (row: any) => {
  editwebShopTitle.value = t('message.gameBusiness.webshop.orderRedo')
  redoDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'logo', header: t('message.gameBusiness.webshop.itemImage'), width: '140px', bodyKey: 'logo'},
  {field: 'name', header: t('message.gameBusiness.webshop.itemName'), width: '200px', bodyKey: 'name'},
  {field: 'orderId', header: t('message.gameBusiness.webshop.orderId'), width: '200px', numeric: true},
  {field: 'gameShopId', header: t('message.gameBusiness.webshop.itemId'), width: '200px', numeric: true},
  {field: 'roleId', header: t('message.gameBusiness.webshop.roleId'), width: '200px', numeric: true},
  {field: 'areaId', header: t('message.gameBusiness.webshop.areaId'), width: '200px', numeric: true},
  {field: 'reason', header: t('message.gameBusiness.webshop.failReason'), width: '200px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  createAction('redo', openEditwebShop, {
    label: t('message.gameBusiness.redo'),
    icon: 'fa-solid fa-pen-to-square',
    visible: (row: any) => !row.orderStatus,
  }),
]

// WebShop list for display
const wenShopListData = ref<Array<{ id: number; logo: string; name: string }>>([])
const getListWebShop = async (appId?: number) => {
  var res = await tenantGameWebShopEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  wenShopListData.value = res.data.data ?? []
}
const getWebShopInfo = (id: any) => {
  var item = wenShopListData.value.find((x) => x.id == id)
  if (item != null) {
    return item
  }
}

watch(() => queryParams.value.appId, (newVal) => {
  getListWebShop(newVal)
})

onMounted(async () => {
  await getListWebShop(queryParams.value.appId)
})
</script>
