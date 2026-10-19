<template>
  <div class="tenantGameAppAssetPackageVersionHistory-container">
    <Dialog v-model:visible="dialogVisible" modal :style="{ width: '75%' }" draggable
            :header="`${queryParams.assetPackageName} ${t('message.client.updateHistory')}`">
      <Card style="margin-top: 8px">
        <template #content>
          <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                         :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                         :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                         @page="onPage">
            <TimeColumn field="createdTime" :header="t('common.label.createTime')" width="240px" />
            <Column field="oldVersion" :header="t('message.client.oldAssetVersion')" style="min-width: 160px"/>
            <Column field="resourceVersion" :header="t('message.client.newAssetVersion')" style="min-width: 160px"/>
            <Column :header="t('message.common.table.action')" style="width: 140px" align="center"
                    v-if="auth('tenantGameAppAssetPackageVersion:restore')">
              <template #body="{ data }">
                <Button link @click="restoreTenantGameAppAssetPackageVersionHistory(data)"
                        v-auth="'tenantGameAppAssetPackageVersion:restore'">
                  <i :class="faPenToSquare.className" class="mr-1"></i>{{ t('message.client.restore') }}
                </Button>
              </template>
            </Column>
          </CrudDataTable>
        </template>
      </Card>
      <template #footer>
        <Button @click="cancel">{{ t('message.client.close') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup="" name="tenantGameAppAssetPackageVersionHistory">
import { faPenToSquare } from '../../../../../../utils/icons.ts';
import {ref} from "vue";
import {showConfirm, showSuccess} from '../../../../../../utils/message.ts';
import {auth} from '../../../../../../utils/authFunction.ts';
import CrudDataTable from '/@/components/crud-data-table/index.vue'

import { tenantGameAppAssetPackageVersionHistoryPagePost } from '../../../../../../api/tenantGameAppAssetPackageVersionHistory';
import { tenantGameAppAssetPackageVersionUpdatePost, tenantGameAppAssetPackageVersionReturnPost } from "../../../../../../api/tenantGameAppAssetPackageVersion";
import {usePrimeTablePage} from '../../../../../../composables';

const emit = defineEmits(["reloadTable"]);
const dialogVisible = ref(false);
const loading = ref(false);
const tableData = ref<any>([]);
const queryParams = ref<any>({assetPackageName: ''});
const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0,
});
const returnVersionPackage = ref<any>({})
// 打开弹窗
const openDialog = (row: any) => {
  queryParams.value = JSON.parse(JSON.stringify(row));
  dialogVisible.value = true;
  onQuery();
};
// 关闭弹窗
const closeDialog = () => {
  dialogVisible.value = false;
};

// 取消
const cancel = () => {
  dialogVisible.value = false;
};
// 查询操作
const onQuery = async () => {
  loading.value = true;
  let res = await tenantGameAppAssetPackageVersionHistoryPagePost(Object.assign({versionId: queryParams.value.id}, tableParams.value));
  tableData.value = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  loading.value = false;
};

const onPage = usePrimeTablePage(tableParams, onQuery)

// 还原
const restoreTenantGameAppAssetPackageVersionHistory = async (row: any) => {
  const confirmed = await showConfirm(t('message.client.confirmRestore'), t('message.common.message.tip'))
  if (!confirmed) return
  let values = returnVersionPackage.value;
  values.id = row.versionId;
  values.resourceVersionHistoryId = row.id
  await tenantGameAppAssetPackageVersionReturnPost(values);
  setTimeout(() => {
    onQuery();
  }, 1000);
  showSuccess(t('message.client.restoreSuccess'));
  closeDialog();
};

// 改变页面容量
const onSizeChange = (val: number) => {
  tableParams.value.pageSize = val;
  onQuery();
};

// 改变页码序号
const onCurrentChange = (val: number) => {
  tableParams.value.page = val;
  onQuery();
};

onQuery();

//将属性或者函数暴露给父组件
defineExpose({openDialog});
</script>
