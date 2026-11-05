<template>
  <div class="appversion-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :showAdd="true"
            authPrefix="gameAppReleaseVersion"
            @query="onQuery"

            @add="openAddTenantGameAppReleaseVersion"
        />
      </template>
    </Card>
    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows rowKey="id"
                       :totalRecords="tableParams.total" :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100, 200, 500]"
                       authPrefix="gameAppReleaseVersion"
                       :actions="actions"
                       :columns="columns"
                       :exportQuery="queryParams"
                       @page="onPage">
          <template #body-isForce="{ data }">
            <span v-if="data.isForce">{{ t('message.common.label.yes') }}</span>
            <span v-else>{{ t('message.common.label.no') }}</span>
          </template>
        </CrudDataTable>
        <editDialog ref="editDialogRef" :title="editTenantGameAppReleaseVersionTitle" @reloadTable="onQuery"/>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup="" name="appversion">
import {computed, ref, watch} from 'vue'
import {useRoute} from 'vue-router'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import type {CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'

import editDialog from '/@/views/clientDelivery/release/appReleaseVersion/component/EditDialog.vue'
import { gameAppReleaseVersionPagePost, gameAppReleaseVersionDeletePost } from '../../../../api/gameAppReleaseVersion'

import { globalPlatformSelectPost } from '../../../../api/globalPlatform'
import { globalLanguageSelectPost } from '../../../../api/globalLanguage'
import { gameAppVersionDefinitionSelectPost } from '../../../../api/gameAppVersionDefinition'
import { gameAppPackageListGet } from '../../../../api/gameAppPackage'
import { channelDefinitionAvailableGet } from '../../../../api/channelDefinition'
import { mapAvailableChannelDefinitions } from '../../../../utils/channelDefinition.ts'
import {MAX_SELECT_SIZE} from '../../../../constants/pagination.ts'

const route = useRoute()

const editDialogRef = ref()
const editTenantGameAppReleaseVersionTitle = ref('')
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
  app: [],
})

const {
  loading, tableData, queryParams, tableParams,
  onQuery, onDelete,
} = useCrudTable({
  api: {pageApi: gameAppReleaseVersionPagePost, deleteApi: gameAppReleaseVersionDeletePost},
  immediate: true,
})

const onPage = usePrimeTablePage(tableParams, onQuery)

const searchFields = computed(() => [
  { key: 'channel', label: t('message.client.channelName'), type: 'select', options: querySelections.value.channel, placeholder: t('message.common.form.pleaseSelect') },
  { key: 'oldVersion', label: t('message.client.oldVersion'), type: 'select', options: querySelections.value.oldVersion, placeholder: t('message.common.form.pleaseSelect') },
  { key: 'platform', label: t('message.client.platform'), type: 'select', options: querySelections.value.platform, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
  { key: 'appVersion', label: t('message.client.newVersion'), type: 'select', options: querySelections.value.version, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
  { key: 'package', label: t('message.client.packageName'), type: 'select', options: querySelections.value.package, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
  { key: 'language', label: t('message.client.languageName'), type: 'select', options: querySelections.value.language, placeholder: t('message.common.form.pleaseSelect'), advanced: true },
])

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'channel', header: t('message.client.channelName'), width: '150px'},
  {field: 'platform', header: t('message.client.platformName'), width: '150px'},
  {field: 'oldVersion', header: t('message.client.oldVersion'), width: '100px'},
  {field: 'appVersion', header: t('message.client.newVersion'), width: '100px'},
  {field: 'downloadUrl', header: t('message.client.downloadUrl'), width: '120px'},
  {field: 'package', header: t('message.client.packageName'), width: '200px'},
  {field: 'language', header: t('message.client.languageName'), width: '120px', responsivePriority: 2},
  {field: 'isForce', header: t('message.client.forceUpdate'), width: '120px', bodyKey: 'isForce'},
  {field: 'title', header: t('message.client.announcementTitle'), width: '180px', responsivePriority: 2},
  {field: 'announcement', header: t('message.client.announcementContent'), width: '180px', responsivePriority: 2},
  {field: 'createdName', header: t('message.common.label.creator'), width: '210px', responsivePriority: 2},
  {field: 'updatedName', header: t('message.common.label.modifier'), width: '210px', responsivePriority: 2},
  {field: 'description', header: t('message.common.label.description'), width: '165px', responsivePriority: 2},
])

const onLanguageQuery = async () => {
  const res = await globalLanguageSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.language = res.data.data
}

const onChannelQuery = async () => {
  const res = await channelDefinitionAvailableGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.channel = mapAvailableChannelDefinitions(res.data.data || [], 'code')
}

const onPlatformQuery = async () => {
  const res = await globalPlatformSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.platform = res.data.data
}

const onVersionQuery = async () => {

  const res = await gameAppVersionDefinitionSelectPost({pageSize: MAX_SELECT_SIZE})
  querySelections.value.version = res.data.data
  querySelections.value.oldVersion = res.data.data
}

// 包名候选按当前应用加载：gameAppPackageListGet 已按应用过滤、默认包置顶；
const buildPackageOptions = (packages: any[]) => {
  const options: any[] = [{label: t('message.client.appDefaultPackage'), value: 'default'}]
  for (const p of packages || []) {
    const val = p.name ?? p.packageName
    if (val === null || val === undefined || val === '') {
      continue
    }
    if (options.some((o) => o.value === val)) {
      continue
    }
    const version = p.packageVersion ? ` (${p.packageVersion})` : ''
    options.push({label: `${p.packageName || p.name}${version}`, value: val})
  }
  return options
}

const onPackageQuery = async () => {


  const res = await gameAppPackageListGet({pageSize: MAX_SELECT_SIZE})
  querySelections.value.package = buildPackageOptions(res.data?.data || [])
}

// 打开新增页面
const openAddTenantGameAppReleaseVersion = () => {
  editTenantGameAppReleaseVersionTitle.value = t('message.client.addAppVersion')
  editDialogRef.value.isEditor = false
  editDialogRef.value.openDialog({})
}

// 打开编辑页面
const openEditTenantGameAppReleaseVersion = (row: any) => {
  editTenantGameAppReleaseVersionTitle.value = t('message.client.editAppVersion')
  editDialogRef.value.isEditor = true
  editDialogRef.value.openDialog(row)
}

const actions = {
  edit: openEditTenantGameAppReleaseVersion,
  delete: onDelete,
}

// 切换应用：清空包名筛选并按新应用重载包名候选，避免残留其它应用的包。

onLanguageQuery()
onPackageQuery()
onPlatformQuery()
onChannelQuery()
onVersionQuery()
</script>
