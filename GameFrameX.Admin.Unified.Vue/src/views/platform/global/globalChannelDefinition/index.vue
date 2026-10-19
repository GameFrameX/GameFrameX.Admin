<template>
  <div class="channel-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="globalChannelDefinition"
            @query="onQuery"
            
            @add="openAddGlobalChannelDefinition"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="globalChannelDefinition"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-scope="{ data }">
            <Tag :value="data.scope === 1 ? t('message.client.channelScopeSystem') : t('message.client.channelScopeTenant')" :severity="data.scope === 1 ? 'success' : 'info'"/>
          </template>
          <template #body-channelType="{ data }">
            {{ channelTypeText(data.channelType) }}
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled !== false" :value="t('message.common.label.yes')" severity="success"/>
            <Tag v-else :value="t('message.common.label.no')" severity="danger"/>
          </template>
          <template #body-isDefault="{ data }">
            <Tag v-if="data.isDefault" :value="t('message.common.label.yes')"/>
            <Tag v-else :value="t('message.common.label.no')" severity="info"/>
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editGlobalChannelDefinitionTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="globalChannelDefinition">
import {ref, computed} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import editDialog from '/@/views/platform/global/globalChannelDefinition/component/EditDialog.vue'
import { globalChannelDefinitionPagePost, globalChannelDefinitionDeletePost } from '../../../../api/globalChannelDefinition'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const editDialogRef = ref()
const editGlobalChannelDefinitionTitle = ref('')

const searchFields = computed(() => [])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
  onDelete,
} = useCrudTable({
  api: {
    pageApi: globalChannelDefinitionPagePost,
    deleteApi: globalChannelDefinitionDeletePost,
  },
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const openAddGlobalChannelDefinition = () => {
  editGlobalChannelDefinitionTitle.value = t('message.client.addChannel')
  editDialogRef.value.openDialog({})
}

const openEditGlobalChannelDefinition = (row: any) => {
  editGlobalChannelDefinitionTitle.value = t('message.client.editChannel')
  editDialogRef.value.openDialog(row)
}

const channelTypeText = (type: number) => {
  const map: Record<number, string> = {
    1: t('message.client.channelTypeOfficial'),
    2: t('message.client.channelTypeStore'),
    3: t('message.client.channelTypeAd'),
    4: t('message.client.channelTypeCustom'),
  }
  return map[type] || type
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'code', header: t('message.client.channelCode'), width: '140px'},
  {field: 'name', header: t('message.common.label.name'), width: '140px'},
  {field: 'scope', header: t('message.client.channelScope'), width: '120px', bodyKey: 'scope'},
  {field: 'platformCode', header: t('message.client.platform'), width: '120px'},
  {field: 'channelType', header: t('message.client.channelType'), width: '120px', bodyKey: 'channelType'},
  {field: 'isEnabled', header: t('message.client.isEnabled'), width: '100px', bodyKey: 'isEnabled'},
  {field: 'isDefault', header: t('message.client.isDefault'), width: '140px', bodyKey: 'isDefault'},
  {field: 'description', header: t('message.common.label.description'), width: '140px'},
  {field: 'createdName', header: t('message.common.label.creator'), width: '210px'},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '210px'},
])

const actions = {
  edit: openEditGlobalChannelDefinition,
  delete: onDelete,
}
</script>
