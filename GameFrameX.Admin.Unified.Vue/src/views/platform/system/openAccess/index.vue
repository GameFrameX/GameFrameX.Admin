<template>
  <div class="sys-open-access-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="state.queryParams"
            :fields="searchFields"
            :showAdd="true"
            :defaultSearchKey="false"
            authPrefix="systemOpenAccess"
            @query="onQuery"
            @reset="resetQuery"
            @add="openAddOpenAccess"
        >
          <template #buttons>
            <Button severity="secondary" @click="openHelp" class="search-form-button">
              <i class="fa-solid fa-circle-question mr-1"></i>
              {{ t('system.openAccess.helpView') }}
            </Button>
          </template>
        </SearchForm>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.openAccessData" v-block-loading="state.loading" showGridlines stripedRows style="width: 100%"
                       :totalRecords="tableParams.total"
                       :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="systemOpenAccess"
                       :actions="actions"
                       @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <Column field="accessKey" :header="t('system.openAccess.accessName')"/>
          <Column field="accessSecret" :header="t('message.system.openAccess.accessSecret')"/>
          <Column field="bindUserAccount" :header="t('message.system.openAccess.bindUserAccount')"/>
          <Column field="bindTenantName" :header="t('message.system.openAccess.bindTenantName')"/>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" align="center" />
        </CrudDataTable>
      </template>
    </Card>

    <EditOpenAccess ref="editOpenAccessRef" :title="state.editOpenAccessTitle" @query="onQuery"/>
    <HelpView ref="helpViewRef"/>
  </div>
</template>

<script lang="ts" setup name="systemOpenAccess">
import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import EditOpenAccess from '/@/views/platform/system/openAccess/component/EditOpenAccess.vue';
import HelpView from '/@/views/platform/system/openAccess/component/HelpView.vue';

import {systemOpenAccessPagePost, systemOpenAccessDeletePost} from '../../../../api/systemOpenAccess';
import {OpenAccessOutput} from '../../../../api/_shared/data-contracts.ts';
import {usePrimeTablePage} from '../../../../composables';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';

const editOpenAccessRef = ref<InstanceType<typeof EditOpenAccess>>();
const helpViewRef = ref<InstanceType<typeof HelpView>>();
const searchFields = computed(() => [
  { key: 'accessKey', label: t('system.openAccess.accessName'), type: 'input', placeholder: t('system.openAccess.accessName') },
])
const state = reactive({
  loading: false,
  openAccessData: [] as Array<OpenAccessOutput>,
  queryParams: {
    accessKey: undefined,
  },
  editOpenAccessTitle: '',
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
  var res = await systemOpenAccessPagePost(params);
  state.openAccessData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.accessKey = undefined;
  onQuery();
};

const openAddOpenAccess = () => {
  state.editOpenAccessTitle = '添加开放接口身份';
  editOpenAccessRef.value?.openDialog({type: 1});
};

const openEditOpenAccess = (row: any) => {
  state.editOpenAccessTitle = '编辑开放接口身份';
  editOpenAccessRef.value?.openDialog(row);
};

const delOpenAccess = async (row: any) => {
  if (!await showConfirm(`确定删除开放接口身份：【${row.accessKey}】?`, t('common.message.tip'), {type: 'danger'})) return;
  await systemOpenAccessDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const onPage = usePrimeTablePage(tableParams, onQuery)

const openHelp = () => {
  helpViewRef.value?.openDialog();
};

const actions = {
  edit: openEditOpenAccess,
  delete: delOpenAccess,
}
</script>
