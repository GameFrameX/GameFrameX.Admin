<template>
  <div class="tenantGameLoginTicketKeyEntity-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            :showAdd="false"
            authPrefix="tenantGameLoginTicketKeyEntity"
            @query="onQuery"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       authPrefix="tenantGameLoginTicketKeyEntity"
                       :customActions="customActions"
                       actionColumnWidth="230px"
                       showIdColumn
                       idColumnWidth="120px"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-publicKeyPem="{ data }">
            <div class="key-pem font-mono" :title="data.publicKeyPem">{{ data.publicKeyPem }}</div>
          </template>
          <template #body-notBeforeTime="{ data }">
            <span v-if="data.notBeforeTime">{{ formatUnix(data.notBeforeTime) }}</span>
          </template>
          <template #body-expireTime="{ data }">
            <span v-if="data.expireTime">{{ formatUnix(data.expireTime) }}</span>
            <Tag v-else severity="info" value="永不过期"/>
          </template>
          <template #body-isEnabled="{ data }">
            <Tag v-if="data.isEnabled" severity="success" value="启用"/>
            <Tag v-else severity="danger" value="禁用"/>
          </template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
        </CrudDataTable>
        <div class="key-tip">
          <i class="fa-solid fa-shield-halved"></i>
          出于安全考虑，签名私钥仅在服务端保存，此页面只展示公钥与元数据；密钥默认永不过期，可在轮换时指定到期日，到期的旧密钥被自然排除。
        </div>
      </template>
    </Card>
    <rotateDialog ref="rotateDialogRef" @rotate="onRotateSubmit"/>
  </div>
</template>

<script lang="ts" setup name="tenantGameLoginTicketKeyEntity">
import {computed, ref} from 'vue'
import SearchForm from '/@/components/search-form/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudAction} from '../../../../components/crud-data-table/types.ts'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {formatUnix} from '../../../../utils/formatTime.ts'
import { tenantGameLoginTicketKeyEntityPagePost, tenantGameLoginTicketKeyEntityRotatePost, tenantGameLoginTicketKeyEntityEnablePost, tenantGameLoginTicketKeyEntityDisablePost } from '../../../../api/tenantGameLoginTicketKeyEntity'
import rotateDialog from './component/RotateDialog.vue'

const searchFields = computed(() => [])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery,
} = useCrudTable({
  api: {
    pageApi: tenantGameLoginTicketKeyEntityPagePost,
  },
  immediate: true,
})

const rotateDialogRef = ref()

const onPage = usePrimeTablePage(tableParams, onQuery)

const onRotateSubmit = async (payload: { appId: number, expireTime: number | null }) => {
  const confirmed = await showConfirm('确认为该应用轮换签名密钥？新密钥立即生效，若指定到期日则旧密钥到期后自然退场。', '提示')
  if (!confirmed) {
    return
  }
  await tenantGameLoginTicketKeyEntityRotatePost({appId: payload.appId, expireTime: payload.expireTime})
  onQuery()
  showSuccess('签名密钥已轮换')
}

const onEnable = async (row: any) => {
  await tenantGameLoginTicketKeyEntityEnablePost({id: row.id})
  onQuery()
  showSuccess('已启用')
}

const onDisable = async (row: any) => {
  const confirmed = await showConfirm('禁用后该密钥无法用于签发与验签，确认禁用？', '提示')
  if (!confirmed) {
    return
  }
  await tenantGameLoginTicketKeyEntityDisablePost({id: row.id})
  onQuery()
  showSuccess('已禁用')
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'appId', header: '应用ID', width: '120px', align: 'center'},
  {field: 'kid', header: 'Key ID', width: '240px'},
  {field: 'algorithm', header: '算法', width: '90px', align: 'center'},
  {field: 'publicKeyPem', header: '公钥 (PEM)', bodyKey: 'publicKeyPem'},
  {field: 'notBeforeTime', header: '生效时间', width: '170px', align: 'center', bodyKey: 'notBeforeTime'},
  {field: 'expireTime', header: '过期时间', width: '170px', align: 'center', bodyKey: 'expireTime'},
  {field: 'isEnabled', header: '状态', width: '90px', align: 'center', bodyKey: 'isEnabled'},
  {field: 'createdTime', header: '创建时间', width: '170px', align: 'center', bodyKey: 'createdTime'},
])

const customActions: CrudAction[] = [
  {auth: 'update', label: '轮换', icon: 'fa-solid fa-rotate', onClick: (row: any) => rotateDialogRef.value?.open(row)},
  {auth: 'update', label: '启用', icon: 'fa-solid fa-toggle-on', severity: 'success', onClick: onEnable, visible: (row: any) => !row.isEnabled},
  {auth: 'update', label: '禁用', icon: 'fa-solid fa-toggle-off', severity: 'warn', onClick: onDisable, visible: (row: any) => !!row.isEnabled},
]
</script>

<style scoped>
.key-pem {
  max-width: 320px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
  color: var(--p-text-muted-color);
}

.key-tip {
  margin-top: 8px;
  padding: 8px 12px;
  background: var(--p-surface-100);
  border-radius: 6px;
  font-size: 12px;
  color: var(--p-text-muted-color);
}

.key-tip i {
  margin-right: 6px;
  color: var(--p-primary-color);
}
</style>
