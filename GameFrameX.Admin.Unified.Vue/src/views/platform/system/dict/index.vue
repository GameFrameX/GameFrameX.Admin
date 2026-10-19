<template>
  <div class="sys-dict-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="state.queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            :showAdd="true"
            authPrefix="systemDictType"
            @query="onQuery"
            @reset="resetQuery"
            @add="openAddDictType"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.dictTypeData" v-block-loading="state.loading" showGridlines stripedRows
                       style="width: 100%"
                       :totalRecords="tableParams.total"
                       :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       @page="onPage">
          <Column field="name" :header="t('system.dict.dictName')"/>
          <Column field="code" :header="t('system.dict.dictCode')"/>
          <Column field="isEnabled" :header="t('common.label.status')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="isEnabled(data)" severity="success" :value="t('common.label.enable')"/>
              <Tag v-else severity="danger" :value="t('common.label.disable')"/>
            </template>
          </Column>
          <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" align="center"/>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" align="center" />
          <Column field="remark" :header="t('common.label.remark')"/>
          <Column :header="t('common.table.action')" style="width: 200px" align="center">
            <template #body="{ data }">
              <Button link @click="openEditDictType(data)" v-auth="'systemDictType:update'">
                <i class="fa-solid fa-pen-to-square mr-1"></i>
                {{ t('common.button.edit') }}
              </Button>
              <Button link @click="openDictDataDialog(data)" v-auth="'systemDictType:page'">
                <i class="fa-solid fa-note-sticky mr-1"></i>
                {{ t('system.dict.dictValueList') }}
              </Button>
              <Button link severity="danger" @click="delDictType(data)" v-auth="'systemDictType:delete'">
                <i class="fa-solid fa-trash mr-1"></i>
                {{ t('common.button.delete') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>

    <EditDictType ref="editDictTypeRef" :title="state.editDictTypeTitle" @query="onQuery"/>
    <DictDataDialog ref="dictDataDialogRef"/>
  </div>
</template>

<script lang="ts" setup name="systemDict">
import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import EditDictType from '/@/views/platform/system/dict/component/EditDictType.vue';
import DictDataDialog from '/@/views/platform/system/dict/component/DictDataDialog.vue';

import {globalDictionaryTypePagePost, globalDictionaryTypeDeletePost} from '../../../../api/globalDictionaryType';
import {GlobalDictionaryTypeEntity} from '../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../utils/isEnabled.ts';
import {usePrimeTablePage} from '../../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';

const editDictTypeRef = ref<InstanceType<typeof EditDictType>>();
const dictDataDialogRef = ref<InstanceType<typeof DictDataDialog>>();
const searchFields = computed(() => [
  {key: 'name', label: t('system.dict.dictName'), type: 'input' as const, placeholder: t('system.dict.dictName')},
  {key: 'code', label: t('system.dict.dictCode'), type: 'input' as const, placeholder: t('system.dict.dictCode')},
]);

const state = reactive({
  loading: false,
  dictTypeData: [] as Array<GlobalDictionaryTypeEntity>,
  queryParams: {
    name: undefined as string | undefined,
    code: undefined as string | undefined,
  },
  editDictTypeTitle: '',
});

const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0 as any,
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  try {
    let params = Object.assign({}, state.queryParams, tableParams.value);
    var res = await globalDictionaryTypePagePost(params);
    state.dictTypeData = res.data.data?.items ?? [];
    tableParams.value.total = res.data.data?.total;
  } finally {
    state.loading = false;
  }
};

const resetQuery = () => {
  tableParams.value.page = 1;
};

const openAddDictType = () => {
  state.editDictTypeTitle = t('system.dict.addDict');
  editDictTypeRef.value?.openDialog({isEnabled: true, orderNo: 100});
};

const openEditDictType = (row: any) => {
  state.editDictTypeTitle = t('system.dict.editDict');
  editDictTypeRef.value?.openDialog(row);
};

const openDictDataDialog = (row: any) => {
  dictDataDialogRef.value?.openDialog(row);
};

const delDictType = async (row: any) => {
  if (!await showConfirm(`${t('system.dict.deleteDictConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})) return;
  await globalDictionaryTypeDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPage = usePrimeTablePage(tableParams, onQuery)
</script>
