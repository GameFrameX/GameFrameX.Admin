<template>
  <div class="notice-container">
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <div class="flex flex-column gap-1">
            <label>{{ t('message.home.title') }}</label>
            <InputText v-model="state.queryParams.title" :placeholder="t('message.home.title')" />
          </div>
          <div class="flex flex-column gap-1">
            <label>{{ t('message.home.type') }}</label>
            <Select v-model="state.queryParams.type" :placeholder="t('message.home.type')" filter showClear :options="typeOptions" optionLabel="label" optionValue="value" />
          </div>
          <div class="flex gap-2">
            <Button @click="onQuery">
              <i :class="faMagnifyingGlass" class="mr-1"></i>
              {{ t('message.home.query') }}
            </Button>
            <Button severity="secondary" @click="resetQuery">
              <i :class="faArrowsRotate" class="mr-1"></i>
              {{ t('message.home.reset') }}
            </Button>
          </div>
        </div>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.noticeData" style="width: 100%" v-block-loading="state.loading" :rowClass="tableRowClassName"
            :totalRecords="state.tableParams.total" :rows="state.tableParams.pageSize"
            :rowsPerPageOptions="[10, 20, 50, 100]"
            :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
            @page="onPageChange">
          <Column field="systemNotice.title" :header="t('message.home.title')" />
          <Column field="systemNotice.content" :header="t('message.home.content')">
            <template #body="{ data }"> {{ removeHtml(data.systemNotice.content) }}</template>
          </Column>
          <Column field="systemNotice.type" :header="t('message.home.type')" style="min-width: 100px">
            <template #body="{ data }">
              <Tag v-if="data.systemNotice.type === 1" severity="info"> {{ t('message.home.notice') }}</Tag>
              <Tag v-else severity="warn"> {{ t('message.home.announcement') }}</Tag>
            </template>
          </Column>
          <Column field="systemNotice.createdTime" :header="t('message.home.createdTime')" >
            <template #body="{ data }">
              {{ formatUnix(data.systemNotice.createdTime) }}
            </template>
          </Column>
          <Column field="readStatus" :header="t('message.home.readStatus')" style="min-width: 100px">
            <template #body="{ data }">
              <Tag v-if="data.readStatus === 1" severity="info"> {{ t('message.home.read') }}</Tag>
              <Tag v-else severity="danger"> {{ t('message.home.unread') }}</Tag>
            </template>
          </Column>
          <Column field="systemNotice.publicUserName" :header="t('message.home.publisher')" />
          <Column field="systemNotice.publicTime" :header="t('message.home.publishTime')" >
            <template #body="{ data }">
              {{ formatUnix(data.systemNotice.publicTime) }}
            </template>
          </Column>
          <Column :header="t('message.home.operation')" style="min-width: 80px">
            <template #body="{ data }">
              <Button size="small" link @click="viewDetail(data)">
                <i :class="faCircleInfo" class="mr-1"></i>
                {{ t('message.home.detail') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>
    <Dialog v-model:visible="state.dialogVisible" :header="t('message.home.messageDetail')" modal style="width: min(769px, calc(100vw - 32px))">
      <p v-html="sanitizeHtml(state.content)"></p>
      <template #footer>
        <Button @click="state.dialogVisible = false">{{ t('message.home.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="notice">
import { faArrowsRotate, faCircleInfo, faMagnifyingGlass } from '../../../../utils/icons.ts';
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {onMounted, reactive, computed} from 'vue';
import commonFunction from '../../../../utils/commonFunction.ts';
import {sanitizeHtml} from '../../../../utils/sanitize.ts';

import {tenantNoticePageReceivedGet, tenantNoticeSetReadPost} from '../../../../api/tenantNotice';
import {TenantNoticeUserEntity} from '../../../../api/_shared/data-contracts.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Dialog from 'primevue/dialog';
import {formatUnix} from '../../../../utils/formatTime.ts'

const {removeHtml} = commonFunction();

const typeOptions = computed(() => [
  { label: t('message.home.notice'), value: 1 },
  { label: t('message.home.announcement'), value: 2 },
]);

const state = reactive({
  loading: false,
  noticeData: [] as Array<TenantNoticeUserEntity>,
  queryParams: {
    title: undefined as string | undefined,
    type: undefined as number | undefined,
  },
  tableParams: {
    page: 1,
    pageSize: 10,
    total: 0 as any,
  },
  editNoticeTitle: '',
  dialogVisible: false,
  content: '',
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  var res = await tenantNoticePageReceivedGet({title: state.queryParams.title, type: state.queryParams.type, page: state.tableParams.page, pageSize: state.tableParams.pageSize});
  state.noticeData = res.data.data?.items ?? [];
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.title = undefined;
  state.queryParams.type = undefined;
  onQuery();
};

const onPageChange = (event: any) => {
  state.tableParams.pageSize = event.rows;
  state.tableParams.page = event.page + 1;
  onQuery();
};

const viewDetail = async (row: any) => {
  state.content = row.systemNotice.content;
  state.dialogVisible = true;

  row.readStatus = 1;
  await tenantNoticeSetReadPost({id: row.systemNotice.id});
};

const tableRowClassName = (data: TenantNoticeUserEntity) => {
  return data.readStatus === 1 ? 'info-row' : '';
};
</script>

<style lang="scss">
</style>
