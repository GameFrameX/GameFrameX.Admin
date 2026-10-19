<template>
  <div class="onlineControlledOperation-container">
    <Card>
      <template #header>
        <span>{{ t('message.onlineCenter.controlledOperation.title') }}</span>
      </template>
      <template #content>
        <div class="font-medium text-color-secondary mb-3">
          {{ t('message.onlineCenter.controlledOperation.hint') }}
        </div>
        <div v-block-loading="loading" class="form-grid">
          <label>{{ t('message.onlineCenter.controlledOperation.commandKind') }}</label>
          <div>
            <Select
              v-model="form.commandKind"
              class="onlineControlledOperation-kind w-full"
              :options="kindOptions"
              optionLabel="label"
              optionValue="value"
              :placeholder="t('message.onlineCenter.controlledOperation.pleaseSelectCommand')"
              data-key="commandKind"
            />
            <small v-if="errors.commandKind" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.commandKindRequired') }}</small>
          </div>

          <label>{{ t('message.onlineCenter.controlledOperation.serverId') }}</label>
          <InputText v-model="form.serverId"
                     :placeholder="t('message.onlineCenter.controlledOperation.pleaseEnterServerId')" />

          <template v-if="targetField === 'ticketId'">
            <label>{{ t('message.onlineCenter.controlledOperation.ticketId') }}</label>
            <div>
              <InputText v-model="form.ticketId" class="w-full"
                         :placeholder="t('message.onlineCenter.controlledOperation.pleaseEnterTicketId')" />
              <small v-if="errors.ticketId" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.targetRequired') }}</small>
            </div>
          </template>
          <template v-else-if="targetField === 'matchId'">
            <label>{{ t('message.onlineCenter.controlledOperation.matchId') }}</label>
            <div>
              <InputText v-model="form.matchId" class="w-full"
                         :placeholder="t('message.onlineCenter.controlledOperation.pleaseEnterMatchId')" />
              <small v-if="errors.matchId" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.targetRequired') }}</small>
            </div>
          </template>
          <template v-else-if="targetField === 'playerId'">
            <label>{{ t('message.onlineCenter.controlledOperation.playerId') }}</label>
            <div>
              <InputText v-model="form.playerId" class="w-full"
                         :placeholder="t('message.onlineCenter.controlledOperation.pleaseEnterPlayerId')" />
              <small v-if="errors.playerId" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.targetRequired') }}</small>
            </div>
          </template>

          <template v-if="directional">
            <label>
              {{ t('message.onlineCenter.controlledOperation.direction') }}
              <span class="text-red-500">*</span>
            </label>
            <div>
              <Select v-model="form.direction"
                      class="onlineControlledOperation-direction w-full"
                      :options="directionOptions"
                      optionLabel="label"
                      optionValue="value"
                      :placeholder="t('message.onlineCenter.controlledOperation.pleaseSelectDirection')"
                      data-key="direction" />
              <small v-if="errors.direction" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.directionRequired') }}</small>
            </div>
          </template>

          <label>
            {{ t('message.onlineCenter.controlledOperation.reason') }}
            <span class="text-red-500">*</span>
          </label>
          <div>
            <Textarea v-model="form.reason" :rows="3" :autoResize="true" class="w-full"
                      :placeholder="t('message.onlineCenter.controlledOperation.reasonPlaceholder')" />
            <small v-if="errors.reason" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.reasonRequired') }}</small>
          </div>

          <label>
            {{ t('message.onlineCenter.controlledOperation.reviewerOperatorId') }}
            <span class="text-red-500">*</span>
          </label>
          <div>
            <InputText v-model="form.reviewerOperatorId" class="w-full"
                       :placeholder="t('message.onlineCenter.controlledOperation.pleaseEnterReviewer')" />
            <small v-if="errors.reviewerOperatorId" class="text-red-500">{{ t('message.onlineCenter.controlledOperation.reviewerRequired') }}</small>
          </div>
        </div>

        <div class="flex justify-content-end gap-2 mt-3">
          <Button :label="t('message.common.button.cancel')" text @click="resetForm" />
          <Button v-if="auth('onlineControlledCommand:execute')" class="onlineControlledOperation-submit"
                  :label="t('message.onlineCenter.controlledOperation.execute')"
                  icon="fa-solid fa-triangle-exclamation"
                  :severity="highRisk ? 'danger' : 'warn'"
                  :loading="loading"
                  @click="confirmExecute" />
        </div>
      </template>
    </Card>

    <Card v-if="lastResult" style="margin-top: 8px" class="onlineControlledOperation-result">
      <template #header>
        <span>{{ t('message.onlineCenter.controlledOperation.resultTitle') }}</span>
      </template>
      <template #content>
        <Message severity="success">
          {{ t('message.onlineCenter.controlledOperation.executed') }}
        </Message>
        <div class="form-grid mt-2">
          <label>{{ t('message.onlineCenter.controlledOperation.resultCommand') }}</label>
          <span data-field="command">{{ lastResult.command }}</span>
          <label>{{ t('message.onlineCenter.controlledOperation.resultTarget') }}</label>
          <span data-field="targetIdentifier">{{ lastResult.targetIdentifier }}</span>
          <label>{{ t('message.onlineCenter.controlledOperation.resultIdempotencyKey') }}</label>
          <span data-field="idempotencyKey">{{ lastResult.idempotencyKey }}</span>
          <label>{{ t('message.onlineCenter.controlledOperation.resultStatus') }}</label>
          <span data-field="status">{{ lastResult.status }}</span>
          <label>{{ t('message.onlineCenter.controlledOperation.resultExecutedAt') }}</label>
          <span data-field="executedAt">{{ formatUnix(lastResult.executedAt) }}</span>
          <label>{{ t('message.onlineCenter.controlledOperation.resultSummary') }}</label>
          <span data-field="summary">{{ lastResult.summary }}</span>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="onlineControlledOperation">
/**
 * 受控操作页（C344 / GFX-761，独立页承载；C356 联调对齐）。
 * command 数字枚举严格四值；目标标识按命令动态；开关型命令 direction 必填；
 * 理由必填；复核人全部命令必填（双人复核全量口径）；
 * 幂等语义：同命令+方向+目标+区服重复调用返回首次结果，对调用方透明（响应无 duplicate 标志）。
 * 时间线页入口携带 playerId/serverId 预填。纯逻辑见 commandForm.ts（node --test 单测）。
 */
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import Message from 'primevue/message'
import { formatUnix } from '../../../../utils/formatTime.ts'
import { auth } from '../../../../utils/authFunction.ts'
import { useCurrentApp } from '../../../../composables/useCurrentApp.ts'
import { showSuccess, showConfirm } from '../../../../utils/message.ts'
import { onlineControlledCommandExecutePost } from '../../../../api/onlineControlledCommand'
import type { OnlineControlledCommandAckResponse } from '../../../../api/onlineControlledCommand/types'
import {
  CONTROLLED_COMMAND_META,
  CONTROLLED_COMMAND_KINDS,
  createControlledCommandForm,
  validateControlledCommandForm,
  buildControlledCommandRequest,
} from './commandForm.ts'

const route = useRoute()
const { currentAppId, resolveInitialAppId } = useCurrentApp()

const form = ref(createControlledCommandForm())
const errors = ref<Record<string, boolean>>({})
const loading = ref(false)
const lastResult = ref<OnlineControlledCommandAckResponse | null>(null)

const kindOptions = computed(() =>
  CONTROLLED_COMMAND_KINDS.map((kind) => ({
    label: t(`message.onlineCenter.controlledOperation.kinds.${CONTROLLED_COMMAND_META[kind].name}`),
    value: kind,
  })))

// 方向（开关型命令必填）：1=Apply 施加 / 2=Lift 解除
const directionOptions = computed(() => [
  { label: t('message.onlineCenter.controlledOperation.directionApply'), value: 1 },
  { label: t('message.onlineCenter.controlledOperation.directionLift'), value: 2 },
])

const meta = computed(() => (form.value.commandKind ? CONTROLLED_COMMAND_META[form.value.commandKind] : undefined))
const targetField = computed(() => meta.value?.targetField)
const directional = computed(() => meta.value?.directional ?? false)
const highRisk = computed(() => meta.value?.highRisk ?? false)

// 切换命令：清空校验 + 重置方向（单向命令禁填 direction，残留值会导致校验失败）
watch(() => form.value.commandKind, () => {
  errors.value = {}
  form.value.direction = ''
})

const resetForm = () => {
  form.value = createControlledCommandForm({ playerId: form.value.playerId, serverId: form.value.serverId })
  errors.value = {}
  lastResult.value = null
}

const confirmExecute = async () => {
  const validation = validateControlledCommandForm({ ...form.value, appId: currentAppId.value ?? form.value.appId })
  errors.value = validation
  if (Object.keys(validation).length > 0) {
    return
  }
  if (highRisk.value) {
    const ok = await showConfirm(t('message.onlineCenter.controlledOperation.highRiskConfirm'))
    if (!ok) {
      return
    }
  }
  loading.value = true
  try {
    const request = buildControlledCommandRequest({ ...form.value, appId: currentAppId.value ?? form.value.appId })
    const res: any = await onlineControlledCommandExecutePost(request)
    lastResult.value = res.data?.data ?? null
    if (lastResult.value) {
      showSuccess(t('message.onlineCenter.controlledOperation.executed'))
    }
  } finally {
    loading.value = false
  }
}

watch(() => route.query.appId, () => {
  currentAppId.value = resolveInitialAppId(route.query)
})

onMounted(() => {
  if (!currentAppId.value) {
    currentAppId.value = resolveInitialAppId(route.query)
  }
  form.value.appId = currentAppId.value ?? ''
  if (typeof route.query.playerId === 'string' && route.query.playerId) {
    form.value.playerId = route.query.playerId
  }
  if (typeof route.query.serverId === 'string' && route.query.serverId) {
    form.value.serverId = route.query.serverId
  }
})
</script>
