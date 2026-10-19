<template>
  <div class="sys-logException-container" v-block-loading="state.loading">
    <SearchForm
      v-model="state.queryParams"
      :fields="searchFields"
      authPrefix="systemLogEx"
      @query="onQuery"
    >
      <template #buttons>
        <Button severity="danger" @click="clearLog" v-auth="'systemLogEx:clear'">
          <i :class="faTrashCan.className" class="mr-1"></i>
          {{ t('system.log.clearLog') }}
        </Button>
        <Button severity="secondary" @click="exportLog" v-auth="'systemLogEx:export'">
          <i :class="faFolderOpen.className" class="mr-1"></i>
          {{ t('system.log.exportLog') }}
        </Button>
      </template>
    </SearchForm>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.logData" @sort="sortChange" showGridlines stripedRows style="width: 100%"
                       :rowClass="rowClass"
                       :totalRecords="state.tableParams.total"
                       :rows="state.tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <Column field="controllerName" :header="t('system.log.moduleName')" style="width: 100px"/>
          <Column field="displayTitle" :header="t('system.log.displayName')" style="width: 150px"/>
          <Column field="actionName" :header="t('system.log.methodName')" style="width: 100px"/>
          <Column field="httpMethod" :header="t('system.log.httpMethod')" style="width: 90px" align="center"/>
          <Column field="requestUrl" :header="t('system.log.requestUrl')" style="width: 300px"/>
          <Column field="logLevel" :header="t('system.log.logLevel')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.logLevel === 1" :value="t('system.log.logLevel_debug')"/>
              <Tag v-else-if="data.logLevel === 2" severity="info" :value="t('system.log.logLevel_info')"/>
              <Tag v-else-if="data.logLevel === 3" severity="warn" :value="t('system.log.logLevel_warn')"/>
              <Tag v-else-if="data.logLevel === 4" severity="danger" :value="t('system.log.logLevel_error')"/>
              <Tag v-else :value="t('common.label.other')"/>
            </template>
          </Column>
          <Column field="eventId" :header="t('system.log.eventId')" style="width: 70px" align="center"/>
          <Column field="threadId" :header="t('system.log.threadId')" sortable style="width: 90px" align="center"/>
          <Column field="traceId" :header="t('system.log.traceId')" sortable style="width: 150px"/>
          <Column field="account" :header="t('system.log.accountName')" style="width: 100px" align="center"/>
          <Column field="realName" :header="t('system.log.realName')" style="width: 100px" align="center"/>
          <Column field="remoteIp" :header="t('system.log.remoteIp')" style="width: 120px" align="center"/>
          <Column field="location" :header="t('system.log.location')" style="width: 150px" align="center"/>
          <Column field="browser" :header="t('system.log.browser')" style="width: 160px" align="center"/>
          <Column field="os" :header="t('system.log.os')" style="width: 120px" align="center"/>
          <Column field="status" :header="t('common.label.status')" style="width: 70px" align="center">
            <template #body="{ data }">
              <Tag v-if="data.status === '200'" severity="success" :value="t('system.log.opSuccess')"/>
              <Tag v-else severity="danger" :value="t('system.log.opFail')"/>
            </template>
          </Column>
          <Column field="elapsed" :header="t('system.log.elapsed')" style="width: 90px" align="center"/>
          <Column field="exception" :header="t('system.log.exceptionMsg')" style="width: 150px"/>
          <TimeColumn field="logDateTime" :header="t('system.log.logDateTime')" width="160px" align="center" />
          <Column :header="t('common.table.action')" style="width: 80px" align="center">
            <template #body="{ data }">
              <Button link @click="viewDetail(data)" v-auth="'systemLogEx:page'">
                <i class="fa-solid fa-circle-info mr-1"></i>
                {{ t('common.button.detail') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>
    <Dialog v-model:visible="state.dialogVisible" draggable modal :style="{ width: 'min(1000px, calc(100vw - 32px))' }"
            :header="t('system.log.logDetail')">
      <pre>{{ state.content }}</pre>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemExLog">
import {faCircleInfo, faFolderOpen, faTrashCan} from '../../../../../utils/icons.ts';

import {onMounted, reactive} from 'vue';
import {showSuccess} from '../../../../../utils/message.ts';
import {downloadByData, getFileName} from '../../../../../utils/download.ts';

import {logExceptionPagePost, logExceptionClearPost, logExceptionExportPost} from '../../../../../api/logException';
import {LogOperationFields} from '../../../../../api/_shared/data-contracts.ts';

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
  tableParams: {
    page: 1,
    pageSize: 10,
    orderBy: 'createdTime',
    isDescending: true,
    total: 0 as any,
  },
  logData: [] as Array<LogOperationFields>,
  dialogVisible: false,
  content: '',
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  if (state.queryParams.startTime == null) state.queryParams.startTime = undefined;
  if (state.queryParams.endTime == null) state.queryParams.endTime = undefined;

  state.loading = true;
  let params = Object.assign(state.queryParams, state.tableParams);
  var res = await logExceptionPagePost(params);
  state.logData = res.data.data?.items ?? [];
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const clearLog = async () => {
  state.loading = true;
  await logExceptionClearPost();
  state.loading = false;
  showSuccess(t('system.log.clearSuccess'));
  onQuery();
};

const exportLog = async () => {
  state.loading = true;
  var res = await logExceptionExportPost(state.queryParams, {responseType: 'blob'});
  state.loading = false;
  var fileName = getFileName(res.headers);
  downloadByData(res.data as any, fileName);
};

const viewDetail = (row: any) => {
  state.content = row.message;
  state.dialogVisible = true;
};

const rowClass = (data: any) => {
  return data.exception != null ? 'bg-orange-50' : '';
};

const sortChange = (event: any) => {
  state.tableParams.orderBy = event.sortField;
  state.tableParams.isDescending = event.sortOrder !== 1;
  onQuery();
};

const onPage = (event: any) => {
  state.tableParams.pageSize = event.rows
  state.tableParams.page = event.page + 1
  onQuery()
}
</script>

<style lang="scss" scoped>
pre {
  white-space: break-spaces;
  line-height: 20px;
}
</style>
