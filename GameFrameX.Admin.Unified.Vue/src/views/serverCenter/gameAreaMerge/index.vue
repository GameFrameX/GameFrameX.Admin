<template>
  <section class="merge-page" data-testid="merge-page" :aria-label="t('message.gameAreaMerge.title')">
    <Card>
      <template #content>
        <div class="flex justify-content-between align-items-center gap-3">
          <div>
            <h2>{{ t('message.gameAreaMerge.title') }}</h2>
            <p>{{ t('message.gameAreaMerge.workflow') }}</p>
          </div>
          <div class="flex gap-2">
            <Button v-auth="'gameAreaMerge:page'" :label="t('message.gameAreaMerge.refresh')" :loading="loading" @click="loadPlans" />
            <Button v-auth="'gameAreaMerge:add'" :label="t('message.gameAreaMerge.create')" @click="openCreate" />
          </div>
        </div>
        <DataTable :value="plans" :loading="loading" dataKey="id" stripedRows>
          <Column field="planId" :header="t('message.gameAreaMerge.planId')" />
          <Column field="targetServerId" :header="t('message.gameAreaMerge.target')" />
          <Column field="sourceServerIds" :header="t('message.gameAreaMerge.sources')" />
          <Column :header="t('message.gameAreaMerge.status')"><template #body="{ data }"><Tag :value="statusLabels[data.status]" /></template></Column>
          <Column field="reason" :header="t('message.gameAreaMerge.reason')" />
          <Column :header="t('message.gameAreaMerge.actions')">
            <template #body="{ data }">
              <Button v-auth="'gameAreaMerge:detail'" :label="t('message.gameAreaMerge.detail')" text @click="openDetail(data.id)" />
              <Button v-if="data.cancellable === true" v-auth="'gameAreaMerge:cancel'" :label="t('message.gameAreaMerge.cancel')" text severity="danger" @click="openCancel(data)" />
            </template>
          </Column>
          <template #empty>{{ t('message.gameAreaMerge.empty') }}</template>
        </DataTable>
        <Paginator :first="(pageNumber - 1) * pageSize" :rows="pageSize" :totalRecords="total" @page="changePage" />
      </template>
    </Card>

    <Dialog v-model:visible="creating" modal :header="t('message.gameAreaMerge.create')" class="merge-dialog" :style="{ width: 'min(42rem, 95vw)' }">
      <form class="merge-form" @submit.prevent="createPlan">
        <Message severity="info" :closable="false">{{ t('message.gameAreaMerge.createHint') }}</Message>
        <label for="merge-target">{{ t('message.gameAreaMerge.target') }}</label>
        <Select inputId="merge-target" v-model="targetServerId" :options="availableServers" optionLabel="name" optionValue="id" :placeholder="t('message.gameAreaMerge.chooseTarget')" filter />
        <label for="merge-sources">{{ t('message.gameAreaMerge.sources') }}</label>
        <MultiSelect inputId="merge-sources" v-model="sourceServerIds" :options="sourceOptions" optionLabel="name" optionValue="id" :placeholder="t('message.gameAreaMerge.chooseSources')" filter />
        <label for="merge-create-reason">{{ t('message.gameAreaMerge.reason') }}</label>
        <Textarea id="merge-create-reason" v-model="createReason" required maxlength="200" rows="3" />
        <Button type="submit" :label="t('message.gameAreaMerge.generate')" :loading="busy" :disabled="!targetServerId || sourceServerIds.length === 0 || !createReason.trim()" />
      </form>
    </Dialog>

    <Dialog v-model:visible="detailsVisible" modal :header="t('message.gameAreaMerge.detail')" :style="{ width: 'min(64rem, 96vw)' }">
      <div v-if="selected" class="merge-form" data-testid="merge-detail">
        <div class="flex justify-content-between align-items-center">
          <strong>{{ selected.planId }}</strong>
          <Tag data-testid="merge-status" :value="statusLabels[selected.status]" />
        </div>
        <p>{{ t('message.gameAreaMerge.target') }}: {{ selected.targetServerId }} · {{ t('message.gameAreaMerge.sources') }}: {{ selected.sourceServerIds }}</p>
        <p>{{ t('message.gameAreaMerge.applicant') }}: {{ selected.applicantOperatorId }} · {{ t('message.gameAreaMerge.reviewer') }}: {{ selected.reviewerOperatorId ?? '—' }}</p>
        <p>{{ t('message.gameAreaMerge.reason') }}: {{ selected.reason }}</p>
        <p v-if="selected.cancellationReason">{{ t('message.gameAreaMerge.cancelReason') }}: {{ selected.cancellationReason }}</p>
        <p v-if="selected.backupIdentifier">{{ t('message.gameAreaMerge.backup') }}: {{ selected.backupIdentifier }}</p>
        <Message v-if="selected.status === 3" severity="info" :closable="false">{{ t('message.gameAreaMerge.runningHint') }}</Message>
        <Message v-if="selected.failureMessage" severity="error" :closable="false">{{ t('message.gameAreaMerge.failureHint') }}</Message>
        <div v-for="report in reports" :key="report.title" class="merge-report" data-testid="merge-report">
          <h3>{{ report.title }}</h3>
          <Message :severity="report.globalDomainRowsTouched === 0 ? 'success' : 'error'" :closable="false">
            {{ t('message.gameAreaMerge.globalRows') }}: <strong>{{ report.globalDomainRowsTouched }}</strong>
          </Message>
          <Message v-if="report.alreadyExecuted" severity="info" :closable="false">{{ t('message.gameAreaMerge.replayed') }}</Message>
          <DataTable :value="report.steps">
            <Column field="step" :header="t('message.gameAreaMerge.step')" />
            <Column field="affectedCount" :header="t('message.gameAreaMerge.affected')" />
            <Column field="details" :header="t('message.gameAreaMerge.details')" />
          </DataTable>
        </div>
        <Button v-auth="'gameAreaMerge:detail'" :label="t('message.gameAreaMerge.refresh')" :loading="busy" @click="openDetail(selected.id)" />
        <Button v-if="selected.cancellable === true" v-auth="'gameAreaMerge:cancel'" severity="danger" :label="t('message.gameAreaMerge.cancel')" :loading="busy" @click="openCancel(selected)" />
        <Button v-if="selected.status === 0 || selected.status === 5" v-auth="'gameAreaMerge:dryRun'" :label="t('message.gameAreaMerge.dryRun')" :loading="busy" @click="dryRun" />
        <div v-if="selected.status === 1" class="merge-form">
          <Message v-if="!canApprove" severity="warn" :closable="false">{{ t('message.gameAreaMerge.reviewHint') }}</Message>
          <label for="merge-reviewer">{{ t('message.gameAreaMerge.reviewer') }}</label>
          <InputText id="merge-reviewer" :modelValue="String(currentOperatorId)" readonly />
          <label for="merge-review-reason">{{ t('message.gameAreaMerge.reason') }}</label>
          <Textarea id="merge-review-reason" v-model="reviewReason" maxlength="200" rows="2" />
          <Button v-auth="'gameAreaMerge:approve'" :label="t('message.gameAreaMerge.approve')" :disabled="!canApprove || !reviewReason.trim()" :loading="busy" @click="approve" />
        </div>
        <div v-if="selected.status === 2 || selected.executionReclaimable === true" class="merge-form">
          <label for="merge-backup">{{ t('message.gameAreaMerge.backup') }}</label>
          <InputText id="merge-backup" v-model="backupIdentifier" maxlength="200" />
          <div class="flex gap-2 align-items-center">
            <Checkbox inputId="merge-backup-confirmed" v-model="backupConfirmed" binary />
            <label for="merge-backup-confirmed">{{ t('message.gameAreaMerge.backupConfirmed') }}</label>
          </div>
          <Button v-auth="'gameAreaMerge:execute'" severity="danger" :label="selected.executionReclaimable === true ? t('message.gameAreaMerge.reexecute') : t('message.gameAreaMerge.execute')" :disabled="!backupConfirmed || !backupIdentifier.trim()" :loading="busy" @click="execute" />
        </div>
      </div>
    </Dialog>

    <Dialog v-model:visible="cancellationVisible" modal :header="t('message.gameAreaMerge.cancel')" :style="{ width: 'min(34rem, 95vw)' }">
      <div v-if="selected" class="merge-form">
        <p>{{ selected.planId }}</p>
        <label for="merge-cancellation-reason">{{ t('message.gameAreaMerge.cancelReason') }}</label>
        <Textarea id="merge-cancellation-reason" v-model="cancellationReason" required maxlength="200" rows="3" />
        <Button v-auth="'gameAreaMerge:cancel'" severity="danger" :label="t('message.gameAreaMerge.cancel')" :disabled="!cancellationReason.trim()" :loading="busy" @click="cancel" />
      </div>
    </Dialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import type { PageState } from 'primevue/paginator'
import { gameAreaMergeAddPost, gameAreaMergeApprovePost, gameAreaMergeCancelPost, gameAreaMergeDetailGet, gameAreaMergeDryRunPost, gameAreaMergeExecutePost, gameAreaMergePagePost } from '../../../api/gameAreaMerge'
import type { GameAreaMergePlanResponse } from '../../../api/gameAreaMerge/types'
import { useServerStore } from '../../../stores/server'
import { useUserInfoStore } from '../../../stores/userInfo'
import { showConfirm, showSuccess } from '../../../utils/message'

defineOptions({ name: 'GameAreaMerge' })

const serverStore = useServerStore()
const userStore = useUserInfoStore()
const plans = ref<GameAreaMergePlanResponse[]>([])
const selected = ref<GameAreaMergePlanResponse>()
const loading = ref(false)
const busy = ref(false)
const creating = ref(false)
const detailsVisible = ref(false)
const cancellationVisible = ref(false)
const pageNumber = ref(1)
const pageSize = 20
const total = ref(0)
const targetServerId = ref('')
const sourceServerIds = ref<string[]>([])
const createReason = ref('')
const reviewReason = ref('')
const cancellationReason = ref('')
const backupIdentifier = ref('')
const backupConfirmed = ref(false)
const currentOperatorId = computed(() => Number(userStore.userInfos.id))
const availableServers = computed(() => serverStore.servers)
const sourceOptions = computed(() => availableServers.value.filter(server => server.id !== targetServerId.value))
watch(targetServerId, identifier => { sourceServerIds.value = sourceServerIds.value.filter(source => source !== identifier) })
const statusLabels = computed(() => [t('message.gameAreaMerge.draft'), t('message.gameAreaMerge.rehearsed'), t('message.gameAreaMerge.approved'), t('message.gameAreaMerge.running'), t('message.gameAreaMerge.completed'), t('message.gameAreaMerge.failed'), t('message.gameAreaMerge.cancelled')])

interface ReportStep { step: string; affectedCount: number; details: string }
interface Report { title: string; globalDomainRowsTouched: number; alreadyExecuted: boolean; steps: ReportStep[] }
function parseReport(snapshot: string | undefined, title: string): Report | undefined {
  if (!snapshot) return undefined
  const report = JSON.parse(snapshot) as Record<string, unknown>
  const steps = (report.Steps ?? report.steps ?? []) as Record<string, unknown>[]
  return {
    title,
    globalDomainRowsTouched: Number(report.GlobalDomainRowsTouched ?? report.globalDomainRowsTouched),
    alreadyExecuted: Boolean(report.AlreadyExecuted ?? report.alreadyExecuted),
    steps: steps.map(step => ({ step: String(step.Step ?? step.step), affectedCount: Number(step.AffectedCount ?? step.affectedCount), details: JSON.stringify(step.Details ?? step.details ?? {}) })),
  }
}
const reports = computed(() => [
  parseReport(selected.value?.dryRunReportSnapshot, t('message.gameAreaMerge.dryRunReport')),
  parseReport(selected.value?.executionReportSnapshot, t('message.gameAreaMerge.executionReport')),
].filter((report): report is Report => !!report))
const canApprove = computed(() => currentOperatorId.value > 0 && currentOperatorId.value !== selected.value?.applicantOperatorId && reports.value[0]?.globalDomainRowsTouched === 0)

async function loadPlans() {
  loading.value = true
  try {
    const response = await gameAreaMergePagePost({ page: pageNumber.value, pageSize })
    plans.value = response.data.data.items ?? []
    total.value = response.data.data.total ?? 0
  } finally { loading.value = false }
}
async function changePage(event: PageState) { pageNumber.value = event.page + 1; await loadPlans() }
async function openCreate() {
  await serverStore.loadServers(true)
  targetServerId.value = ''; sourceServerIds.value = []; createReason.value = ''; creating.value = true
}
async function openDetail(identifier: number) {
  selected.value = (await gameAreaMergeDetailGet({ id: identifier })).data.data
  detailsVisible.value = true
  reviewReason.value = ''; backupIdentifier.value = selected.value.backupIdentifier ?? ''; backupConfirmed.value = selected.value.backupConfirmed === true
}
function openCancel(plan: GameAreaMergePlanResponse) {
  selected.value = plan
  cancellationReason.value = ''
  cancellationVisible.value = true
}
async function perform(action: () => Promise<void>) {
  if (busy.value) return
  busy.value = true
  try { await action(); await loadPlans() } finally { busy.value = false }
}
async function createPlan() {
  await perform(async () => {
    selected.value = (await gameAreaMergeAddPost({ targetServerId: Number(targetServerId.value), sourceServerIds: sourceServerIds.value.map(Number), reason: createReason.value })).data.data
    creating.value = false; detailsVisible.value = true
    reviewReason.value = ''; backupIdentifier.value = ''; backupConfirmed.value = false
  })
}
async function dryRun() {
  if (!selected.value) return
  const identifier = selected.value.id
  await perform(async () => { selected.value = (await gameAreaMergeDryRunPost({ id: identifier })).data.data })
}
async function approve() {
  if (!selected.value) return
  const identifier = selected.value.id
  await perform(async () => { selected.value = (await gameAreaMergeApprovePost({ id: identifier, reviewerOperatorId: currentOperatorId.value, reason: reviewReason.value })).data.data })
}
async function execute() {
  if (!selected.value || !await showConfirm(t('message.gameAreaMerge.executeConfirm'), t('message.gameAreaMerge.execute'))) return
  const identifier = selected.value.id
  await perform(async () => {
    selected.value = (await gameAreaMergeExecutePost({ id: identifier, backupIdentifier: backupIdentifier.value, backupConfirmed: backupConfirmed.value })).data.data
    await serverStore.loadServers(true)
    showSuccess(t('message.gameAreaMerge.completed'))
  })
}
async function cancel() {
  if (!selected.value || !cancellationReason.value.trim() || !await showConfirm(t('message.gameAreaMerge.cancelConfirm'), t('message.gameAreaMerge.cancel'))) return
  const identifier = selected.value.id
  await perform(async () => {
    selected.value = (await gameAreaMergeCancelPost({ id: identifier, reason: cancellationReason.value })).data.data
    cancellationVisible.value = false
    showSuccess(t('message.gameAreaMerge.cancelled'))
  })
}
onMounted(loadPlans)
</script>

<style scoped>
.merge-page { padding: 1rem; }
.merge-form { display: flex; flex-direction: column; gap: 0.75rem; }
.merge-report { border-top: 1px solid var(--surface-border); padding-top: 0.75rem; }
h2, h3, p { margin: 0 0 0.5rem; }
</style>
