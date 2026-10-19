<template>
  <div class="platform-password-policy-config-container">
    <Card>
      <template #title>
        <div class="page-title">
          <i class="fa-solid fa-key"></i>
          <span>{{ t('platformPasswordPolicyConfig.title') }}</span>
        </div>
      </template>
      <template #content>
        <Message severity="info" :closable="false" class="mb-4">
          {{ t('platformPasswordPolicyConfig.saveHint') }}
        </Message>

        <div class="form-grid">
          <div class="field switch-field">
            <label for="strongPassword">{{ t('platformPasswordPolicyConfig.strongPassword') }}</label>
            <ToggleSwitch id="strongPassword" v-model="form.strongPassword"/>
          </div>
          <div class="field field-wide">
            <label for="passwordStrengthValidation">{{ t('platformPasswordPolicyConfig.passwordStrengthValidation') }}</label>
            <InputText id="passwordStrengthValidation" v-model="form.passwordStrengthValidation" class="w-full" maxlength="1024"/>
            <small class="field-hint">{{ t('platformPasswordPolicyConfig.passwordStrengthValidationHint') }}</small>
          </div>
          <div class="field field-wide">
            <label for="passwordStrengthValidationMsg">{{ t('platformPasswordPolicyConfig.passwordStrengthValidationMsg') }}</label>
            <InputText id="passwordStrengthValidationMsg" v-model="form.passwordStrengthValidationMsg" class="w-full" maxlength="1024"/>
          </div>
        </div>

        <Divider/>

        <div class="readonly-grid">
          <div>
            <span>{{ t('platformPasswordPolicyConfig.updateTime') }}</span>
            <strong>{{ updateTimeText }}</strong>
          </div>
        </div>

        <div class="actions">
          <Button severity="secondary" :loading="loading" @click="loadConfig">
            <i class="fa-solid fa-arrows-rotate mr-1"></i>
            {{ t('common.button.refresh') }}
          </Button>
          <Button v-auth="'platformPasswordPolicyConfig:update'" :loading="saving" @click="saveConfig">
            <i class="fa-solid fa-floppy-disk mr-1"></i>
            {{ t('common.button.save') }}
          </Button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="platformPasswordPolicyConfig">
import {computed, onMounted, reactive, ref} from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Divider from 'primevue/divider'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import ToggleSwitch from 'primevue/toggleswitch'
import { platformPasswordPolicyConfigDetailPost, platformPasswordPolicyConfigUpdatePost } from '../../../../api/platformPasswordPolicyConfig'
import type {PlatformPasswordPolicyConfigUpdateInput} from '../../../api/global/platformPasswordPolicyConfig'
import {formatUnix} from '../../../../utils/formatTime.ts'
import {showError, showSuccess} from '../../../../utils/message.ts'

const loading = ref(false)
const saving = ref(false)

const form = reactive({
  strongPassword: false,
  passwordStrengthValidation: '',
  passwordStrengthValidationMsg: '',
})

const updateTime = ref<number | null>(null)

const updateTimeText = computed(() => (updateTime.value ? formatUnix(updateTime.value) : '-'))

const loadConfig = async () => {
  loading.value = true
  try {
    const res = await platformPasswordPolicyConfigDetailPost()
    const data = res.data.data || {}
    form.strongPassword = !!data.strongPassword
    form.passwordStrengthValidation = data.passwordStrengthValidation ?? ''
    form.passwordStrengthValidationMsg = data.passwordStrengthValidationMsg ?? ''
    updateTime.value = data.updateTime ?? null
  } catch {
    showError(t('platformPasswordPolicyConfig.loadFailed'))
  } finally {
    loading.value = false
  }
}

const validateForm = () => {
  if (!form.passwordStrengthValidation || !form.passwordStrengthValidation.trim()) {
    showError(t('platformPasswordPolicyConfig.validationEmpty'))
    return false
  }
  try {
    // 与后端 Validate 一致：正则必须可编译，避免存入无效表达式。
    new RegExp(form.passwordStrengthValidation)
  } catch {
    showError(t('platformPasswordPolicyConfig.validationInvalid'))
    return false
  }
  if (!form.passwordStrengthValidationMsg || !form.passwordStrengthValidationMsg.trim()) {
    showError(t('platformPasswordPolicyConfig.validationMessageInvalid'))
    return false
  }
  return true
}

const buildUpdatePayload = (): PlatformPasswordPolicyConfigUpdateInput => {
  return {
    strongPassword: form.strongPassword,
    passwordStrengthValidation: form.passwordStrengthValidation,
    passwordStrengthValidationMsg: form.passwordStrengthValidationMsg,
  }
}

const saveConfig = async () => {
  if (!validateForm()) return
  saving.value = true
  try {
    await platformPasswordPolicyConfigUpdatePost(buildUpdatePayload())
    showSuccess(t('platformPasswordPolicyConfig.saveSuccess'))
    await loadConfig()
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped lang="scss">
.platform-password-policy-config-container {
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

.field-hint {
  color: var(--p-text-muted-color);
  font-size: 12px;
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
