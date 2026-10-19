<template>
  <div class="sys-job-container">
    <Card>
      <template #content>
        <SearchForm
          v-model="state.queryParams"
          :fields="searchFields"
          :defaultSearchKey="false"
          @query="onQuery"
        >
          <template #buttons>
            <Button @click="openAddJobDetail" v-auth="'systemJob:addJobDetail'" v-tooltip="t('system.job.addJobTip')">
              <i class="fa-solid fa-circle-plus"></i>
            </Button>
            <Button @click="startAllJob" v-tooltip="t('system.job.startAllJobsTip')">
              <i class="fa-solid fa-play"></i>
            </Button>
            <Button @click="pauseAllJob" v-tooltip="t('system.job.pauseAllJobsTip')">
              <i class="fa-solid fa-pause"></i>
            </Button>
            <Button @click="cancelSleep" v-tooltip="t('system.job.cancelSleepTip')">
              <i class="fa-solid fa-clock"></i>
            </Button>
            <Button @click="persistAll" v-tooltip="t('system.job.persistAllTip')">
              <i class="fa-solid fa-link"></i>
            </Button>
            <Button severity="secondary" @click="openJobCluster">
              <i class="fa-solid fa-coins mr-1"></i>
              {{ t('system.job.jobCluster') }}
            </Button>
            <Button severity="secondary" @click="openJobDashboard">
              <i class="fa-solid fa-table-cells mr-1"></i>
              {{ t('system.job.jobDashboard') }}
            </Button>
          </template>
        </SearchForm>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #content>
        <CrudDataTable :value="state.jobData" v-block-loading="state.loading" showGridlines stripedRows
                       style="width: 100%" v-model:expandedRows="expandedRows"
                       :totalRecords="tableParams.total"
                       :rows="tableParams.pageSize"
                       :rowsPerPageOptions="[10, 20, 50, 100]"
                       @page="onPage">
          <Column expander style="width: 40px"/>
          <Column field="jobDetail.jobId" :header="t('system.job.jobId')" style="width: 180px">
            <template #body="{ data }">
              <div style="display: flex; align-items: center">
                <i class="fa-solid fa-clock mr-1"></i>
                <span>{{ (data as JobOutput).jobDetail?.jobId }}</span>
              </div>
            </template>
          </Column>
          <Column field="jobDetail.groupName" :header="t('system.job.groupName')" style="width: 100px" align="center"/>
          <Column field="jobDetail.jobType" :header="t('common.label.type')"/>
          <Column field="jobDetail.description" :header="t('common.label.description')"/>
          <Column field="jobDetail.concurrent" :header="t('system.job.executeMode')" style="width: 90px" align="center">
            <template #body="{ data }">
              <Tag v-if="(data as JobOutput).jobDetail?.concurrent == true" severity="success" :value="t('system.job.parallel')"/>
              <Tag v-else severity="warn" :value="t('system.job.serial')"/>
            </template>
          </Column>
          <Column field="jobDetail.createType" :header="t('message.system.job.createType')" style="width: 110px"
                  align="center">
            <template #body="{ data }">
              <Tag v-if="(data as JobOutput).jobDetail?.createType == JobCreateTypeEnum.Value0" severity="info"
                   :value="t('system.job.builtIn')"/>
              <Tag v-if="(data as JobOutput).jobDetail?.createType == JobCreateTypeEnum.Value1" severity="warn"
                   :value="t('system.job.script')"/>
              <Tag v-if="(data as JobOutput).jobDetail?.createType == JobCreateTypeEnum.Value2" severity="success"
                   :value="t('system.job.httpRequest')"/>
            </template>
          </Column>
          <Column field="jobDetail.updatedTime" :header="t('message.common.label.updateTime')" style="width: 130px"
                  align="center">
            <template #body="{ data }">
              {{ formatUnix((data as JobOutput).jobDetail?.updatedTime) }}
            </template>
          </Column>
          <Column field="jobDetail.properties" :header="t('system.job.extraData')">
            <template #body="{ data }">
              <span v-if="(data as JobOutput).jobDetail?.createType != JobCreateTypeEnum.Value2">
                {{ (data as JobOutput).jobDetail?.properties }}
              </span>
              <div v-else style="text-align: center">
                <OverlayPanel ref="op">
                  <div class="p-4" style="min-width: 300px">
                    <h3 class="mb-2">{{ t('system.job.httpRequestParams') }}</h3>
                    <table class="w-full text-sm">
                      <tbody>
                      <tr>
                        <td class="font-semibold pr-4 py-1" style="width: 80px">{{ t('system.job.requestUrl') }}</td>
                        <td>{{ getHttpJobMessage((data as JobOutput).jobDetail?.properties).requestUri }}</td>
                      </tr>
                      <tr>
                        <td class="font-semibold pr-4 py-1">{{ t('system.job.requestMethod') }}</td>
                        <td>{{ getHttpMethodDesc(getHttpJobMessage((data as JobOutput).jobDetail?.properties).httpMethod) }}</td>
                      </tr>
                      <tr>
                        <td class="font-semibold pr-4 py-1">{{ t('system.job.requestBody') }}</td>
                        <td>{{ getHttpJobMessage((data as JobOutput).jobDetail?.properties).body }}</td>
                      </tr>
                      </tbody>
                    </table>
                  </div>
                </OverlayPanel>
                <Tag severity="info" :value="t('system.job.requestParams')" style="cursor: pointer"
                     @mouseover="($refs.op as any)?.show($event)" @mouseleave="($refs.op as any)?.hide()"/>
              </div>
            </template>
          </Column>
          <Column :header="t('common.table.action')" style="width: 200px" align="center">
            <template #body="{ data }">
              <Button link @click="openAddJobTrigger(data)" v-tooltip="t('system.job.addTriggerTip')">
                <i class="fa-solid fa-circle-plus"></i>
              </Button>
              <Button link @click="runJob(data)" v-tooltip="t('system.job.runJobTip')">
                <i class="fa-solid fa-circle-check"></i>
              </Button>
              <Button link @click="startJob(data)" v-tooltip="t('system.job.startJobTip')">
                <i class="fa-solid fa-play"></i>
              </Button>
              <Button link @click="pauseJob(data)" v-tooltip="t('system.job.pauseJobTip')">
                <i class="fa-solid fa-pause"></i>
              </Button>
              <Button link @click="openEditJobDetail(data)" v-auth="'systemJob:updateJobDetail'" v-tooltip="t('system.job.editJobTip')">
                <i class="fa-solid fa-pen-to-square"></i>
              </Button>
              <Button link severity="danger" @click="delJobDetail(data)" v-auth="'systemJob:deleteJobDetail'"
                      v-tooltip="t('system.job.deleteJobTip')">
                <i class="fa-solid fa-trash"></i>
              </Button>
            </template>
          </Column>
          <template #expansion="{ data }">
            <DataTable :value="(data as JobOutput).jobTriggers" showGridlines style="margin-left: 48px; width: calc(100% - 48px)"
                       size="small">
              <Column field="id" :header="t('common.label.id')" style="width: 180px" align="center"/>
              <Column field="triggerId" :header="t('system.job.triggerId')" style="width: 120px"/>
              <Column field="triggerType" :header="t('system.job.triggerType')"/>
              <Column field="args" :header="t('message.system.job.args')"/>
              <Column field="description" :header="t('common.label.description')" style="width: 120px"/>
              <Column field="status" :header="t('common.label.status')" style="width: 120px" align="center">
                <template #body="{ data: trigger }">
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 0" severity="warn" :value="t('system.job.blocked')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 1" :value="t('system.job.running')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 2" severity="success" :value="t('system.job.triggerRunning')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 3" severity="danger" :value="t('system.job.triggerPaused')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 4" severity="danger" :value="t('system.job.triggerBlocked')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 5" :value="t('system.job.triggerReadyFromFailure')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 6" severity="danger" :value="t('system.job.triggerArchived')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 7" severity="danger" :value="t('system.job.triggerCrashed')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 8" severity="danger" :value="t('system.job.triggerExceeded')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 9" severity="danger" :value="t('system.job.triggerNoNextTime')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 10" severity="danger" :value="t('system.job.triggerNotStarted')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 11" severity="danger" :value="t('system.job.triggerUnknown')"/>
                  <Tag v-if="(trigger as SystemJobTriggerEntity).status == 12" severity="danger" :value="t('system.job.triggerUnknownHandler')"/>
                </template>
              </Column>
              <TimeColumn field="startTime" :header="t('system.job.startTime')" width="100px" align="center" />
              <TimeColumn field="endTime" :header="t('system.job.endTime')" width="100px" align="center" />
              <TimeColumn field="lastRunTime" :header="t('message.system.job.lastRunTime')" width="130px" align="center" />
              <TimeColumn field="nextRunTime" :header="t('message.system.job.nextRunTime')" width="130px" align="center" />
              <Column field="numberOfRuns" :header="t('message.system.job.numberOfRuns')" style="width: 100px"
                      align="center"/>
              <Column field="maxNumberOfRuns" :header="t('system.job.maxRunCount')" style="width: 120px" align="center"/>
              <Column field="numberOfErrors" :header="t('message.system.job.numberOfErrors')" style="width: 100px"
                      align="center"/>
              <Column field="maxNumberOfErrors" :header="t('system.job.maxErrorCount')" style="width: 120px"
                      align="center"/>
              <Column field="numRetries" :header="t('system.job.retryCount')" style="width: 100px" align="center"/>
              <Column field="retryTimeout" :header="t('message.system.job.retryTimeout')" style="width: 100px"
                      align="center"/>
              <Column field="startNow" :header="t('message.system.job.startNow')" style="width: 100px" align="center">
                <template #body="{ data: trigger }">
                  <Tag v-if="(trigger as SystemJobTriggerEntity).startNow == true" :value="t('common.label.yes')"/>
                  <Tag v-else severity="info" :value="t('common.label.no')"/>
                </template>
              </Column>
              <Column field="runOnStart" :header="t('system.job.runOnceOnStart')" style="width: 150px"
                      align="center">
                <template #body="{ data: trigger }">
                  <Tag v-if="(trigger as SystemJobTriggerEntity).runOnStart == true" :value="t('common.label.yes')"/>
                  <Tag v-else severity="info" :value="t('common.label.no')"/>
                </template>
              </Column>
              <Column field="resetOnlyOnce" :header="t('message.system.job.resetRunCount')" style="width: 120px"
                      align="center">
                <template #body="{ data: trigger }">
                  <Tag v-if="(trigger as SystemJobTriggerEntity).resetOnlyOnce == true" :value="t('common.label.yes')"/>
                  <Tag v-else severity="info" :value="t('common.label.no')"/>
                </template>
              </Column>
              <TimeColumn field="updatedTime" :header="t('message.common.label.updateTime')" width="130px" align="center" />
              <Column :header="t('common.table.action')" style="width: 140px" align="center">
                <template #body="{ data: trigger }">
                  <Button link @click="startTrigger(trigger)" v-tooltip="t('system.job.startTriggerTip')">
                    <i class="fa-solid fa-play"></i>
                  </Button>
                  <Button link @click="pauseTrigger(trigger)" v-tooltip="t('system.job.pauseTriggerTip')">
                    <i class="fa-solid fa-pause"></i>
                  </Button>
                  <Button link @click="openEditJobTrigger(trigger)" v-tooltip="t('system.job.editTriggerTip')">
                    <i class="fa-solid fa-pen-to-square"></i>
                  </Button>
                  <Button link severity="danger" @click="delJobTrigger(trigger)" v-tooltip="t('system.job.deleteTriggerTip')">
                    <i class="fa-solid fa-trash"></i>
                  </Button>
                </template>
              </Column>
            </DataTable>
          </template>
        </CrudDataTable>
      </template>
    </Card>

    <EditJobDetail ref="editJobDetailRef" :title="state.editJobDetailTitle" @query="onQuery"/>
    <EditJobTrigger ref="editJobTriggerRef" :title="state.editJobTriggerTitle" @query="onQuery"/>
    <JobCluster ref="editJobClusterRef"/>
  </div>
</template>

<script lang="ts" setup name="systemJob">
import {
  faCircleCheck,
  faCirclePlus,
  faClock,
  faCoins,
  faLink,
  faPause,
  faPenToSquare,
  faPlay,
  faTableCells,
  faTrash
} from '../../../../utils/icons.ts';

import {computed, onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess} from '../../../../utils/message.ts';
import {useRouter} from 'vue-router';

import EditJobDetail from '/@/views/platform/system/job/component/EditJobDetail.vue';
import EditJobTrigger from '/@/views/platform/system/job/component/EditJobTrigger.vue';
import JobCluster from '/@/views/platform/system/job/component/JobCluster.vue';

import {
  systemJobPageJobDetailPost,
  systemJobDeleteJobDetailPost,
  systemJobDeleteJobTriggerPost,
  systemJobStartAllJobPost,
  systemJobPauseAllJobPost,
  systemJobRunJobPost,
  systemJobStartJobPost,
  systemJobPauseJobPost,
  systemJobStartTriggerPost,
  systemJobPauseTriggerPost,
  systemJobCancelSleepPost,
  systemJobPersistAllPost,
} from '../../../../api/systemJob';
import {JobCreateTypeEnum, JobOutput, SystemJobTriggerEntity} from '../../../../api/_shared/data-contracts.ts';

import CrudDataTable from '/@/components/crud-data-table/index.vue';
import SearchForm from '/@/components/search-form/index.vue';
import {formatUnix} from '../../../../utils/formatTime.ts'
import {usePrimeTablePage} from '../../../../composables';

const router = useRouter();
const editJobDetailRef = ref<InstanceType<typeof EditJobDetail>>();
const editJobTriggerRef = ref<InstanceType<typeof EditJobTrigger>>();
const editJobClusterRef = ref<InstanceType<typeof JobCluster>>();
const expandedRows = ref<JobOutput[]>([]);
const op = ref();
const state = reactive({
  loading: false,
  jobData: [] as Array<JobOutput>,
  queryParams: {
    jobId: undefined as string | undefined,
    description: undefined as string | undefined,
  },
  editJobDetailTitle: '',
  editJobTriggerTitle: '',
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
  var res = await systemJobPageJobDetailPost(params);
  state.jobData = res.data.data?.items ?? [];
  tableParams.value.total = res.data.data?.total;
  state.loading = false;
};

const searchFields = computed(() => [
  { key: 'jobId', label: t('system.job.jobId'), type: 'input' as const, placeholder: t('system.job.jobId') },
  { key: 'description', label: t('system.job.description'), type: 'input' as const, placeholder: t('system.job.description') },
]);

const openAddJobDetail = () => {
  state.editJobDetailTitle = t('system.job.jobInfo');
  editJobDetailRef.value?.openDialog({
    concurrent: true,
    includeAnnotations: true,
    groupName: 'default',
    createType: JobCreateTypeEnum.Value2
  });
};

const openEditJobDetail = (row: JobOutput) => {
  state.editJobDetailTitle = t('system.job.jobInfo');
  editJobDetailRef.value?.openDialog(row.jobDetail);
};

const delJobDetail = async (row: JobOutput) => {
  const confirmed = await showConfirm(t('system.job.deleteJobConfirm') + `：【${row.jobDetail?.jobId}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await systemJobDeleteJobDetailPost({jobId: row.jobDetail?.jobId});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const openAddJobTrigger = (row: JobOutput) => {
  state.editJobTriggerTitle = t('system.job.triggerId');
  editJobTriggerRef.value?.openDialog({
    jobId: row.jobDetail?.jobId,
    retryTimeout: 1000,
    startNow: true,
    runOnStart: true,
    resetOnlyOnce: true,
    triggerType: 'GameFrameX.Core.Job.PeriodTrigger',
  });
};

const openEditJobTrigger = (row: SystemJobTriggerEntity) => {
  state.editJobTriggerTitle = t('system.job.triggerId');
  editJobTriggerRef.value?.openDialog(row);
};

const delJobTrigger = async (row: SystemJobTriggerEntity) => {
  const confirmed = await showConfirm(t('system.job.deleteTriggerConfirm') + `：【${row.triggerId}】?`, t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return
  await systemJobDeleteJobTriggerPost({jobId: row.jobId, triggerId: row.triggerId});
  onQuery();
  showSuccess(t('common.message.deleteSuccess'));
};

const startAllJob = async () => {
  await systemJobStartAllJobPost();
  showSuccess(t('system.job.startAllJobsSuccess'));
};

const pauseAllJob = async () => {
  await systemJobPauseAllJobPost();
  showSuccess(t('system.job.pauseAllJobsSuccess'));
};

const runJob = async (row: JobOutput) => {
  await systemJobRunJobPost({jobId: row.jobDetail?.jobId});
  showSuccess(t('system.job.runJobSuccess'));
};

const startJob = async (row: JobOutput) => {
  await systemJobStartJobPost({jobId: row.jobDetail?.jobId});
  showSuccess(t('system.job.startJobSuccess'));
};

const pauseJob = async (row: JobOutput) => {
  await systemJobPauseJobPost({jobId: row.jobDetail?.jobId});
  showSuccess(t('system.job.pauseJobSuccess'));
};

const startTrigger = async (row: SystemJobTriggerEntity) => {
  await systemJobStartTriggerPost({jobId: row.jobId, triggerId: row.triggerId});
  showSuccess(t('system.job.startTriggerSuccess'));
};

const pauseTrigger = async (row: SystemJobTriggerEntity) => {
  await systemJobPauseTriggerPost({jobId: row.jobId, triggerId: row.triggerId});
  showSuccess(t('system.job.pauseTriggerSuccess'));
};

const cancelSleep = async () => {
  await systemJobCancelSleepPost();
  showSuccess(t('system.job.cancelSleepSuccess'));
};

const persistAll = async () => {
  await systemJobPersistAllPost();
  showSuccess(t('system.job.persistAllSuccess'));
};

const openJobCluster = () => {
  editJobClusterRef.value?.openDrawer();
};

const openJobDashboard = () => {
  router.push({path: '/platform/job/dashboard'});
};

const getHttpJobMessage = (properties: string | undefined | null): HttpJobMessage => {
  if (properties === undefined || properties === null || properties === '') return {};
  const propData = JSON.parse(properties);
  const httpJobMessageNet = JSON.parse(propData['HttpJob']);
  return {
    requestUri: httpJobMessageNet.RequestUri,
    httpMethod: JSON.stringify(httpJobMessageNet.HttpMethod),
    body: httpJobMessageNet.Body,
  };
};

const getHttpMethodDesc = (httpMethodStr: string | undefined | null): string => {
  if (httpMethodStr === undefined || httpMethodStr === null || httpMethodStr === '') return '';
  for (const key in editJobDetailRef.value?.httpMethodDef) {
    if (editJobDetailRef.value?.httpMethodDef[key] === httpMethodStr) return key;
  }
  return '';
};

const onPage = usePrimeTablePage(tableParams, onQuery)
</script>
