<template>
  <div class="platform-logging-config-container">
    <Card>
      <template #title>
        <div class="page-title">
          <i class="fa-solid fa-file-lines"></i>
          <span>{{ t('platformLoggingConfig.title') }}</span>
        </div>
      </template>
      <template #content>
        <Message severity="info" :closable="false" class="mb-4">
          {{ t('platformLoggingConfig.saveHint') }}
        </Message>

        <!-- 文件日志 -->
        <h3 class="section-title">{{ t('platformLoggingConfig.groupFile') }}</h3>
        <div class="form-grid">
          <div class="field switch-field">
            <label for="file-enabled">{{ t('platformLoggingConfig.enabled') }}</label>
            <ToggleSwitch id="file-enabled" v-model="form.file.enabled"/>
          </div>
          <div class="field field-wide">
            <label for="file-fileName">{{ t('platformLoggingConfig.fileName') }}</label>
            <InputText id="file-fileName" v-model="form.file.fileName" class="w-full" maxlength="4096"/>
          </div>
          <div class="field switch-field">
            <label for="file-append">{{ t('platformLoggingConfig.append') }}</label>
            <ToggleSwitch id="file-append" v-model="form.file.append"/>
          </div>
          <div class="field">
            <label for="file-fileSizeLimitBytes">{{ t('platformLoggingConfig.fileSizeLimitBytes') }}</label>
            <InputNumber id="file-fileSizeLimitBytes" v-model="form.file.fileSizeLimitBytes" class="w-full" :min="1024" :max="1073741824" :useGrouping="false"/>
          </div>
          <div class="field">
            <label for="file-maxRollingFiles">{{ t('platformLoggingConfig.maxRollingFiles') }}</label>
            <InputNumber id="file-maxRollingFiles" v-model="form.file.maxRollingFiles" class="w-full" :min="1" :max="365" :useGrouping="false"/>
          </div>
        </div>

        <!-- 数据库日志 -->
        <h3 class="section-title">{{ t('platformLoggingConfig.groupDatabase') }}</h3>
        <div class="form-grid">
          <div class="field switch-field">
            <label for="db-enabled">{{ t('platformLoggingConfig.enabled') }}</label>
            <ToggleSwitch id="db-enabled" v-model="form.database.enabled"/>
          </div>
          <div class="field">
            <label for="db-minimumLevel">{{ t('platformLoggingConfig.minimumLevel') }}</label>
            <Select id="db-minimumLevel" v-model="form.database.minimumLevel" :options="logLevelOptions" optionLabel="label" optionValue="value" filter class="w-full"/>
          </div>
        </div>

        <!-- ElasticSearch -->
        <h3 class="section-title">{{ t('platformLoggingConfig.groupElasticSearch') }}</h3>
        <div class="form-grid">
          <div class="field switch-field">
            <label for="es-enabled">{{ t('platformLoggingConfig.enabled') }}</label>
            <ToggleSwitch id="es-enabled" v-model="form.elasticSearch.enabled"/>
          </div>
          <div class="field">
            <label for="es-defaultIndex">{{ t('platformLoggingConfig.defaultIndex') }}</label>
            <InputText id="es-defaultIndex" v-model="form.elasticSearch.defaultIndex" class="w-full" maxlength="128"/>
          </div>
          <div class="field field-wide">
            <label for="es-serverUris">{{ t('platformLoggingConfig.serverUris') }}</label>
            <div class="es-masked">
              <Tag v-if="esUriCount > 0" severity="warn" :value="t('platformLoggingConfig.serverUrisMasked', {count: esUriCount})"/>
              <Tag v-else severity="secondary" :value="t('platformLoggingConfig.serverUrisMaskedEmpty')"/>
            </div>
            <Textarea id="es-serverUris" v-model="esServerUrisInput" rows="3" class="w-full" :placeholder="t('platformLoggingConfig.serverUrisEditHint')"/>
          </div>
        </div>

        <!-- 监控采集 -->
        <h3 class="section-title">{{ t('platformLoggingConfig.groupMonitor') }}</h3>
        <div class="form-grid">
          <div class="field switch-field">
            <label for="mon-globalEnabled">{{ t('platformLoggingConfig.globalEnabled') }}</label>
            <ToggleSwitch id="mon-globalEnabled" v-model="form.monitor.globalEnabled"/>
          </div>
          <div class="field">
            <label for="mon-bahLogLevel">{{ t('platformLoggingConfig.bahLogLevel') }}</label>
            <Select id="mon-bahLogLevel" v-model="form.monitor.bahLogLevel" :options="logLevelOptions" optionLabel="label" optionValue="value" filter class="w-full"/>
          </div>
          <div class="field switch-field">
            <label for="mon-withReturnValue">{{ t('platformLoggingConfig.withReturnValue') }}</label>
            <ToggleSwitch id="mon-withReturnValue" v-model="form.monitor.withReturnValue"/>
          </div>
          <div class="field">
            <label for="mon-returnValueThreshold">{{ t('platformLoggingConfig.returnValueThreshold') }}</label>
            <InputNumber id="mon-returnValueThreshold" v-model="form.monitor.returnValueThreshold" class="w-full" :min="0" :max="100000" :useGrouping="false"/>
          </div>
          <div class="field">
            <label for="mon-jsonBehavior">{{ t('platformLoggingConfig.jsonBehavior') }}</label>
            <Select id="mon-jsonBehavior" v-model="form.monitor.jsonBehavior" :options="jsonBehaviorOptions" optionLabel="label" optionValue="value" filter class="w-full"/>
          </div>
          <div class="field switch-field">
            <label for="mon-jsonIndented">{{ t('platformLoggingConfig.jsonIndented') }}</label>
            <ToggleSwitch id="mon-jsonIndented" v-model="form.monitor.jsonIndented"/>
          </div>
          <div class="field switch-field">
            <label for="mon-useUtcTimestamp">{{ t('platformLoggingConfig.useUtcTimestamp') }}</label>
            <ToggleSwitch id="mon-useUtcTimestamp" v-model="form.monitor.useUtcTimestamp"/>
          </div>
        </div>

        <Divider/>

        <div class="readonly-grid">
          <div>
            <span>{{ t('platformLoggingConfig.updateTime') }}</span>
            <strong>{{ updateTimeText }}</strong>
          </div>
        </div>

        <div class="actions">
          <Button severity="secondary" :loading="loading" @click="loadConfig">
            <i class="fa-solid fa-arrows-rotate mr-1"></i>
            {{ t('common.button.refresh') }}
          </Button>
          <Button v-auth="'platformLoggingConfig:update'" :loading="saving" @click="saveConfig">
            <i class="fa-solid fa-floppy-disk mr-1"></i>
            {{ t('common.button.save') }}
          </Button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="platformLoggingConfig">
import {computed, onMounted, reactive, ref} from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Divider from 'primevue/divider'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import { platformLoggingConfigDetailPost, platformLoggingConfigUpdatePost } from '../../../../api/platformLoggingConfig'
import type {PlatformLoggingConfigUpdateInput} from '../../../api/global/platformLoggingConfig'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {showError, showSuccess} from '../../../../utils/message.ts'

const loading = ref(false)
const saving = ref(false)

const form = reactive({
  file: {
    enabled: true,
    fileName: 'data/logs/{0:yyyyMMdd}_{1}.log',
    append: true,
    fileSizeLimitBytes: 10485760,
    maxRollingFiles: 30,
  },
  database: {
    enabled: true,
    minimumLevel: 'Information',
  },
  elasticSearch: {
    enabled: false,
    serverUris: [] as string[],
    defaultIndex: 'adminnet',
  },
  monitor: {
    globalEnabled: true,
    bahLogLevel: 'Information',
    withReturnValue: true,
    returnValueThreshold: 500,
    jsonBehavior: 'OnlyJson',
    jsonIndented: false,
    useUtcTimestamp: false,
  },
})

const esUriCount = ref(0)
const esServerUrisInput = ref('')
const updateTime = ref<number | null>(null)

const logLevelOptions = computed(() => [
  {label: t('platformLoggingConfig.levelTrace'), value: 'Trace'},
  {label: t('platformLoggingConfig.levelDebug'), value: 'Debug'},
  {label: t('platformLoggingConfig.levelInformation'), value: 'Information'},
  {label: t('platformLoggingConfig.levelWarning'), value: 'Warning'},
  {label: t('platformLoggingConfig.levelError'), value: 'Error'},
  {label: t('platformLoggingConfig.levelCritical'), value: 'Critical'},
  {label: t('platformLoggingConfig.levelNone'), value: 'None'},
])

const jsonBehaviorOptions = computed(() => [
  {label: t('platformLoggingConfig.jsonBehaviorNone'), value: 'None'},
  {label: t('platformLoggingConfig.jsonBehaviorOnlyJson'), value: 'OnlyJson'},
])

const updateTimeText = computed(() => (updateTime.value ? formatUnix(updateTime.value) : '-'))

const loadConfig = async () => {
  loading.value = true
  try {
    const res = await platformLoggingConfigDetailPost()
    const data = res.data.data || {}
    if (data.file) Object.assign(form.file, data.file)
    if (data.database) Object.assign(form.database, data.database)
    if (data.elasticSearch) Object.assign(form.elasticSearch, data.elasticSearch)
    if (data.monitor) Object.assign(form.monitor, data.monitor)
    esUriCount.value = form.elasticSearch.serverUris?.length || 0
    esServerUrisInput.value = ''
    updateTime.value = data.updateTime ?? null
  } catch {
    showError(t('platformLoggingConfig.loadFailed'))
  } finally {
    loading.value = false
  }
}

const validateForm = () => {
  if (!form.file.fileName || !form.file.fileName.trim()) {
    showError(t('platformLoggingConfig.fileNameInvalid'))
    return false
  }
  if (form.file.fileSizeLimitBytes < 1024 || form.file.fileSizeLimitBytes > 1073741824) {
    showError(t('platformLoggingConfig.fileSizeLimitBytesInvalid'))
    return false
  }
  if (form.file.maxRollingFiles < 1 || form.file.maxRollingFiles > 365) {
    showError(t('platformLoggingConfig.maxRollingFilesInvalid'))
    return false
  }
  if (form.monitor.returnValueThreshold < 0 || form.monitor.returnValueThreshold > 100000) {
    showError(t('platformLoggingConfig.returnValueThresholdInvalid'))
    return false
  }
  return true
}

const buildUpdatePayload = (): PlatformLoggingConfigUpdateInput => {
  const trimmed = esServerUrisInput.value.trim()
  // 留空 → 保留掩码占位，后端识别全 "***" 为未变更；非空 → 解析为新 URI 数组替换。
  const serverUris = trimmed
    ? trimmed.split(/[\s,]+/).map((s) => s.trim()).filter(Boolean)
    : [...form.elasticSearch.serverUris]
  return {
    file: {...form.file},
    database: {...form.database},
    elasticSearch: {
      enabled: form.elasticSearch.enabled,
      serverUris,
      defaultIndex: form.elasticSearch.defaultIndex,
    },
    monitor: {...form.monitor},
  }
}

const saveConfig = async () => {
  if (!validateForm()) return
  saving.value = true
  try {
    await platformLoggingConfigUpdatePost(buildUpdatePayload())
    showSuccess(t('platformLoggingConfig.saveSuccess'))
    await loadConfig()
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped lang="scss">
.platform-logging-config-container {
  padding: 8px;
}

.page-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
}

.section-title {
  margin: 18px 0 10px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-color);
  border-left: 3px solid var(--p-primary-color);
  padding-left: 8px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 16px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.field-wide {
  grid-column: 1 / -1;
}

.switch-field {
  justify-content: flex-end;
}

.field label,
.readonly-grid span {
  color: var(--p-text-muted-color);
  font-size: 12px;
}

.es-masked {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.readonly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}

.readonly-grid > div {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}
</style>
