<template>
  <div class="wechatUser-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            authPrefix="systemWechatUser"
            @query="onQuery"
        />
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
            :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
            authPrefix="systemWechatUser"
            :actions="actions"
            :columns="columns"
            :exportQuery="queryParams"
            @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <template #body-platformType="{ data }">
            <Tag v-if="data.platformType === 1" :value="t('system.wechatUser.wechatMp')"/>
            <Tag v-else-if="data.platformType === 2" :value="t('system.wechatUser.wechatMini')"/>
            <Tag v-else-if="data.platformType === 3" :value="t('system.wechatUser.qq')"/>
            <Tag v-else-if="data.platformType === 4" :value="t('system.wechatUser.alipay')"/>
            <Tag v-else-if="data.platformType === 5" :value="t('system.wechatUser.gitee')"/>
            <Tag v-else :value="t('system.wechatUser.unknown')"/>
          </template>
          <template #body-avatar="{ data }">
            <Avatar :image="data.avatar" :size="'normal'" style="vertical-align: middle"/>
          </template>
          <template #body-sex="{ data }">
            <Tag v-if="data.sex === 0" :value="t('common.label.male')"/>
            <Tag v-else severity="danger" :value="t('common.label.female')"/>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <EditWechatUser ref="editWechatUserRef" :title="editTitle" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="wechatUser">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'

import {computed, ref} from 'vue'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import {tenantWechatUserPagePost, tenantWechatUserDeletePost} from '../../../../api/tenantWechatUser'
import EditWechatUser from './component/EditWechatUser.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'

const editWechatUserRef = ref<InstanceType<typeof EditWechatUser>>()
const editTitle = ref('')

const searchFields = computed(() => [
  {key: 'nickName', label: t('common.label.nickname'), type: 'input', placeholder: t('common.label.nickname')},
  {key: 'mobile', label: t('common.label.phone'), type: 'input', placeholder: t('common.label.phone')},
])

const {
  loading,
  tableData,
  queryParams,
  tableParams,
  onQuery
} = useCrudTable({
  api: {
    pageApi: (params) => tenantWechatUserPagePost(params),
    deleteApi: (params) => tenantWechatUserDeletePost({id: params.id}),
  },
  immediate: true,
})

queryParams.value = {nickName: undefined, mobile: undefined}
tableParams.value.pageSize = 10

const onPage = usePrimeTablePage(tableParams, onQuery)

const delWechatUser = async (row: any) => {
  if (!await showConfirm(`确定删除微信账号：【${row.nickName}】?`, t('common.message.tip'), {type: 'danger'})) return
  await tenantWechatUserDeletePost({id: row.id})
  onQuery()
  showSuccess(t('common.message.deleteSuccess'))
}

const openEditWechatUser = (row: any) => {
  editTitle.value = '编辑微信账号'
  editWechatUserRef.value?.openDialog(row)
}

const actions = {
  edit: openEditWechatUser,
  delete: delWechatUser,
}

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'openId', header: t('system.wechatUser.openId'), align: 'center'},
  {field: 'unionId', header: t('system.wechatUser.unionId'), align: 'center'},
  {field: 'platformType', header: t('message.system.wechatUser.platformType'), width: '110px', align: 'center', bodyKey: 'platformType'},
  {field: 'nickName', header: t('common.label.nickname'), align: 'center'},
  {field: 'avatar', header: t('common.label.avatar'), width: '70px', align: 'center', bodyKey: 'avatar'},
  {field: 'mobile', header: t('common.label.phone'), align: 'center', sensitive: true},
  {field: 'sex', header: t('system.wechatUser.sex'), width: '60px', align: 'center', bodyKey: 'sex'},
  {field: 'city', header: t('system.wechatUser.city'), align: 'center'},
  {field: 'province', header: t('message.system.wechatUser.province'), align: 'center'},
  {field: 'country', header: t('system.wechatUser.country'), align: 'center'},
])
</script>
