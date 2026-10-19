<template>
  <div class="language-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="globalLanguage"
            @query="onQuery"
            
            @add="openAddGlobalLanguage"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" style="width: 100%" v-block-loading="loading" showGridlines stripedRows
                   rowKey="id" :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                   :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                   authPrefix="globalLanguage"
                   :actions="actions"
                   :columns="columns"
                   :exportQuery="queryParams"
                   @page="onPage">
          <template #body-isDefault="{ data }">
            <Tag v-if="data.isDefault" :value="t('message.common.label.yes')" />
            <Tag v-else :value="t('message.common.label.no')" severity="secondary" />
          </template>
          <template #body-description="{ data }">
            {{ getLanguageDescription(data) }}
          </template>
        </CrudDataTable>
        <editDialog
            ref="editDialogRef"
            :title="editGlobalLanguageTitle"
            @reloadTable="onQuery"
        />
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="language">
import {ref, computed} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import editDialog from '/@/views/platform/global/language/component/EditDialog.vue'
import { globalLanguagePagePost, globalLanguageDeletePost } from '../../../../api/globalLanguage'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const editDialogRef = ref()
const editGlobalLanguageTitle = ref('')

const searchFields = computed(() => [])

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: globalLanguagePagePost, deleteApi: globalLanguageDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const toIntlLocale = (value?: string) => (value || '').replace(/_/g, '-')

const getLanguageDescription = (row: any) => {
  const code = row?.code || ''
  if (code === 'default') {
    return t('message.client.defaultLanguageDescription')
  }

  const localeName = toIntlLocale(locale.value)
  const [languageCode, regionCode] = code.split('_')
  if (!languageCode || !regionCode) {
    return row?.description || code
  }

  try {
    const languageNames = new Intl.DisplayNames([localeName], { type: 'language' })
    const regionNames = new Intl.DisplayNames([localeName], { type: 'region' })
    const languageName = languageNames.of(languageCode)
    const regionName = regionNames.of(regionCode)
    if (languageName && regionName) {
      return `${languageName} (${regionName})`
    }
  } catch {
    // Fallback to the persisted description when the browser cannot format a locale.
  }

  return row?.description || code
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('message.common.label.name'), width: '140px'},
  {field: 'code', header: t('message.client.languageCode'), width: '140px'},
  {field: 'isDefault', header: t('message.client.isDefault'), width: '120px', bodyKey: 'isDefault'},
  {field: 'description', header: t('message.common.label.description'), width: '180px', bodyKey: 'description'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '210px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '210px'},
])

// 打开新增页面
const openAddGlobalLanguage = () => {
  editGlobalLanguageTitle.value = t('message.client.addLanguage')
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditGlobalLanguage = (row: any) => {
  editGlobalLanguageTitle.value = t('message.client.editLanguage')
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditGlobalLanguage,
  delete: onDelete,
}
</script>
