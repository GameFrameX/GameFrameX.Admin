<template>
  <div class="sys-jobCluster-container">
    <Drawer v-model:visible="state.isVisible" :header="t('system.job.jobCluster')" :style="{ width: '40%' }">
      <CrudDataTable :value="state.jobClusterList" v-block-loading="state.loading" showGridlines stripedRows
                 style="width: 100%; margin: 8px" :showPagination="false">
        <Column field="clusterId" :header="t('message.system.job.clusterId')"/>
        <Column field="status" :header="t('common.label.status')" align="center">
          <template #body="{ data }">
            <Tag v-if="data.status == 0" :value="t('system.job.blocked')"/>
            <Tag v-if="data.status == 1" :value="t('system.job.running')"/>
            <Tag v-if="data.status == 2" :value="t('system.job.paused')"/>
          </template>
        </Column>
        <Column field="description" :header="t('common.label.description')"/>
        <TimeColumn field="updatedTime" :header="t('message.common.label.updateTime')" align="center" />
      </CrudDataTable>
    </Drawer>
  </div>
</template>

<script lang="ts" setup name="systemJobCluster">

import {onMounted, reactive} from 'vue';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import {systemJobJobClusterListGet} from '../../../../../api/systemJob';
import {SystemJobClusterEntity} from '../../../../../api/_shared/data-contracts.ts';

const state = reactive({
  loading: false,
  isVisible: false,
  jobClusterList: [] as Array<SystemJobClusterEntity>,
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  var res = await systemJobJobClusterListGet();
  state.jobClusterList = res.data.data ?? [];
  state.loading = false;
};

const openDrawer = () => {
  state.isVisible = true;
};

defineExpose({openDrawer});
</script>
