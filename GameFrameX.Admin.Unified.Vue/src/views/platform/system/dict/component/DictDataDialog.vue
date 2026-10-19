<template>
  <div class="sys-dictData-container">
    <Dialog v-model:visible="state.dialogVisible" draggable modal :style="{ width: 'min(980px, calc(100vw - 32px))' }"
            :header="t('system.dict.dictValueList')">
      <Card style="margin-bottom: 8px">
        <template #content>
          <form @submit.prevent class="flex flex-wrap gap-2 align-items-end">
            <div class="field">
              <label>{{ t('system.dict.dictValue') }}</label>
              <InputText v-model="state.queryParams.value" :placeholder="t('system.dict.dictValue')" class="w-full"/>
            </div>
            <div class="field">
              <label>{{ t('common.label.code') }}</label>
              <InputText v-model="state.queryParams.code" :placeholder="t('common.label.code')" class="w-full"/>
            </div>
            <div class="flex gap-2">
              <Button @click="onQuery">
                <i class="fa-solid fa-magnifying-glass mr-1"></i>
                {{ t('common.button.search') }}
              </Button>
              <Button severity="secondary" @click="resetQuery">
                <i class="fa-solid fa-arrows-rotate mr-1"></i>
                {{ t('common.button.reset') }}
              </Button>
            </div>
            <Button @click="openAddDictData">
              <i class="fa-solid fa-plus mr-1"></i>
              {{ t('common.button.add') }}
            </Button>
          </form>
        </template>
      </Card>

      <CrudDataTable :value="state.dictDataData" v-block-loading="state.loading" showGridlines stripedRows
                     style="width: 100%"
                     :totalRecords="tableParams.total"
                     :rows="tableParams.pageSize"
                     :rowsPerPageOptions="[10, 20, 50, 100]"
                     @page="onPage">
        <Column field="value" :header="t('system.dict.dictValue')" style="width: 140px">
          <template #body="{ data }">
            <Tag :severity="tagSeverityMap[data.tagType]" :style="data.styleSetting" :class="data.classSetting">
              {{ data.value }}
            </Tag>
          </template>
        </Column>
        <Column field="code" :header="t('common.label.code')" style="width: 90px"/>
        <Column field="isEnabled" :header="t('common.label.status')" style="width: 70px" align="center">
          <template #body="{ data }">
            <Tag v-if="isEnabled(data)" severity="success" :value="t('common.label.enable')"/>
            <Tag v-else severity="danger" :value="t('common.label.disable')"/>
          </template>
        </Column>
        <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" align="center"/>
        <TimeColumn field="createdTime" :header="t('common.label.createTime')" width="120px" align="center" />
        <Column field="remark" :header="t('common.label.remark')"/>
        <Column field="extData" :header="t('system.dict.extData')" style="width: 90px">
          <template #body="{ data }">
            <Tag v-if="data.extData == null || data.extData == ''" severity="warn"
                 :value="t('system.dict.extDataEmpty')"/>
            <Tag v-else severity="success" :value="t('system.dict.extDataHasValue')"/>
          </template>
        </Column>
        <Column :header="t('common.table.action')" style="width: 140px" align="center">
          <template #body="{ data }">
            <Button link @click="openEditDictData(data)">
              <i class="fa-solid fa-pen-to-square mr-1"></i>
              {{ t('common.button.edit') }}
            </Button>
            <Button link severity="danger" @click="delDictData(data)">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('common.button.delete') }}
            </Button>
          </template>
        </Column>
      </CrudDataTable>
    </Dialog>

    <EditDictData ref="editDictDataRef" :title="state.editDictDataTitle" :dictTypeId="state.queryParams.dictTypeId"
                  @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemDictData">
import {onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../../utils/message.ts';
import EditDictData from '/@/views/platform/system/dict/component/EditDictData.vue';

import {globalDictionaryDataPagePost, globalDictionaryDataDeletePost} from '../../../../../api/globalDictionaryData';
import {GlobalDictionaryDataEntity} from '../../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../../utils/isEnabled.ts';
import {usePrimeTablePage} from '../../../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';

const tagSeverityMap: Record<string, string | undefined> = {
  success: 'success',
  info: 'info',
  warning: 'warn',
  danger: 'danger',
  default: undefined,
};

const editDictDataRef = ref();
const state = reactive({
  dialogVisible: false,
  loading: false,
  dictDataData: [] as Array<GlobalDictionaryDataEntity>,
  queryParams: {
    value: undefined as string | undefined,
    code: undefined as string | undefined,
    dictTypeId: 0,
  },
  editDictDataTitle: '',
});

const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0 as any,
});

onMounted(async () => {
  onQuery();
});

const openDialog = async (row: any) => {
  state.queryParams.dictTypeId = row.id;
  onQuery();
  state.dialogVisible = true;
};

const onQuery = async () => {
  state.loading = true;
  let params = Object.assign(state.queryParams, tableParams.value);
  var res = await globalDictionaryDataPagePost(params);
  state.dictDataData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.value = undefined;
  state.queryParams.code = undefined;
  onQuery();
};

const openAddDictData = () => {
  state.editDictDataTitle = t('system.dict.addDictData');
  editDictDataRef.value.openDialog({isEnabled: true, orderNo: 100, dictTypeId: state.queryParams.dictTypeId});
};

const openEditDictData = (row: any) => {
  state.editDictDataTitle = t('system.dict.editDictData');
  editDictDataRef.value.openDialog(row);
};

const delDictData = async (row: any) => {
  if (!await showConfirm(`${t('system.dict.deleteDictDataConfirm')}：【${row.value}】?`, t('common.message.tip'), {type: 'danger'})) return;
  await globalDictionaryDataDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPage = usePrimeTablePage(tableParams, onQuery)

defineExpose({openDialog});
</script>
