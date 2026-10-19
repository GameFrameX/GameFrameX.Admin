<template>
  <div class="gameMailTemplateCategoryEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameMailTemplateCategoryEntity"
            :defaultSearchKey="false"
            @query="onQuery"
            
            @add="openAddGameMailTemplateCategoryEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameMailTemplateCategoryEntity"
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
        <editDialog ref="editDialogRef" :title="editGameMailTemplateCategoryEntityTitle"
                    :appId="queryParams.appId"
                    @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameMailTemplateCategoryEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {ref, computed} from 'vue'
import {useRoute} from 'vue-router'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage, useRouteAppIdQuerySync} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import editDialog from '/@/views/app/contentCenter/mail/gameMailTemplateCategoryEntity/component/EditDialog.vue'
import { tenantGameMailTemplateCategoryEntityPagePost, tenantGameMailTemplateCategoryEntityDeletePost } from '../../../../../api/tenantGameMailTemplateCategoryEntity'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editGameMailTemplateCategoryEntityTitle = ref('')

const enabledOptions = computed(() => [
  {label: t('message.gameBusiness.enable'), value: true},
  {label: t('message.gameBusiness.disable'), value: false},
])

const searchFields = computed(() => [
  { key: 'name', label: t('message.gameBusiness.name'), type: 'input', placeholder: t('message.gameBusiness.pleaseEnterName'), advanced: true },
  { key: 'isEnabled', label: t('message.gameBusiness.isEnabled'), type: 'select', options: enabledOptions.value, optionLabel: 'label', optionValue: 'value', placeholder: t('message.common.form.pleaseSelect'), advanced: true },
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
    pageApi: (params) => tenantGameMailTemplateCategoryEntityPagePost(params),
    deleteApi: (params) => tenantGameMailTemplateCategoryEntityDeletePost(params),
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameMailTemplateCategoryEntity = () => {
  editGameMailTemplateCategoryEntityTitle.value = t('message.gameBusiness.mail.addMailTemplateCategory')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameMailTemplateCategoryEntity = (row: any) => {
  editGameMailTemplateCategoryEntityTitle.value = t('message.gameBusiness.mail.editMailTemplateCategory')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGameMailTemplateCategoryEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'description', header: t('message.gameBusiness.description'), width: '200px'},
  {field: 'icon', header: t('message.gameBusiness.goods.icon'), width: '140px'},
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
