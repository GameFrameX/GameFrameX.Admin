<template>
  <div class="gameRewardTemplateGroupEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="!!queryParams.appId"
            authPrefix="gameRewardTemplateGroupEntity"
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
                       authPrefix="gameRewardTemplateGroupEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-isEnabled="{ data }">
            <Tag :value="t('message.gameBusiness.enable')" severity="success" v-if="isEnabled(data)"/>
            <Tag :value="t('message.gameBusiness.disable')" severity="danger" v-else/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="gameRewardTemplateGroupEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {computed, ref} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/contentCenter/reward/gameRewardTemplateGroupEntity/component/EditDialog.vue'
import { tenantGameRewardTemplateGroupEntityDeletePost, tenantGameRewardTemplateGroupEntityPagePost } from '../../../../../api/tenantGameRewardTemplateGroupEntity'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const emptyPage = {data: {result: {items: [], total: 0}}}
const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()
const editDialogRef = ref()
const editTitle = ref('')

const enabledOptions = computed(() => [
  {label: t('message.gameBusiness.enable'), value: true},
  {label: t('message.gameBusiness.disable'), value: false},
])

const searchFields = computed(() => [
  {key: 'isEnabled', label: t('message.gameBusiness.isEnabled'), type: 'select', options: enabledOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.common.form.pleaseSelect'), advanced: true},
])

const {loading, tableData, queryParams, tableParams, onQuery, onDelete} = useCrudTable({
  api: {
    pageApi: (params) => params.appId ? tenantGameRewardTemplateGroupEntityPagePost(params) : Promise.resolve(emptyPage),
    deleteApi: (params) => tenantGameRewardTemplateGroupEntityDeletePost(params),
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAdd = () => {
  editTitle.value = t('message.gameBusiness.rewardTemplate.addGroup')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog()
}

const openEdit = (row: any) => {
  editTitle.value = t('message.gameBusiness.rewardTemplate.editGroup')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEdit,
  delete: {onClick: onDelete, visible: (row: any) => row.isDelete !== true},
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '160px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '220px'},
  {field: 'sort', header: t('message.gameBusiness.goods.sort'), width: '100px', align: 'center'},
  {field: 'isEnabled', header: t('message.gameBusiness.isEnabled'), width: '100px', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
])

useRouteAppIdQuerySync({
  route,
  queryParams,
  resolveAppId: resolveInitialAppId,
  onQuery,
})

</script>
