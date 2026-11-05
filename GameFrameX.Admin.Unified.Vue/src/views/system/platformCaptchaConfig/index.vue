<template>
  <div class="platform-captcha-config-container">
    <Card>
      <template #title>
        <div class="page-title">
          <i class="fa-solid fa-image"></i>
          <span>{{ t('platformCaptchaConfig.title') }}</span>
        </div>
      </template>
      <template #content>
        <Message severity="info" :closable="false" class="mb-4">
          {{ t('platformCaptchaConfig.saveHint') }}
        </Message>

        <div class="form-grid">
          <div class="field switch-field">
            <label for="enabled">{{ t('platformCaptchaConfig.enabled') }}</label>
            <ToggleSwitch id="enabled" v-model="form.enabled"/>
          </div>

          <div class="field">
            <label for="captchaType">{{ t('platformCaptchaConfig.captchaType') }}</label>
            <InputNumber id="captchaType" v-model="form.captchaType" class="w-full" :min="0" :max="11" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="codeLength">{{ t('platformCaptchaConfig.codeLength') }}</label>
            <InputNumber id="codeLength" v-model="form.codeLength" class="w-full" :min="1" :max="8" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="expirySeconds">{{ t('platformCaptchaConfig.expirySeconds') }}</label>
            <InputNumber id="expirySeconds" v-model="form.expirySeconds" class="w-full" :min="10" :max="600" :useGrouping="false"/>
          </div>

          <div class="field switch-field">
            <label for="ignoreCase">{{ t('platformCaptchaConfig.ignoreCase') }}</label>
            <ToggleSwitch id="ignoreCase" v-model="form.ignoreCase"/>
          </div>
        </div>

        <Divider/>

        <div class="form-grid">
          <div class="field switch-field">
            <label for="animation">{{ t('platformCaptchaConfig.animation') }}</label>
            <ToggleSwitch id="animation" v-model="form.imageOption.animation"/>
          </div>

          <div class="field">
            <label for="width">{{ t('platformCaptchaConfig.width') }}</label>
            <InputNumber id="width" v-model="form.imageOption.width" class="w-full" :min="80" :max="400" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="height">{{ t('platformCaptchaConfig.height') }}</label>
            <InputNumber id="height" v-model="form.imageOption.height" class="w-full" :min="30" :max="180" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="fontSize">{{ t('platformCaptchaConfig.fontSize') }}</label>
            <InputNumber id="fontSize" v-model="form.imageOption.fontSize" class="w-full" :min="12" :max="96" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="fontFamily">{{ t('platformCaptchaConfig.fontFamily') }}</label>
            <InputText id="fontFamily" v-model="form.imageOption.fontFamily" class="w-full" maxlength="32"/>
          </div>

          <div class="field">
            <label for="backgroundColor">{{ t('platformCaptchaConfig.backgroundColor') }}</label>
            <InputText id="backgroundColor" v-model="form.imageOption.backgroundColor" class="w-full" maxlength="16"/>
          </div>

          <div class="field">
            <label for="foregroundColors">{{ t('platformCaptchaConfig.foregroundColors') }}</label>
            <InputText id="foregroundColors" v-model="form.imageOption.foregroundColors" class="w-full" maxlength="128"/>
          </div>

          <div class="field">
            <label for="bubbleMinRadius">{{ t('platformCaptchaConfig.bubbleMinRadius') }}</label>
            <InputNumber id="bubbleMinRadius" v-model="form.imageOption.bubbleMinRadius" class="w-full" :min="0" :max="40" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="bubbleMaxRadius">{{ t('platformCaptchaConfig.bubbleMaxRadius') }}</label>
            <InputNumber id="bubbleMaxRadius" v-model="form.imageOption.bubbleMaxRadius" class="w-full" :min="0" :max="60" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="bubbleCount">{{ t('platformCaptchaConfig.bubbleCount') }}</label>
            <InputNumber id="bubbleCount" v-model="form.imageOption.bubbleCount" class="w-full" :min="0" :max="30" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="bubbleThickness">{{ t('platformCaptchaConfig.bubbleThickness') }}</label>
            <InputNumber id="bubbleThickness" v-model="form.imageOption.bubbleThickness" class="w-full" :min="0" :max="10" :minFractionDigits="0" :maxFractionDigits="2" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="interferenceLineCount">{{ t('platformCaptchaConfig.interferenceLineCount') }}</label>
            <InputNumber id="interferenceLineCount" v-model="form.imageOption.interferenceLineCount" class="w-full" :min="0" :max="30" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="frameDelay">{{ t('platformCaptchaConfig.frameDelay') }}</label>
            <InputNumber id="frameDelay" v-model="form.imageOption.frameDelay" class="w-full" :min="50" :max="2000" :useGrouping="false"/>
          </div>

          <div class="field">
            <label for="quality">{{ t('platformCaptchaConfig.quality') }}</label>
            <InputNumber id="quality" v-model="form.imageOption.quality" class="w-full" :min="1" :max="100" :useGrouping="false"/>
          </div>

          <div class="field switch-field">
            <label for="textBold">{{ t('platformCaptchaConfig.textBold') }}</label>
            <ToggleSwitch id="textBold" v-model="form.imageOption.textBold"/>
          </div>
        </div>

        <Divider/>

        <div class="readonly-grid">
          <div>
            <span>{{ t('platformCaptchaConfig.version') }}</span>
            <strong>{{ detail.version || '-' }}</strong>
          </div>
          <div>
            <span>{{ t('platformCaptchaConfig.currentStatus') }}</span>
            <Tag :severity="form.enabled ? 'success' : 'secondary'" :value="form.enabled ? t('platformCaptchaConfig.on') : t('platformCaptchaConfig.off')"/>
          </div>
        </div>

        <div class="actions">
          <Button severity="secondary" :loading="loading" @click="loadConfig">
            <i class="fa-solid fa-arrows-rotate mr-1"></i>
            {{ t('common.button.refresh') }}
          </Button>
          <Button v-auth="'platformCaptchaConfig:update'" :loading="saving" @click="saveConfig">
            <i class="fa-solid fa-floppy-disk mr-1"></i>
            {{ t('common.button.save') }}
          </Button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="platformCaptchaConfig">
import {onMounted, reactive, ref} from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Divider from 'primevue/divider'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import ToggleSwitch from 'primevue/toggleswitch'
import {platformCaptchaConfigDetailPost, platformCaptchaConfigUpdatePost} from '../../../api/platformCaptchaConfig';
import {showError, showSuccess} from '../../../utils/message'

const loading = ref(false)
const saving = ref(false)

const form = reactive({
  enabled: false,
  captchaType: 10,
  codeLength: 1,
  expirySeconds: 60,
  ignoreCase: true,
  imageOption: {
    animation: true,
    fontSize: 36,
    width: 150,
    height: 50,
    bubbleMinRadius: 5,
    bubbleMaxRadius: 10,
    bubbleCount: 3,
    bubbleThickness: 1,
    interferenceLineCount: 3,
    fontFamily: 'kaiti',
    frameDelay: 300,
    backgroundColor: '#ffffff',
    foregroundColors: '',
    quality: 100,
    textBold: true,
  },
})

const detail = reactive({
  version: 0,
})

const loadConfig = async () => {
  loading.value = true
  try {
    const res = await platformCaptchaConfigDetailPost()
    const data = res.data.data || {}
    Object.assign(form, {
      enabled: data.enabled ?? false,
      captchaType: data.captchaType ?? 10,
      codeLength: data.codeLength ?? 1,
      expirySeconds: data.expirySeconds ?? 60,
      ignoreCase: data.ignoreCase ?? true,
      imageOption: {...form.imageOption, ...(data.imageOption || {})},
    })
    detail.version = data.version || 0
  } finally {
    loading.value = false
  }
}

const validateForm = () => {
  if (form.imageOption.bubbleMinRadius > form.imageOption.bubbleMaxRadius) {
    showError(t('platformCaptchaConfig.bubbleRadiusInvalid'))
    return false
  }
  if (!/^#?([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$/.test(form.imageOption.backgroundColor || '')) {
    showError(t('platformCaptchaConfig.backgroundColorInvalid'))
    return false
  }
  return true
}

const saveConfig = async () => {
  if (!validateForm()) return
  saving.value = true
  try {
    await platformCaptchaConfigUpdatePost({...form, imageOption: {...form.imageOption}})
    showSuccess(t('platformCaptchaConfig.saveSuccess'))
    await loadConfig()
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped lang="scss">
.platform-captcha-config-container {
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
