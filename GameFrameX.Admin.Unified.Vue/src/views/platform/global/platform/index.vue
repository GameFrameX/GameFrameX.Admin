<template>
  <div class="appplatform-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="globalPlatform"
            @query="onQuery"
            @reset="onReset"
            @add="openAddGlobalPlatform"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <Table ref="tableRef" v-bind="tb.tableData" :getData="getData" @sortHeader="onSortHeader" border>
          <template #isDefault="scope">
            <template v-if="getScopeRow(scope)">
              <Tag v-if="getScopeRow(scope).isDefault" :value="t('message.common.label.yes')"/>
              <Tag v-else :value="t('message.common.label.no')" severity="info"/>
            </template>
          </template>
          <template #action="scope">
            <Button link @click="openEditGlobalPlatform(getScopeRow(scope))" v-auth="'globalPlatform:update'">
              <i class="fa-solid fa-pen-to-square mr-1"></i>
              {{ t('message.common.button.edit') }}
            </Button>
            <Button link severity="danger" @click="deletePlatform(getScopeRow(scope))" v-auth="'globalPlatform:delete'">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('message.common.button.delete') }}
            </Button>
          </template>
        </Table>
        <editDialog ref="editDialogRef" :title="editGlobalPlatformTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="appplatform">
import {ref, computed, reactive, defineAsyncComponent, nextTick} from 'vue'
import editDialog from '/@/views/platform/global/platform/component/EditDialog.vue'
import { globalPlatformPagePost, globalPlatformDeletePost } from '../../../../api/globalPlatform'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'

// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Table = defineAsyncComponent(() => import('/@/components/simple-table/index.vue'))
const tableRef = ref<RefType>()
const editDialogRef = ref()
const editGlobalPlatformTitle = ref('')

const queryParams = ref({})
const searchFields = computed(() => [])

const tb = reactive<TableDemoState>({
  tableData: {
    columns: [
      {
        prop: 'name',
        width: 300,
        label: t('message.common.label.name'),
        headerAlign: 'center',
        sortable: true,
        isCheck: true,
        hideCheck: true,
      },
      {
        prop: 'isDefault',
        width: 140,
        label: t('message.client.isDefault'),
        align: 'center',
        headerAlign: 'center',
        isCheck: true,
      },
      {
        prop: 'description',
        width: 180,
        label: t('message.common.label.description'),
        headerAlign: 'center',
        showOverflowTooltip: true,
        isCheck: true,
      },
      {
        prop: 'createdName',
        width: 210,
        label: t('message.common.label.creator'),
        headerAlign: 'center',
        isCheck: true,
      },
      {
        prop: 'updatedName',
        width: 210,
        label: t('message.common.label.modifier'),
        headerAlign: 'center',
        isCheck: true,
      },
      {
        prop: 'action',
        width: 140,
        label: t('message.common.table.action'),
        type: 'action',
        align: 'center',
        fixed: 'right',
        isCheck: true,
        hideCheck: true,
      },
    ],
    config: {
      isStripe: true,
      isBorder: true,
      isSerialNo: true,
      isSelection: false,
      showSelection: false,
      pageSize: 10,
      pageSizes: [10, 20, 50, 100, 200, 500],
      hideExport: true,
    },
    param: {},
    defaultSort: {},
  },
})

const getData = (param: any) => {
  return globalPlatformPagePost(param).then((res) => res.data)
}

const onSortHeader = (data: object[]) => {
  tb.tableData.columns = data
}

const getScopeRow = (scope: any) => scope?.row ?? scope?.data

const onQuery = () => {
  tb.tableData.param = Object.assign({}, tb.tableData.param, {...queryParams.value})
  nextTick(() => {
    tableRef.value?.pageReset()
  })
}

const onReset = () => {
  queryParams.value = {}
  tb.tableData.param = {}
  nextTick(() => {
    tableRef.value?.pageReset()
  })
}

// 打开新增页面
const openAddGlobalPlatform = () => {
  editGlobalPlatformTitle.value = t('message.client.addPlatform')
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditGlobalPlatform = (row: any) => {
  editGlobalPlatformTitle.value = t('message.client.editPlatform')
  editDialogRef.value.openDialog(row)
}

const deletePlatform = async (row: any) => {
  const confirmed = await showConfirm(`${t('message.common.message.deleteConfirm')}：【${row.name}】?`, t('message.common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await globalPlatformDeletePost(row)
  tableRef.value?.onList()
  showSuccess(t('message.common.message.deleteSuccess'))
}
</script>
