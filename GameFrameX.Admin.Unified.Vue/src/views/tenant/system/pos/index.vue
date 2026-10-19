<template>
  <div class="sys-pos-container">
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <div class="flex flex-column gap-1">
            <label>{{ t('system.pos.posName') }}</label>
            <InputText v-model="state.queryParams.name" :placeholder="t('system.pos.posName')" />
          </div>
          <div class="flex flex-column gap-1">
            <label>{{ t('system.pos.posCode') }}</label>
            <InputText v-model="state.queryParams.code" :placeholder="t('system.pos.posCode')" />
          </div>
          <div class="flex gap-2">
            <Button v-auth="'systemPos:list'" @click="onQuery">
              <i :class="faMagnifyingGlass" class="mr-1"></i>
              {{ t('common.button.search') }}
            </Button>
            <Button severity="secondary" @click="resetQuery">
              <i :class="faArrowsRotate" class="mr-1"></i>
              {{ t('common.button.reset') }}
            </Button>
          </div>
          <div>
            <Button v-auth="'systemPos:add'" @click="openAddPos">
              <i :class="faPlus" class="mr-1"></i>
              {{ t('common.button.add') }}
            </Button>
          </div>
        </div>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.posData" style="width: 100%" v-block-loading="state.loading" showGridlines :showPagination="false"
                       authPrefix="systemPos"
                       :actions="actions">
          <Column field="name" :header="t('system.pos.posName')" />
          <Column field="code" :header="t('system.pos.posCode')" />
          <Column field="orderNo" :header="t('common.label.sort')" style="width: 70px" />
          <Column :header="t('common.label.status')" style="width: 70px">
            <template #body="{ data }">
              <Tag v-if="isEnabled(data)" severity="success">{{ t('common.label.enable') }}</Tag>
              <Tag v-else severity="danger">{{ t('common.label.disable') }}</Tag>
            </template>
          </Column>
          <TimeColumn field="createdTime" :header="t('common.label.createTime')" />
          <Column field="remark" :header="t('common.label.remark')" />
        </CrudDataTable>
      </template>
    </Card>

    <EditPos ref="editPosRef" :title="state.editPosTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemPos">
import { faArrowsRotate, faMagnifyingGlass, faPlus } from '../../../../utils/icons.ts';

import {onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import EditPos from '/@/views/tenant/system/pos/component/EditPos.vue';

import {tenantPositionListGet, tenantPositionDeletePost} from '../../../../api/tenantPosition';
import {TenantPositionEntity} from '../../../../api/_shared/data-contracts.ts';
import {isEnabled} from '../../../../utils/isEnabled.ts';

import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import Column from 'primevue/column';
import Tag from 'primevue/tag';

const editPosRef = ref<InstanceType<typeof EditPos>>();
const state = reactive({
  loading: false,
  posData: [] as Array<TenantPositionEntity>,
  queryParams: {
    name: undefined,
    code: undefined,
  },
  editPosTitle: '',
});

onMounted(async () => {
  onQuery();
});

const onQuery = async () => {
  state.loading = true;
  var res = await tenantPositionListGet({Name: state.queryParams.name, Code: state.queryParams.code});
  state.posData = res.data.data ?? [];
  state.loading = false;
};

const resetQuery = () => {
  state.queryParams.name = undefined;
  state.queryParams.code = undefined;
  onQuery();
};

const openAddPos = () => {
  state.editPosTitle = t('system.pos.addPos');
  editPosRef.value?.openDialog({isEnabled: true, orderNo: 100});
};

const openEditPos = (row: any) => {
  state.editPosTitle = t('system.pos.editPos');
  editPosRef.value?.openDialog(row);
};

const delPos = async (row: any) => {
  const confirmed = await showConfirm(`${t('system.pos.deletePosConfirm')}：【${row.name}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await tenantPositionDeletePost({id: row.id});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const actions = {
  edit: openEditPos,
  delete: delPos,
}
</script>
