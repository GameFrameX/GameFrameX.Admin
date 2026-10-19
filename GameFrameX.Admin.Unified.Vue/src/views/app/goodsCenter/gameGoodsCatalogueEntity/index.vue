<template>
  <div class="gameGoodsCatalogueEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameGoodsCatalogueEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddGameGoodsCatalogueEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" showGridlines stripedRows v-block-loading="loading" rowKey="id" style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameGoodsCatalogueEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameGoodsCatalogueEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/goodsCenter/gameGoodsCatalogueEntity/component/EditDialog.vue'
import { tenantGameGoodsCatalogueEntityPagePost, tenantGameGoodsCatalogueEntityDeletePost } from '../../../../api/tenantGameGoodsCatalogueEntity';

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'description', label: t('message.gameBusiness.description'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterDescription'), advanced: true },
])

const editDialogRef = ref();
const editTitle = ref("");

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete
} = useCrudTable({
  api: {pageApi: tenantGameGoodsCatalogueEntityPagePost, deleteApi: tenantGameGoodsCatalogueEntityDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameGoodsCatalogueEntity = () => {
  editTitle.value = t('message.gameBusiness.goods.addGoodsCatalogue');
  editDialogRef.value.isEditor = false;
  editDialogRef.value.openDialog({});
};

const openEditGameGoodsCatalogueEntity = (row: any) => {
  editTitle.value = t('message.gameBusiness.goods.editGoodsCatalogue');
  editDialogRef.value.isEditor = true;
  editDialogRef.value.openDialog(row);
};

const actions = {
  edit: openEditGameGoodsCatalogueEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '200px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '200px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '200px', bodyKey: 'updateTime'},
])

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})
</script>
