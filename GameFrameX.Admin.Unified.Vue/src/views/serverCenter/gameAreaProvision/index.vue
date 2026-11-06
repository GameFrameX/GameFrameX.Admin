<template>
  <div class="gameAreaProvision-container">
    <Card>
      <template #title>{{ t('message.gameAreaProvision.title') }}</template>
      <template #content>
        <p class="text-color-secondary">{{ t('message.gameAreaProvision.hint') }}</p>
        <Steps :model="steps" :activeStep="activeStep" readonly class="mb-4" />
        <Message v-if="failure" severity="error" :closable="false" class="mb-3" role="alert">{{ failure }}</Message>
        <form class="provision-form" @submit.prevent="startProvisioning">
          <div class="field">
            <label for="provision-template">{{ t('message.gameAreaProvision.template') }}</label>
            <Select inputId="provision-template" v-model="templateId" :options="templates" optionLabel="name" optionValue="id"
                    :disabled="busy || !!status" class="w-full" @change="previewTemplate" />
          </div>
          <div class="field">
            <label for="provision-server-id">{{ t('message.gameAreaProvision.fields.ServerId') }}</label>
            <InputText id="provision-server-id" v-model="serverIdentifier" inputmode="numeric" required :disabled="busy || !!status" class="w-full" />
          </div>
          <div v-for="field in editableFields" :key="field.key" class="field">
            <label :for="`provision-${field.key}`">{{ t(field.label) }}</label>
            <Textarea v-if="field.key === 'serverParameters'" :id="`provision-${field.key}`" v-model="form[field.key]" rows="4"
                      required :disabled="busy || !!status" class="w-full" />
            <InputText v-else :id="`provision-${field.key}`" v-model="form[field.key]" required :disabled="busy || !!status" class="w-full" />
          </div>
          <div v-if="preview" class="provision-preview">
            <h3>{{ t('message.gameAreaProvision.cloneTitle') }}</h3>
            <dl>
              <template v-for="(value, key) in preview.clonedFields" :key="key">
                <dt>{{ t(`message.gameAreaProvision.fields.${key}`) }}</dt><dd>{{ value || '—' }}</dd>
              </template>
            </dl>
            <p>{{ t('message.gameAreaProvision.resetHint') }}</p>
          </div>
          <div class="flex flex-wrap gap-2">
            <Button v-if="!status" v-auth="'gameAreaProvision:create'" type="submit" :label="t('message.gameAreaProvision.start')" :icon="faPlay"
                    :loading="busy" :disabled="!preview || !canExecute" />
            <Button v-if="status && !status.isEnabled" v-auth="'gameAreaProvision:execute'" type="button" :label="t('message.gameAreaProvision.retry')"
                    :icon="faRotateRight" :loading="busy" @click="executeProvisioning" />
            <Button v-auth="'gameAreaProvision:status'" type="button" :label="t('message.gameAreaProvision.query')" :icon="faMagnifyingGlass"
                    severity="secondary" :disabled="busy || !serverIdentifier" @click="refreshStatus" />
            <Button type="button" :label="t('message.gameAreaProvision.newServer')" severity="secondary" :disabled="busy" @click="resetWizard" />
          </div>
        </form>
        <section v-if="status" class="mt-4" aria-live="polite" data-testid="provision-result">
          <h3>{{ status.name }} · {{ status.serverId }}</h3>
          <ul>
            <li v-for="step in stepStates" :key="step.key">{{ t(step.label) }}: {{ t(step.done ? 'message.gameAreaProvision.complete' : 'message.gameAreaProvision.pending') }}</li>
          </ul>
          <Message v-if="status.isEnabled && status.registered" severity="success" :closable="false">{{ t('message.gameAreaProvision.success') }}</Message>
        </section>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts" name="gameAreaProvision">
import { computed, onMounted, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { isAxiosError } from 'axios';
import Card from 'primevue/card';
import Steps from 'primevue/steps';
import Select from 'primevue/select';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Button from 'primevue/button';
import Message from 'primevue/message';
import { gameAreaProvisionCreatePost, gameAreaProvisionExecutePost, gameAreaProvisionPreviewGet, gameAreaProvisionStatusGet,
  gameAreaProvisionTemplatesGet, type GameAreaProvisionPreviewResponse, type GameAreaProvisionStatusResponse,
  type GameAreaProvisionTemplateResponse } from '../../../api/gameAreaProvision';
import { useServerStore } from '../../../stores/server';
import { faMagnifyingGlass, faPlay, faRotateRight } from '../../../utils/icons';
import { auth } from '../../../utils/authFunction';

defineOptions({ name: 'gameAreaProvision' });
const { t } = useI18n();
const serverStore = useServerStore();
const templates = ref<GameAreaProvisionTemplateResponse[]>([]);
const templateId = ref<number>();
const preview = ref<GameAreaProvisionPreviewResponse>();
const status = ref<GameAreaProvisionStatusResponse>();
const busy = ref(false);
const failure = ref('');
const serverIdentifier = ref('');
const storageKey = 'gameAreaProvisionServerId';
const form = reactive({ name: '', httpManageUrl: '', tcpSocketUrl: '', webSocketUrl: '', serverParameters: '{}' });
const canExecute = computed(() => auth('gameAreaProvision:execute'));
const editableFields = [
  { key: 'name', label: 'message.gameAreaProvision.fields.Name' },
  { key: 'httpManageUrl', label: 'message.gameAreaProvision.fields.HttpManageUrl' },
  { key: 'tcpSocketUrl', label: 'message.gameAreaProvision.fields.TcpSocketUrl' },
  { key: 'webSocketUrl', label: 'message.gameAreaProvision.fields.WebSocketUrl' },
  { key: 'serverParameters', label: 'message.gameAreaProvision.fields.ServerParameters' },
] as const;
const stepStates = computed(() => [
  { key: 'record', label: 'message.gameAreaProvision.record', done: status.value?.recordCreated },
  { key: 'container', label: 'message.gameAreaProvision.container', done: status.value?.containerCreated },
  { key: 'registration', label: 'message.gameAreaProvision.registration', done: status.value?.registered },
  { key: 'enabled', label: 'message.gameAreaProvision.enabled', done: status.value?.isEnabled },
]);
const steps = computed(() => stepStates.value.map(step => ({ label: t(step.label) })));
const activeStep = computed(() => Math.min(stepStates.value.filter(step => step.done).length, 3));
function errorMessage(error: unknown): string {
  if (isAxiosError(error)) {
    const payload = error.response?.data as { message?: string; Message?: string; errorCode?: string; ErrorCode?: string; code?: number; Code?: number } | undefined;
    const reason = payload?.message ?? payload?.Message;
    if (reason) return `${payload?.code ?? payload?.Code ?? payload?.errorCode ?? payload?.ErrorCode ?? ''} ${reason}`.trim();
  }
  if (error instanceof Error) {
    const code = (error as Error & { code?: unknown }).code;
    return `${typeof code === 'number' ? code : ''} ${error.message}`.trim();
  }
  return t('message.gameAreaProvision.failed');
}
function rememberIdentifier(): void {
  try { window.localStorage.setItem(storageKey, serverIdentifier.value); } catch { /* 浏览器禁用存储时仍可按编号查询。 */ }
}
async function previewTemplate(): Promise<void> {
  preview.value = undefined;
  failure.value = '';
  if (!templateId.value) return;
  busy.value = true;
  try { preview.value = (await gameAreaProvisionPreviewGet({ id: templateId.value })).data.data; }
  catch (error) { failure.value = errorMessage(error); }
  finally { busy.value = false; }
}
async function startProvisioning(): Promise<void> {
  const identifier = Number(serverIdentifier.value);
  if (!templateId.value || !Number.isSafeInteger(identifier) || identifier <= 0) {
    failure.value = t('message.gameAreaProvision.invalidIdentifier');
    return;
  }
  busy.value = true;
  failure.value = '';
  rememberIdentifier();
  try {
    status.value = (await gameAreaProvisionCreatePost({ templateId: templateId.value, serverId: identifier, ...form })).data.data;
    await executeProvisioning();
  } catch (error) { failure.value = errorMessage(error); }
  finally { busy.value = false; }
}
async function executeProvisioning(): Promise<void> {
  if (!status.value) return;
  busy.value = true;
  failure.value = '';
  try {
    status.value = (await gameAreaProvisionExecutePost({ id: status.value.serverId })).data.data;
    if (status.value.isEnabled) await serverStore.loadServers(true);
  } catch (error) {
    failure.value = errorMessage(error);
    // 查询只读事实，不自动重试写操作；保留首个失败原因供运营处理。
    try { status.value = (await gameAreaProvisionStatusGet({ id: status.value.serverId })).data.data; } catch { /* 保留已知步骤。 */ }
  } finally { busy.value = false; }
}
async function refreshStatus(): Promise<void> {
  const identifier = Number(serverIdentifier.value);
  if (!Number.isSafeInteger(identifier) || identifier <= 0) {
    failure.value = t('message.gameAreaProvision.invalidIdentifier');
    return;
  }
  busy.value = true;
  failure.value = '';
  try {
    status.value = (await gameAreaProvisionStatusGet({ id: identifier })).data.data;
    rememberIdentifier();
    if (status.value.isEnabled) await serverStore.loadServers(true);
  } catch (error) { failure.value = errorMessage(error); }
  finally { busy.value = false; }
}
function resetWizard(): void {
  status.value = undefined;
  serverIdentifier.value = '';
  failure.value = '';
  try { window.localStorage.removeItem(storageKey); } catch { /* 允许无存储环境。 */ }
}
onMounted(async () => {
  try {
    templates.value = (await gameAreaProvisionTemplatesGet()).data.data;
    try { serverIdentifier.value = window.localStorage.getItem(storageKey) || ''; } catch { /* 允许无存储环境。 */ }
    if (serverIdentifier.value && auth('gameAreaProvision:status')) await refreshStatus();
  } catch (error) { failure.value = errorMessage(error); }
});
</script>

<style scoped>
.provision-form { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; }
.field label { display: block; margin-bottom: .5rem; }
.provision-preview, .provision-form > .flex { grid-column: 1 / -1; }
dl { display: grid; grid-template-columns: minmax(10rem, 1fr) 2fr; gap: .5rem 1rem; }
dd { margin: 0; overflow-wrap: anywhere; }
@media (max-width: 640px) { .provision-form { grid-template-columns: 1fr; } }
</style>
