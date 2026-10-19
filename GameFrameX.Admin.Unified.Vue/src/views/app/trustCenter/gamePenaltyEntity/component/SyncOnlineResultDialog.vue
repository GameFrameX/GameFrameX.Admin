<template>
  <Dialog v-model:visible="visible" modal
          :header="t('message.gameBusiness.trustSafety.penalty.syncResultTitle')"
          :draggable="false" :dismissableMask="false"
          class="gamePenaltyEntity-sync-result-dialog w-full md:w-40rem">
    <div v-if="result" class="flex flex-column gap-3">
      <div class="form-grid">
        <label>{{ t('message.gameBusiness.trustSafety.penalty.syncResultPenaltyId') }}</label>
        <span data-testid="sync-result-penalty-id">{{ result.penaltyId }}</span>
        <label>{{ t('message.gameBusiness.trustSafety.penalty.syncResultTarget') }}</label>
        <span data-testid="sync-result-target">{{ targetId }}</span>
        <label>{{ t('message.gameBusiness.trustSafety.penalty.syncResultOperator') }}</label>
        <span data-testid="sync-result-operator">{{ operatorId }}</span>
        <label>{{ t('message.gameBusiness.trustSafety.penalty.syncResultCommand') }}</label>
        <span>
          <Tag :value="commandLabel(result.command)"
               :severity="result.command === 2 ? 'danger' : 'warn'" />
        </span>
        <label>{{ t('message.gameBusiness.trustSafety.penalty.syncResultAllSucceeded') }}</label>
        <span data-testid="sync-result-all-succeeded">{{ result.allSucceeded ? t('common.label.yes') : t('common.label.no') }}</span>
      </div>

      <DataTable :value="result.results ?? []" showGridlines stripedRows size="small"
                 class="gamePenaltyEntity-sync-result-table">
        <Column :header="t('message.gameBusiness.trustSafety.penalty.syncResultServerId')" field="serverId">
          <template #body="{ data }">{{ data.serverId }}</template>
        </Column>
        <Column :header="t('message.gameBusiness.trustSafety.penalty.syncResultSuccess')" field="success">
          <template #body="{ data }">
            <Tag :value="data.success ? t('common.label.yes') : t('common.label.no')"
                 :severity="data.success ? 'success' : 'danger'" />
          </template>
        </Column>
        <Column :header="t('message.gameBusiness.trustSafety.penalty.syncResultTokenRevoked')">
          <template #body="{ data }">
            <Tag :value="data.tokenRevokedConfirmed ? t('common.label.yes') : t('common.label.no')"
                 :severity="data.tokenRevokedConfirmed ? 'success' : 'secondary'"
                 class="sync-result-token-revoked" />
          </template>
        </Column>
        <Column :header="t('message.gameBusiness.trustSafety.penalty.syncResultFailureMessage')" field="failureMessage">
          <template #body="{ data }">{{ data.failureMessage ?? '-' }}</template>
        </Column>
      </DataTable>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="gamePenaltyEntitySyncOnlineResultDialog">
/**
 * 「同步 Online 运行时」结果弹层（C342 / GFX-758）。
 * 展示联动命令聚合结果：命令种类、各目标区服执行结果、旧 Token 吊销确认字段
 * （vault:C2 S1.4 契约的 Admin 展示半边；服务端验证引用 vault 侧用例）。
 */
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import type { OnlinePenaltyLinkResponse } from '../../../../../api/_shared/data-contracts'

const visible = ref(false)
const result = ref<OnlinePenaltyLinkResponse | null>(null)
const targetId = ref('')
const operatorId = ref('')

// OnlinePenaltyCommandKind：Kick=1 / Ban=2 / Mute=3（三值硬白名单）
const commandLabel = (value: number | undefined | null) => {
  switch (value) {
    case 1: return t('message.gameBusiness.trustSafety.penalty.syncCommand.kick')
    case 2: return t('message.gameBusiness.trustSafety.penalty.syncCommand.ban')
    case 3: return t('message.gameBusiness.trustSafety.penalty.syncCommand.mute')
    default: return '-'
  }
}

const close = () => {
  visible.value = false
  result.value = null
}

defineExpose({
  openDialog: (payload: OnlinePenaltyLinkResponse, context: { targetId?: string; operatorId?: string | number }) => {
    result.value = payload
    targetId.value = String(context.targetId ?? '-')
    operatorId.value = String(context.operatorId ?? '-')
    visible.value = true
  },
})
</script>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 0.5rem 1rem;
  align-items: center;
}
</style>
