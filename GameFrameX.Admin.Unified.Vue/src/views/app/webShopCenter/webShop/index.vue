<template>
  <div class="gameAnnouncementEntitycontainer">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameWebShopEntity"
            @query="onQuery"

            @add="openAddwebShop"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameWebShopEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-logo="{ data }">
            <img :src="data.logo" :alt="data.name" style="width:100px;height:100px;object-fit:fill"/>
          </template>
          <template #body-itemJson="{ data }">
            {{ data.itemJson }}
          </template>
          <template #body-isUp="{ data }">
            <Tag :value="t('message.gameBusiness.yes')" v-if="data.isUp"/>
            <Tag :value="t('message.gameBusiness.no')" severity="info" v-else/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editwebShopTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="webShop">
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type { CrudColumnConfig } from '../../../../components/crud-data-table/types.ts'
import { formatUnix } from '../../../../utils/formatTime.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/webShopCenter/webShop/component/EditDialog.vue'
import { tenantGameWebShopEntityPagePost, tenantGameWebShopEntityDeletePost } from '../../../../api/tenantGameWebShopEntity'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
])

const editDialogRef = ref()
const editwebShopTitle = ref('')

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete
} = useCrudTable({
  api: {pageApi: tenantGameWebShopEntityPagePost, deleteApi: tenantGameWebShopEntityDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

// Open add dialog
const openAddwebShop = () => {
  editwebShopTitle.value = t('message.gameBusiness.webshop.addItem')
  editDialogRef.value.openDialog({})
}

// Open edit dialog
const openEditwebShop = (row: any) => {
  editwebShopTitle.value = t('message.gameBusiness.webshop.editItem')
  editDialogRef.value.openDialog(row)
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'logo', header: t('message.gameBusiness.webshop.itemImage'), width: '140px', bodyKey: 'logo'},
  {field: 'name', header: t('message.gameBusiness.webshop.itemName'), width: '200px'},
  {field: 'itemJson', header: t('message.gameBusiness.webshop.includeItems'), width: '200px', bodyKey: 'itemJson'},
  {field: 'description', header: t('message.gameBusiness.webshop.itemDescription'), width: '200px'},
  {field: 'keyWords', header: t('message.gameBusiness.webshop.itemKeywords'), width: '200px'},
  {field: 'price', header: t('message.gameBusiness.webshop.retailPrice'), width: '140px', numeric: true},
  {field: 'realPrice', header: t('message.gameBusiness.webshop.realPrice'), width: '120px', numeric: true},
  {field: 'weight', header: t('message.gameBusiness.webshop.sortOrder'), width: '140px', numeric: true},
  {field: 'isUp', header: t('message.gameBusiness.webshop.isUp'), width: '140px', bodyKey: 'isUp'},
  {field: 'sales', header: t('message.gameBusiness.webshop.sales'), width: '140px', numeric: true},
  {field: 'createdTime', header: t('common.label.createTime'), width: '140px', bodyKey: 'createdTime'},
])

const actions = {
  edit: openEditwebShop,
  delete: onDelete,
}

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
