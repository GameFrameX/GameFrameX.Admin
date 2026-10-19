<template>
  <div class="gameMailTemplateEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameMailTemplateEntity"
            @query="onQuery"
            
            @add="openAddGameMailTemplateEntity"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                       rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameMailTemplateEntity"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage" showIdColumn idColumnWidth="180px">
          <template #body-isEnabled="{ data }">
            <Tag :value="t('message.gameBusiness.enable')" severity="success" v-if="isEnabled(data)"/>
            <Tag :value="t('message.gameBusiness.disable')" severity="danger" v-else/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGameMailTemplateEntityTitle" :appId="queryParams.appId" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="gameMailTemplateEntity">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'
import type {CrudColumnConfig} from '../../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../../utils/formatTime.ts'
import {ref, computed, watch, onMounted} from 'vue'
import {useRoute} from 'vue-router'
import editDialog from '/@/views/app/contentCenter/mail/gameMailTemplateEntity/component/EditDialog.vue'
import { tenantGameMailTemplateEntityPagePost, tenantGameMailTemplateEntityDeletePost } from '../../../../../api/tenantGameMailTemplateEntity'
import { tenantGameMailTemplateCategoryEntitySelectPost } from '../../../../../api/tenantGameMailTemplateCategoryEntity'
import {useCrudTable} from '../../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../../composables'
import {useCurrentApp} from '../../../../../composables/useCurrentApp.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'
import {isEnabled} from '../../../../../utils/isEnabled.ts'

const route = useRoute()
const {resolveInitialAppId} = useCurrentApp()

const editDialogRef = ref()
const editGameMailTemplateEntityTitle = ref('')
const categoryList = ref<any[]>([])

const enabledOptions = computed(() => [
  {label: t('message.gameBusiness.enable'), value: true},
  {label: t('message.gameBusiness.disable'), value: false},
])

const searchFields = computed(() => [
  { key: 'categoryId', label: t('message.gameBusiness.mail.category'), type: 'select', options: categoryList.value, optionLabel: 'value', optionValue: 'id', placeholder: t('message.gameBusiness.mail.pleaseSelectCategory'), advanced: true },
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
    pageApi: (params) => tenantGameMailTemplateEntityPagePost(params),
    deleteApi: (params) => tenantGameMailTemplateEntityDeletePost(params),
  },
  immediate: true,
})

queryParams.value.appId = resolveInitialAppId(route.query)

const loadCategoryList = async (appId?: number) => {
  const res = await tenantGameMailTemplateCategoryEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE})
  categoryList.value = res.data.data ?? []
}

watch(() => queryParams.value.appId, (newVal) => {
  queryParams.value.categoryId = undefined
  loadCategoryList(newVal)
})

onMounted(async () => {
  await loadCategoryList(queryParams.value.appId)
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGameMailTemplateEntity = () => {
  editGameMailTemplateEntityTitle.value = t('message.gameBusiness.mail.addMailTemplate')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

const openEditGameMailTemplateEntity = (row: any) => {
  editGameMailTemplateEntityTitle.value = t('message.gameBusiness.mail.editMailTemplate')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGameMailTemplateEntity,
  delete: { onClick: onDelete, visible: (row: any) => row.isDelete !== true },
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.gameBusiness.name'), width: '140px'},
  {field: 'categoryIdName', header: t('message.gameBusiness.mail.category'), width: '120px'},
  {field: 'titleTemplate', header: t('message.gameBusiness.mail.titleTemplate'), width: '140px'},
  {field: 'subTitleTemplate', header: t('message.gameBusiness.mail.subTitleTemplate'), width: '140px'},
  {field: 'bodyTemplate', header: t('message.gameBusiness.mail.bodyTemplate'), width: '200px'},
  {field: 'sort', header: t('message.gameBusiness.goods.sort'), width: '80px', align: 'center'},
  {field: 'isEnabled', header: t('message.gameBusiness.isEnabled'), width: '100px', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: t('common.label.createTime'), width: '200px', bodyKey: 'createdTime'},
])

watch(() => route.query.appId, async () => {
  queryParams.value.appId = resolveInitialAppId(route.query)
  queryParams.value.categoryId = undefined
  await onQuery()
})
</script>
