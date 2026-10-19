<template>
  <div class="gameGoodsEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameGoodsEntity"
            @query="onQuery"

            @add="openAddGameGoodsEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" showGridlines stripedRows v-block-loading="loading" rowKey="id" style="width: 100%"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameGoodsEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-classifyId="{ data }">
            {{ getClassifyName(data.classifyId) }}
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #body-updateTime="{ data }">{{ formatUnix(data.updateTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery" />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameGoodsEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {useCurrentApp} from '../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/goodsCenter/gameGoodsEntity/component/EditDialog.vue'
import { tenantGameGoodsEntityPagePost, tenantGameGoodsEntityDeletePost } from '../../../../api/tenantGameGoodsEntity';
import { tenantGameGoodsCatalogueEntitySelectPost } from '../../../../api/tenantGameGoodsCatalogueEntity';
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts';

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const searchFields = computed(() => [
  { key: 'configId', label: t('message.gameBusiness.goods.configId'), type: 'number', placeholder: t('message.gameBusiness.goods.pleaseEnterConfigId'), advanced: true },
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
  api: {pageApi: tenantGameGoodsEntityPagePost, deleteApi: tenantGameGoodsEntityDeletePost},
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameGoodsEntity = () => {
  editTitle.value = t('message.gameBusiness.goods.addGoods');
  editDialogRef.value.isEditor = false;
  editDialogRef.value.openDialog({});
};

const openEditGameGoodsEntity = (row: any) => {
  editTitle.value = t('message.gameBusiness.goods.editGoods');
  editDialogRef.value.isEditor = true;
  editDialogRef.value.openDialog(row);
};

const actions = {
  edit: openEditGameGoodsEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'configId', header: t('message.gameBusiness.goods.configId'), width: '140px'},
  {field: 'classifyId', header: t('message.gameBusiness.goods.classify'), width: '120px', bodyKey: 'classifyId'},
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '140px'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
  {field: 'updateTime', header: t('message.gameBusiness.updateTime'), width: '200px', bodyKey: 'updateTime'},
])

const gameGoodsCatalogueEntityClassifyIdDropdownList = ref<any>([]);
const getClassifyName = (areaId: number) => {
  for (let i = 0; i < gameGoodsCatalogueEntityClassifyIdDropdownList.value.length; i++) {
    const item = gameGoodsCatalogueEntityClassifyIdDropdownList.value[i]
    if (item.id == areaId) {
      return item.label;
    }
  }
  return areaId;
};
const getGameGoodsCatalogueEntityClassifyIdDropdownList = async (appId?: number) => {
  let list = await tenantGameGoodsCatalogueEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE});
  gameGoodsCatalogueEntityClassifyIdDropdownList.value = list.data.data ?? [];
};

// Reset dependent filter selections when current app changes
watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.classifyId = undefined
  getGameGoodsCatalogueEntityClassifyIdDropdownList(newVal)
})

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.classifyId = undefined
  await onQuery()
})

onMounted(async () => {
  await getGameGoodsCatalogueEntityClassifyIdDropdownList(queryParams.value.appId)
})
</script>
