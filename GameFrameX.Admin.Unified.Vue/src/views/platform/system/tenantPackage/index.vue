<template>
  <div class="layout-padding">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            :defaultSearchKey="false"
            authPrefix="systemTenantPackageEntity"
            @query="onQuery"
            @reset="onReset"
            @add="onAdd"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines
                       authPrefix="systemTenantPackageEntity" :actions="actions"
                       :columns="columns" :exportQuery="queryParams">
          <template #body-isEnabled="{ data }">
            <Tag :value="isEnabled(data) ? t('common.label.enable') : t('common.label.disable')"
                 :severity="isEnabled(data) ? 'success' : 'danger'" />
          </template>
        </CrudDataTable>
      </template>
    </Card>
    <EditTenantPackage ref="editRef" @query="onQuery" />
  </div>
</template>

<script lang="ts" setup name="systemTenantPackage">
import { computed, ref } from 'vue';
import CrudDataTable from '/@/components/crud-data-table/index.vue';
import { useCrudTable } from '../../../../composables/useCrudTable.ts';
import { globalPackagePagePost, globalPackageDeletePost } from '../../../../api/globalPackage/index.ts';
import EditTenantPackage from './component/EditTenantPackage.vue';
import SearchForm from '/@/components/search-form/index.vue';
import { isEnabled } from '../../../../utils/isEnabled.ts';
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts';

const editRef = ref();
const searchFields = computed(() => [
  { key: 'name', label: t('system.tenantPackage.name'), type: 'input', placeholder: t('system.tenantPackage.name') },
]);

const { loading, tableData, queryParams, onQuery, onReset, onDelete } = useCrudTable({
  api: {
    pageApi: (params) => globalPackagePagePost(params),
    deleteApi: (params) => globalPackageDeletePost({ id: params.id }),
  },
  immediate: true,
});

const onAdd = () => editRef.value?.openDialog({ isEnabled: true });
const onEdit = (row: any) => editRef.value?.openDialog(row);

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'name', header: t('system.tenantPackage.name')},
  {field: 'code', header: t('system.tenantPackage.code')},
  {field: 'maxUserCount', header: t('system.tenantPackage.maxUserCount')},
  {field: 'maxOrgCount', header: t('system.tenantPackage.maxOrgCount')},
  {field: 'maxOrgLevel', header: t('system.tenantPackage.maxOrgLevel')},
  {field: 'maxRoleCount', header: t('system.tenantPackage.maxRoleCount')},
  {field: 'maxGameAppCount', header: t('system.tenantPackage.maxGameAppCount')},
  {field: 'maxPackageCountPerApp', header: t('system.tenantPackage.maxPackageCountPerApp')},
  {field: 'gracePeriodDays', header: t('system.tenantPackage.gracePeriodDays')},
  {field: 'isEnabled', header: t('common.label.status'), bodyKey: 'isEnabled'},
  {field: 'remark', header: t('common.label.remark')},
]);

const actions = { edit: onEdit, delete: onDelete };
</script>
