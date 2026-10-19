<template>
  <div class="sys-region-container">
    <div class="grid" style="gap: 8px; width: 100%">
      <div class="col-3">
        <RegionTree ref="regionTreeRef" @nodeSelect="nodeClick"/>
      </div>

      <div class="col-9">
        <Card>
          <template #content>
            <div class="flex flex-wrap gap-3 align-items-end">
              <div class="flex flex-column gap-1">
                <label>{{ t('system.region.regionName') }}</label>
                <InputText v-model="state.queryParams.name" :placeholder="t('system.region.regionName')" />
              </div>
              <div class="flex flex-column gap-1">
                <label>{{ t('system.region.regionCode') }}</label>
                <InputText v-model="state.queryParams.code" :placeholder="t('system.region.regionCode')" />
              </div>
              <div class="flex gap-2">
                <Button v-auth="'systemRegion:page'" @click="onQuery">
                  <i :class="faMagnifyingGlass" class="mr-1"></i>
                  {{ t('common.button.search') }}
                </Button>
                <Button severity="secondary" @click="resetQuery">
                  <i :class="faArrowsRotate" class="mr-1"></i>
                  {{ t('common.button.reset') }}
                </Button>
              </div>
              <div class="flex gap-2">
                <Button v-auth="'systemRegion:add'" @click="openAddRegion">
                  <i :class="faPlus" class="mr-1"></i>
                  {{ t('common.button.add') }}
                </Button>
                <Button severity="danger" v-auth="'systemRegion:sync'" @click="handlSync">
                  <i :class="faBolt" class="mr-1"></i>
                  {{ t('system.region.sync') }}
                </Button>
              </div>
            </div>
          </template>
        </Card>

        <Card style="margin-top: 8px">
          <template #content>
            <CrudDataTable :value="state.regionData" style="width: 100%" v-block-loading="state.loading" showGridlines rowKey="id"
                           :totalRecords="state.tableParams.total"
                           :rows="state.tableParams.pageSize"
                           :rowsPerPageOptions="[10, 20, 50, 100]"
                           :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
                           authPrefix="systemRegion"
                           :actions="actions"
                           @page="onPageChange">
              <Column field="name" :header="t('system.region.regionName')" />
              <Column field="code" :header="t('system.region.regionCode')" />
              <Column field="cityCode" :header="t('system.region.cityCode')" />
              <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
              <Column field="remark" :header="t('common.label.remark')" />
            </CrudDataTable>
          </template>
        </Card>
      </div>
    </div>

    <EditRegion ref="editRegionRef" :title="state.editRegionTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemRegion">
import { faArrowsRotate, faBolt, faMagnifyingGlass, faPlus } from '../../../../utils/icons.ts';

import {onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess, showMessage} from '../../../../utils/message.ts';
import RegionTree from '/@/views/platform/system/region/component/RegionTree.vue';
import EditRegion from '/@/views/platform/system/region/component/EditRegion.vue';

import {systemRegionPagePost, systemRegionDeletePost, systemRegionSyncPost} from '../../../../api/systemRegion';
import {SystemRegionEntity} from '../../../../api/_shared/data-contracts.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Column from 'primevue/column';
import CrudDataTable from '/@/components/crud-data-table/index.vue';

const editRegionRef = ref<InstanceType<typeof EditRegion>>();
const regionTreeRef = ref<InstanceType<typeof RegionTree>>();
const state = reactive({
  loading: false,
  regionData: [] as Array<SystemRegionEntity>,
  queryParams: {
    id: -1,
    pid: undefined,
    name: undefined,
    code: undefined,
  },
  tableParams: {
    page: 1,
    pageSize: 10,
    total: 0 as any,
  },
  editRegionTitle: '',
});

onMounted(() => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  let params = Object.assign(state.queryParams, state.tableParams);
  var res = await systemRegionPagePost(params);
  state.regionData = res.data.data?.items ?? [];
  state.tableParams.total = res.data.data?.total;
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.id = -1;
  state.queryParams.pid = undefined;
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  onQuery();
};

const openAddRegion = () => {
  state.editRegionTitle = t('system.region.addRegion');
  editRegionRef.value?.openDialog({orderNo: 100});
};

const openEditRegion = (row: any) => {
  state.editRegionTitle = t('system.region.editRegion');
  editRegionRef.value?.openDialog(row);
};

const delRegion = async (row: any) => {
  if (!await showConfirm(`${t('system.region.deleteRegionConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})) return;
  await systemRegionDeletePost({id: row.id});
  onQuery();
  regionTreeRef.value?.initTreeData();
  showSuccess(t('common.message.deleteSuccess'));
};

const nodeClick = async (node: any) => {
  state.queryParams.pid = node.id;
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  onQuery();
};

const handlSync = async () => {
  if (!await showConfirm(t('system.region.syncConfirm'), t('common.message.tip'))) return;
  showMessage(t('system.region.syncing'), 'success');
  await systemRegionSyncPost({timeout: 1000 * 60 * 30});
};

const onPageChange = (event: any) => {
  state.tableParams.pageSize = event.rows;
  state.tableParams.page = Math.floor(event.first / event.rows) + 1;
  onQuery();
};

const actions = {
  edit: openEditRegion,
  delete: delRegion,
}
</script>
