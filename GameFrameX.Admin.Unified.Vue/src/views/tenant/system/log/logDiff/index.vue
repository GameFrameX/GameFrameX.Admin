<template>
  <div class="sys-logDiff-container">
    <SearchForm
      v-model="state.queryParams"
      :fields="searchFields"
      authPrefix="systemLogDiff"
      @query="onQuery"
    >
      <template #buttons>
        <Button severity="danger" @click="clearLog" v-auth="'systemLogDiff:clear'">
          <i :class="faTrashCan.className" class="mr-1"></i>
          {{ t('system.log.clearLog') }}
        </Button>
      </template>
    </SearchForm>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.logData" v-block-loading="state.loading" showGridlines stripedRows
                       style="width: 100%"
                       :totalRecords="tableParams.total"
                       :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <Column field="diffType" :header="t('system.log.differenceTable')"/>
          <Column field="elapsed" :header="t('system.log.elapsed')"/>
          <Column field="message" :header="t('system.log.logDetail')"/>
          <Column field="beforeData" :header="t('system.log.differenceBefore')">
            <template #body="slotProps">
              <DiffFields :raw="slotProps.data.beforeData"/>
            </template>
          </Column>
          <Column field="afterData" :header="t('system.log.differenceAfter')">
            <template #body="slotProps">
              <DiffFields :raw="slotProps.data.afterData"/>
            </template>
          </Column>
          <Column field="snapshot" :header="t('system.log.differenceField')">
            <template #body="slotProps">
              <DiffFields :raw="slotProps.data.snapshot"/>
            </template>
          </Column>
          <Column :header="t('common.table.action')" align="center" style="width: 80px">
            <template #body="slotProps">
              <Button link size="small" @click="openDetail(slotProps.data)">
                {{ t('system.log.logDetail') }}
              </Button>
            </template>
          </Column>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" align="center" />
        </CrudDataTable>
      </template>
    </Card>

    <DetailDrawer v-model="state.detailVisible" :log="state.selectedLog"/>
  </div>
</template>

<script lang="ts" setup name="systemDiffLog">
import {faTrashCan} from '../../../../../utils/icons.ts';

import {onMounted, reactive, ref} from 'vue';
import {showSuccess} from '../../../../../utils/message.ts';

import {logDiffPagePost, logDiffClearPost} from '../../../../../api/logDiff';
import {LogDiffFields} from '../../../../../api/_shared/data-contracts.ts';
import {usePrimeTablePage} from '../../../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';
import DiffFields from './component/DiffFields.vue';
import DetailDrawer from './component/DetailDrawer.vue';

const searchFields = [
  { key: 'startTime', label: t('system.log.startTime'), type: 'date', props: { showTime: true, dateFormat: 'yy-mm-dd' } },
  { key: 'endTime', label: t('system.log.endTime'), type: 'date', props: { showTime: true, dateFormat: 'yy-mm-dd' } },
];

const state = reactive({
  loading: false,
  queryParams: {
    searchKey: undefined as string | undefined,
    startTime: undefined as string | undefined,
    endTime: undefined as string | undefined,
  },
  logData: [] as Array<LogDiffFields>,
  detailVisible: false,
  selectedLog: null as LogDiffFields | null,
});

const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0 as any,
});

const openDetail = (row: LogDiffFields) => {
  state.selectedLog = row;
  state.detailVisible = true;
};

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  if (state.queryParams.startTime == null) state.queryParams.startTime = undefined;
  if (state.queryParams.endTime == null) state.queryParams.endTime = undefined;

  state.loading = true;
  let params = Object.assign(state.queryParams, tableParams.value);
  var res = await logDiffPagePost(params);
  state.logData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const clearLog = async () => {
  state.loading = true;
  await logDiffClearPost();
  state.loading = false;
  showSuccess(t('system.log.clearSuccess'));
  onQuery();
};

const onPage = usePrimeTablePage(tableParams, onQuery)
</script>
