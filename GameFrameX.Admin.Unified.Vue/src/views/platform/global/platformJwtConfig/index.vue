<template>
  <div class="platform-jwt-config-container">
    <Card>
      <template #title>
        <div class="page-title">
          <i class="fa-solid fa-shield-halved"></i>
          <span>{{ t('platformJwtConfig.title') }}</span>
        </div>
      </template>
      <template #content>
        <Message severity="warn" :closable="false" class="mb-4">
          {{ t('platformJwtConfig.saveWarning') }}
        </Message>
        <Message severity="error" :closable="false" class="mb-4">
          {{ t('platformJwtConfig.hubSyncWarning') }}
        </Message>

        <div class="form-grid">
          <div class="field">
            <label for="issuerSigningKey">{{ t('platformJwtConfig.issuerSigningKey') }}</label>
            <Password
                id="issuerSigningKey"
                v-model="form.issuerSigningKey"
                :feedback="false"
                toggleMask
                class="w-full"
                inputClass="w-full"
                autocomplete="new-password"
            />
          </div>

          <div class="field">
            <label for="validIssuer">{{ t('platformJwtConfig.validIssuer') }}</label>
            <InputText id="validIssuer" v-model="form.validIssuer" class="w-full" maxlength="128"/>
          </div>

          <div class="field">
            <label for="validAudience">{{ t('platformJwtConfig.validAudience') }}</label>
            <InputText id="validAudience" v-model="form.validAudience" class="w-full" maxlength="128"/>
          </div>

          <div class="field">
            <label for="clockSkew">{{ t('platformJwtConfig.clockSkew') }}</label>
            <InputNumber id="clockSkew" v-model="form.clockSkew" class="w-full" :min="0" :max="300" :useGrouping="false"/>
          </div>
        </div>

        <Divider/>

        <div class="readonly-grid">
          <div>
            <span>{{ t('platformJwtConfig.algorithm') }}</span>
            <strong>{{ detail.algorithm || 'HS256' }}</strong>
          </div>
          <div>
            <span>{{ t('platformJwtConfig.tokenVersion') }}</span>
            <strong>{{ detail.tokenVersion || '-' }}</strong>
          </div>
          <div>
            <span>{{ t('platformJwtConfig.hasSigningKey') }}</span>
            <Tag :severity="detail.hasSigningKey ? 'success' : 'danger'" :value="detail.hasSigningKey ? t('common.label.yes') : t('common.label.no')"/>
          </div>
          <div>
            <span>{{ t('platformJwtConfig.securityBaseline') }}</span>
            <strong>{{ baselineText }}</strong>
          </div>
        </div>

        <div class="actions">
          <Button severity="secondary" :loading="loading" @click="loadConfig">
            <i class="fa-solid fa-arrows-rotate mr-1"></i>
            {{ t('common.button.refresh') }}
          </Button>
          <Button v-auth="'platformJwtConfig:update'" :loading="saving" @click="saveConfig">
            <i class="fa-solid fa-floppy-disk mr-1"></i>
            {{ t('common.button.save') }}
          </Button>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="platformJwtConfig">
import {computed, onMounted, reactive, ref} from 'vue'
import {useRouter} from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import Divider from 'primevue/divider'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import Tag from 'primevue/tag'
import { platformJwtConfigDetailPost, platformJwtConfigUpdatePost } from '../../../../api/platformJwtConfig'
import {clearTokens} from '../../../../utils/axiosUtils.ts'
import {showConfirm, showSuccess} from '../../../../utils/message.ts'

const router = useRouter()
const loading = ref(false)
const saving = ref(false)

const form = reactive({
  issuerSigningKey: '******',
  validIssuer: '',
  validAudience: '',
  clockSkew: 5,
})

const detail = reactive({
  hasSigningKey: false,
  algorithm: 'HS256',
  tokenVersion: 0,
  validateIssuerSigningKey: true,
  validateIssuer: true,
  validateAudience: true,
  validateLifetime: true,
})

const baselineText = computed(() => {
  return [
    detail.validateIssuerSigningKey,
    detail.validateIssuer,
    detail.validateAudience,
    detail.validateLifetime,
  ].every(Boolean) ? t('platformJwtConfig.enabled') : t('platformJwtConfig.disabled')
})

const loadConfig = async () => {
  loading.value = true
  try {
    const res = await platformJwtConfigDetailPost()
    const data = res.data.data || {}
    form.issuerSigningKey = data.issuerSigningKey || '******'
    form.validIssuer = data.validIssuer || ''
    form.validAudience = data.validAudience || ''
    form.clockSkew = data.clockSkew ?? 5
    Object.assign(detail, data)
  } finally {
    loading.value = false
  }
}

const saveConfig = async () => {
  const confirmed = await showConfirm(t('platformJwtConfig.confirmSave'), t('common.message.tip'), {type: 'danger'})
  if (!confirmed) return

  saving.value = true
  try {
    await platformJwtConfigUpdatePost({...form})
    showSuccess(t('platformJwtConfig.saveSuccess'))
    clearTokens()
    await router.replace('/login')
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped lang="scss">
.platform-jwt-config-container {
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
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 16px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.field label,
.readonly-grid span {
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
