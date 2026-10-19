<template>
  <div class="platform-rate-limit-config-container">
    <Card>
      <template #title>
        <div class="page-title">
          <i class="fa-solid fa-gauge-high"></i>
          <span>{{ t('platformRateLimitConfig.title') }}</span>
        </div>
      </template>
      <template #content>
        <Message severity="info" :closable="false" class="mb-4">
          {{ t('platformRateLimitConfig.saveHint') }}
        </Message>

        <div class="form-grid">
          <div class="field switch-field">
            <label for="enabled">{{ t('platformRateLimitConfig.enabled') }}</label>
            <ToggleSwitch id="enabled" v-model="form.enabled"/>
          </div>

          <div class="field">
            <label for="permitLimit">{{ t('platformRateLimitConfig.permitLimit') }}</label>
            <InputNumber id="permitLimit" v-model="form.permitLimit" class="w-full" :min="1" :max="100000" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="windowSeconds">{{ t('platformRateLimitConfig.windowSeconds') }}</label>
            <InputNumber id="windowSeconds" v-model="form.windowSeconds" class="w-full" :min="1" :max="3600" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="queueLimit">{{ t('platformRateLimitConfig.queueLimit') }}</label>
            <InputNumber id="queueLimit" v-model="form.queueLimit" class="w-full" :min="0" :max="100000" :useGrouping="false"/>
          </div>
        </div>

        <Divider/>

        <div class="readonly-grid">
          <div>
            <span>{{ t('platformRateLimitConfig.version') }}</span>
            <strong>{{ detail.version || '-' }}</strong>
          </div>
          <div>
            <span>{{ t('platformRateLimitConfig.currentStatus') }}</span>
            <Tag :severity="form.enabled ? 'success' : 'secondary'" :value="form.enabled ? t('platformRateLimitConfig.on') : t('platformRateLimitConfig.off')"/>
          </div>
        </div>

        <div class="actions">
          <Button severity="secondary" :loading="loading" @click="loadConfig">
            <i class="fa-solid fa-arrows-rotate mr-1"></i>
            {{ t('common.button.refresh') }}
          </Button>
          <Button v-auth="'platformRateLimitConfig:update'" :loading="saving" @click="saveConfig">
            <i class="fa-solid fa-floppy-disk mr-1"></i>
            {{ t('common.button.save') }}
          </Button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="platformRateLimitConfig">
import {onMounted, reactive, ref} from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Divider from 'primevue/divider'
import InputNumber from 'primevue/inputnumber'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import ToggleSwitch from 'primevue/toggleswitch'
import {platformRateLimitConfigDetailPost, platformRateLimitConfigUpdatePost} from '../../../../api/platformRateLimitConfig';
import {showError, showSuccess} from '../../../../utils/message.ts'

const loading = ref(false)
const saving = ref(false)

const form = reactive({
  enabled: true,
  permitLimit: 20,
  windowSeconds: 1,
  queueLimit: 0,
})

const detail = reactive({
  version: 0,
})

const loadConfig = async () => {
  loading.value = true
  try {
    const res = await platformRateLimitConfigDetailPost()
    const data = res.data.data || {}
    Object.assign(form, {
      enabled: data.enabled ?? true,
      permitLimit: data.permitLimit ?? 20,
      windowSeconds: data.windowSeconds ?? 1,
      queueLimit: data.queueLimit ?? 0,
    })
    detail.version = data.version || 0
  } finally {
    loading.value = false
  }
}

const validateForm = () => {
  if (form.permitLimit < 1 || form.permitLimit > 100000) {
    showError(t('platformRateLimitConfig.permitLimitInvalid'))
    return false
  }
  if (form.windowSeconds < 1 || form.windowSeconds > 3600) {
    showError(t('platformRateLimitConfig.windowSecondsInvalid'))
    return false
  }
  if (form.queueLimit < 0 || form.queueLimit > 100000) {
    showError(t('platformRateLimitConfig.queueLimitInvalid'))
    return false
  }
  return true
}

const saveConfig = async () => {
  if (!validateForm()) return
  saving.value = true
  try {
    await platformRateLimitConfigUpdatePost({...form})
    showSuccess(t('platformRateLimitConfig.saveSuccess'))
    await loadConfig()
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped lang="scss">
.platform-rate-limit-config-container {
  padding: 8px;
}

.page-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.switch-field {
  justify-content: flex-end;
}

.field label,
.readonly-grid span {
  color: var(--p-text-muted-color);
  font-size: 12px;
}

.readonly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
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
