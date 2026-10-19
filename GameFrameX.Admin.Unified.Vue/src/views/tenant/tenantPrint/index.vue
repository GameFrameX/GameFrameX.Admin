<template>
  <div class="sys-print-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="state.queryParams"
            :fields="searchFields"
            :showAdd="true"
            :defaultSearchKey="false"
            authPrefix="systemPrint"
            @query="onQuery"
            @reset="resetQuery"
            @add="openAddPrint"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.printData" v-block-loading="state.loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total"
                       :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="systemPrint"
                       :actions="actions"
                       @page="onPage">
          <Column field="name" :header="t('common.label.name')"/>
          <Column field="orderNo" :header="t('common.label.sort')" align="center"/>
          <Column :header="t('common.label.status')" align="center">
            <template #body="{ data }">
              <Tag v-if="isEnabled(data)" severity="success" :value="t('common.label.enable')"/>
              <Tag v-else severity="danger" :value="t('common.label.disable')"/>
            </template>
          </Column>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" align="center" />
          <Column field="remark" :header="t('common.label.remark')"/>
        </CrudDataTable>
      </template>
    </Card>

    <EditPrint ref="editPrintRef" :title="state.editPrintTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemPrint">
import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../utils/message';
import EditPrint from '/@/views/tenant/tenantPrint/component/EditPrint.vue';

import {tenantPrintPagePost, tenantPrintDeletePost} from '../../../api/tenantPrint';
import {TenantPrintEntity} from '../../../api/_shared/data-contracts';
import {isEnabled} from '../../../utils/isEnabled';
import {usePrimeTablePage} from '../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';

const editPrintRef = ref<InstanceType<typeof EditPrint>>();
const searchFields = computed(() => [
  { key: 'name', label: t('system.print.printName'), type: 'input', placeholder: t('system.print.printName') },
])
const state = reactive({
  loading: false,
  printData: [] as Array<TenantPrintEntity>,
  queryParams: {
    name: undefined,
  },
  editPrintTitle: '',
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
  let params = Object.assign(state.queryParams, tableParams.value);
  var res = await tenantPrintPagePost(params);
  state.printData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.name = undefined;
  onQuery();
};

const openAddPrint = () => {
  state.editPrintTitle = t('system.print.addPrint');
  editPrintRef.value?.openDialog({});
};

const openEditPrint = (row: any) => {
  state.editPrintTitle = t('system.print.editPrint');
  editPrintRef.value?.openDialog(row);
};

const delPrint = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.print.deletePrintConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantPrintDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPage = usePrimeTablePage(tableParams, onQuery)

const actions = {
  edit: openEditPrint,
  delete: delPrint,
}
</script>
