<template>
  <Dialog v-model:visible="visible" modal header="轮换签名密钥" :style="{ width: 'min(460px, calc(100vw - 32px))' }">
    <div class="key-form">
      <div class="key-label">应用 ID</div>
      <InputNumber :modelValue="appId" :useGrouping="false" class="w-full" disabled/>
      <div class="key-label">旧密钥到期时间（双活窗口收口）</div>
      <DatePicker v-model="oldKeyExpireTime" showTime class="w-full" placeholder="留空表示不改动旧密钥租约"/>
      <small class="key-tip">轮换 = 新插一把立即生效的密钥，旧密钥不禁用不删除：新旧双活期间两边签名均通过，客户端全量切换后再让旧密钥到期退场。</small>
    </div>
    <template #footer>
      <Button label="取消" text @click="visible = false"/>
      <Button label="确认轮换" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import { tenantAppSignatureKeyEntityRotatePost } from '../../../../../api/tenantAppSignatureKeyEntity'

const emit = defineEmits(['rotated'])

const visible = ref(false)
const appId = ref(0)
const oldKeyExpireTime = ref<any>(null)

const open = (row: any) => {
  appId.value = row.appId
  oldKeyExpireTime.value = null
  visible.value = true
}

const confirm = async () => {
  const expireTimeLong = oldKeyExpireTime.value ? Math.floor(new Date(oldKeyExpireTime.value).getTime() / 1000) : null
  const res = await tenantAppSignatureKeyEntityRotatePost({ appId: appId.value, oldKeyExpireTime: expireTimeLong })
  visible.value = false
  emit('rotated', res.data)
}

defineExpose({ open })
</script>

<style scoped>
.key-form {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 4px 0 12px;
}

.key-label {
  font-size: 14px;
  font-weight: 500;
}

.key-tip {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
