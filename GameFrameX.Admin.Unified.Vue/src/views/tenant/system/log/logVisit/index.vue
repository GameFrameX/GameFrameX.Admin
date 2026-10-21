<template>
  <div class="sys-logVisit-container">
    <SearchForm
      v-model="state.queryParams"
      :fields="searchFields"
      authPrefix="systemLogVis"
      @query="onQuery"
    >
      <template #buttons>
        <Button severity="danger" @click="clearLog" v-auth="'systemLogVis:clear'">
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
          <Column field="displayTitle" :header="t('system.log.displayName')" style="width: 150px" align="center"/>
          <Column field="actionName" :header="t('system.log.methodName')" style="width: 150px"/>
          <Column field="account" :header="t('system.log.accountName')" style="width: 100px" align="center"/>
          <Column field="realName" :header="t('system.log.realName')" style="width: 100px" align="center"/>
          <Column field="remoteIp" :header="t('system.log.remoteIp')" align="center"/>
          <Column field="location" :header="t('system.log.location')" align="center"/>
          <Column field="browser" :header="t('system.log.browser')" align="center"/>
          <Column field="os" :header="t('system.log.os')" style="width: 120px" align="center"/>
          <Column field="status" :header="t('common.label.status')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.status === '200'" severity="success" :value="t('system.log.opSuccess')"/>
              <Tag v-else severity="danger" :value="t('system.log.opFail')"/>
            </template>
          </Column>
          <Column field="elapsed" :header="t('system.log.elapsed')" style="width: 90px" align="center"/>
          <TimeColumn field="logDateTime" :header="t('system.log.logDateTime')" width="160px" align="center" />
        </CrudDataTable>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="systemVisLog">
import {faTrashCan} from '../../../../../utils/icons.ts';

import {onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../../utils/message.ts';

import {logVisitPagePost, logVisitClearPost} from '../../../../../api/logVisit';
import {LogVisitFields} from '../../../../../api/_shared/data-contracts.ts';
import {usePrimeTablePage} from '../../../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';

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
  logData: [] as Array<LogVisitFields>,
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
  if (state.queryParams.startTime == null) state.queryParams.startTime = undefined;
  if (state.queryParams.endTime == null) state.queryParams.endTime = undefined;

  state.loading = true;
  let params = Object.assign(state.queryParams, tableParams.value);
  var res = await logVisitPagePost(params);
  state.logData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const clearLog = async () => {
  // 清空是不可逆的整库删除，必须二次确认（danger 样式与高危删除一致）
  const confirmed = await showConfirm(t('system.log.clearConfirm'), t('system.log.clearLog'), {type: 'danger'});
  if (!confirmed) return;
  state.loading = true;
  await logVisitClearPost();
  state.loading = false;
  showSuccess(t('system.log.clearSuccess'));
  onQuery();
};

const onPage = usePrimeTablePage(tableParams, onQuery)
</script>
