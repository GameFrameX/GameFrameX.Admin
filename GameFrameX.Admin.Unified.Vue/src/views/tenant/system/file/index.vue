<template>
  <div class="sys-file-container">
    <Card>
      <template #content>
        <SearchForm
            v-model="queryParams"
            :fields="searchFields"
            :defaultSearchKey="false"
            authPrefix="systemFile"
            @query="onQuery"
            @reset="onReset"
        >
          <template #buttons>
            <Button @click="openUploadDialog" v-auth="'systemFile:uploadFile'" class="search-form-button">
              <i class="fa-solid fa-plus mr-1"></i>
              {{ t('system.file.uploadFile') }}
            </Button>
          </template>
        </SearchForm>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="tableData" v-block-loading="loading" showGridlines stripedRows style="width: 100%"
            :totalRecords="tableParams.total" :rows="tableParams.pageSize" :rowsPerPageOptions="[10, 20, 50, 100]"
            authPrefix="systemFile"
            :customActions="customActions"
            :columns="columns"
            :exportQuery="queryParams"
            actionColumnWidth="260px"
            @page="onPage" showIdColumn :idColumnHeader="t('common.label.id')" idColumnWidth="180px">
          <template #body-suffix="{ data }">
            <Tag rounded :value="data.suffix"/>
          </template>
          <template #body-url="{ data }">
            <Image
                :src="getFileUrl(data)"
                :alt="t('system.file.cannotPreview')"
                preview
                width="60"
                height="60"
                imageStyle="object-fit: scale-down"
            />
          </template>
          <template #body-provider="{ data }">{{ getFileField(data, 'provider', 'Provider') || '-' }}</template>
          <template #body-storageConfigId="{ data }">{{ getFileField(data, 'storageConfigId', 'StorageConfigId') || '-' }}</template>
          <template #body-createdTime="{ data }">{{ formatUnix(data.createdTime) }}</template>
          <template #action-extra="{ data }">
            <Button link @click="openFilePreviewDialog(data)" v-auth="'systemFile:delete'">
              <i class="fa-solid fa-eye mr-1"></i>
              {{ t('system.file.previewBtn') }}
            </Button>
            <Button link @click="downloadFile(data)" v-auth="'systemFile:downloadFile'">
              <i class="fa-solid fa-download mr-1"></i>
              {{ t('system.file.download') }}
            </Button>
            <Button link severity="danger" @click="onDelete(data)" v-auth="'systemFile:delete'">
              <i class="fa-solid fa-trash mr-1"></i>
              {{ t('common.button.delete') }}
            </Button>
            <Button link @click="openEditSysFile(data)" v-auth="'systemFile:update'">
              <i class="fa-solid fa-pen-to-square mr-1"></i>
              {{ t('common.button.edit') }}
            </Button>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <Dialog v-model:visible="dialogUploadVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(400px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-cloud-arrow-up" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ t('system.file.uploadFile') }} </span>
        </div>
      </template>
      <div>
        <div class="flex flex-column align-items-center justify-center border-dashed border-2 border-round p-5 cursor-pointer hover:border-primary" @click="triggerFileInput">
          <i class="fa-solid fa-cloud-arrow-up" style="font-size: 40px; color: var(--p-text-color-secondary)"></i>
          <div class="mt-2">{{ t('system.file.dragOrClick') }}<em>{{ t('system.file.clickUpload') }}</em></div>
          <input ref="fileInputRef" type="file" style="display: none" accept=".jpg,.png,.bmp,.gif,.txt,.pdf,.xlsx,.docx" @change="onFileSelect"/>
        </div>
        <div v-if="selectedFile" class="mt-2 text-sm">{{ selectedFile.name }}</div>
        <div class="mt-2 text-sm" style="color: var(--p-text-color-secondary)">{{ t('system.file.uploadTip') }}</div>
      </div>
      <template #footer>
        <Button severity="secondary" @click="dialogUploadVisible = false">{{ t('common.button.cancel') }}</Button>
        <Button @click="uploadFile">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>

    <Drawer v-model:visible="dialogDocxVisible" :header="fileName" :style="{width: '50%'}">
      <vue-office-docx :src="docxUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler"/>
    </Drawer>
    <Drawer v-model:visible="dialogXlsxVisible" :header="fileName" :style="{width: '50%'}">
      <vue-office-excel :src="excelUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler"/>
    </Drawer>
    <Drawer v-model:visible="dialogPdfVisible" :header="fileName" :style="{width: '50%'}">
      <vue-office-pdf :src="pdfUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler"/>
    </Drawer>
    <Dialog v-model:visible="showViewer" :modal="true" :style="{width: '80vw'}" :closable="true">
      <div class="flex justify-center">
        <Image :src="previewList[0]" preview imageStyle="max-width: 100%; max-height: 80vh"/>
      </div>
    </Dialog>
    <EditSysFile ref="editSysFileRef" :title="t('system.file.editFile')" @query="onQuery"/>
  </div>
</template>

<script lang="ts" setup name="systemFile">
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import SearchForm from '/@/components/search-form/index.vue'

import {computed, ref} from 'vue'
import {showSuccess, showError} from '../../../../utils/message.ts'
import VueOfficeExcel from '@vue-office/excel'
import VueOfficePdf from '@vue-office/pdf'
import '@vue-office/excel/lib/index.css'
import {useCrudTable} from '../../../../composables/useCrudTable.ts'
import {usePrimeTablePage} from '../../../../composables'
import EditSysFile from '/@/views/tenant/system/file/component/EditSysFile.vue'
import {downloadByUrl} from '../../../../utils/download.ts'
import {tenantFilePagePost, tenantFileDeletePost, tenantFileUploadFilePost} from '../../../../api/tenantFile';
import {TenantFileEntity} from '../../../../api/_shared/data-contracts.ts'
import type {CrudAction, CrudColumnConfig} from '../../../../components/crud-data-table/types.ts'
import {formatUnix} from '../../../../utils/formatTime.ts'

const editSysFileRef = ref<InstanceType<typeof EditSysFile>>()
const fileInputRef = ref<HTMLInputElement>()
const selectedFile = ref<File | null>(null)

const searchFields = computed(() => [
  {key: 'fileName', label: t('system.file.fileName'), type: 'input', placeholder: t('system.file.fileName')},
  {
    key: 'startTime',
    label: t('system.log.startTime'),
    type: 'date',
    placeholder: t('system.log.startTime'),
    props: {showTime: true, showSeconds: true, dateFormat: 'yy-mm-dd'},
  },
  {
    key: 'endTime',
    label: t('system.log.endTime'),
    type: 'date',
    placeholder: t('system.log.endTime'),
    props: {showTime: true, showSeconds: true, dateFormat: 'yy-mm-dd'},
  },
])

const {
  loading, tableData, queryParams, tableParams,
  onQuery: doQuery, onDelete,
} = useCrudTable({
  api: {
    pageApi: (params) => tenantFilePagePost(params),
    deleteApi: (params) => tenantFileDeletePost({id: params.id}),
  },
  deleteConfirmMessage: (row: any) => t('system.file.deleteFileConfirm') + `：【${row.fileName}】?`,
  immediate: false,
})

// 占位 customActions：仅用于触发 CrudDataTable 内置操作列的显隐与权限聚合判断
// （所有按钮均通过 #action-extra 插槽原样渲染，保留原有的 v-auth 逻辑）
const customActions: CrudAction[] = [
  {auth: 'delete', label: '', onClick: () => {}, visible: () => false},
  {auth: 'downloadFile', label: '', onClick: () => {}, visible: () => false},
  {auth: 'update', label: '', onClick: () => {}, visible: () => false},
]

const columns = computed<CrudColumnConfig[]>(() => [
  {field: 'fileName', header: t('system.file.fileName'), width: '150px'},
  {field: 'suffix', header: t('system.file.fileSuffix'), align: 'center', bodyKey: 'suffix'},
  {field: 'sizeKb', header: t('system.file.fileSizeKb'), align: 'center'},
  {field: 'url', header: t('system.file.preview'), align: 'center', bodyKey: 'url'},
  {field: 'provider', header: t('system.file.storageProvider'), align: 'center', bodyKey: 'provider'},
  {field: 'bucketName', header: t('system.file.storageLocation'), align: 'center'},
  {field: 'storageConfigId', header: t('system.file.storageConfigId'), align: 'center', bodyKey: 'storageConfigId'},
  {field: 'id', header: t('system.file.storageId'), align: 'center'},
  {field: 'createdName', header: t('system.file.uploader'), align: 'center'},
  {field: 'createdTime', header: t('common.label.createTime'), align: 'center', bodyKey: 'createdTime'},
])

const onQuery = () => {
  if (queryParams.value.startTime == null) queryParams.value.startTime = undefined
  if (queryParams.value.endTime == null) queryParams.value.endTime = undefined
  doQuery()
}

const onReset = () => {
  queryParams.value.fileName = undefined
  queryParams.value.startTime = undefined
  queryParams.value.endTime = undefined
  tableParams.value.page = 1
  doQuery()
}

const onPage = usePrimeTablePage(tableParams, doQuery)

const dialogUploadVisible = ref(false)

const openUploadDialog = () => {
  selectedFile.value = null
  dialogUploadVisible.value = true
}

const triggerFileInput = () => {
  fileInputRef.value?.click()
}

const onFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
  }
}

const uploadFile = async () => {
  if (!selectedFile.value) return
  await tenantFileUploadFilePost(selectedFile.value)
  onQuery()
  showSuccess(t('system.file.uploadSuccess'))
  dialogUploadVisible.value = false
}

const downloadFile = async (row: any) => {
  downloadByUrl({url: getFileUrl(row)})
}

const openEditSysFile = (row: any) => {
  editSysFileRef.value?.openDialog(row)
}

const getFileField = (row: any, camelKey: string, pascalKey: string) => row?.[camelKey] ?? row?.[pascalKey]

const dialogDocxVisible = ref(false)
const dialogXlsxVisible = ref(false)
const dialogPdfVisible = ref(false)
const showViewer = ref(false)
const docxUrl = ref('')
const excelUrl = ref('')
const pdfUrl = ref('')
const fileName = ref('')
const previewList = ref<string[]>([])

const openFilePreviewDialog = async (row: any) => {
  if (row.suffix == '.pdf') {
    fileName.value = `【${row.fileName}${row.suffix}】`
    pdfUrl.value = getFileUrl(row)
    dialogPdfVisible.value = true
  } else if (row.suffix == '.xlsx') {
    fileName.value = `【${row.fileName}${row.suffix}】`
    excelUrl.value = getFileUrl(row)
    dialogXlsxVisible.value = true
  } else if (['.jpg', '.png', '.jpeg', '.bmp'].findIndex((e) => e == row.suffix) > -1) {
    previewList.value = [getFileUrl(row)]
    showViewer.value = true
  } else {
    showError(t('system.file.previewNotSupported'))
  }
}

const getFileUrl = (row: TenantFileEntity): string => {
  if (row.bucketName == 'Local') {
    return `/${row.filePath}/${row.id}${row.suffix}`
  } else {
    return row.url!
  }
}

const renderedHandler = () => {
}
const errorHandler = () => {
}

onQuery()
</script>
