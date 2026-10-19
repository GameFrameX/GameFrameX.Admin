<template>
  <Dialog v-model:visible="visible" modal header="续租会话密钥" :style="{ width: 'min(460px, calc(100vw - 32px))' }">
    <div class="key-form">
      <div class="key-label">Key ID</div>
      <InputText :modelValue="kid" readonly class="w-full"/>
      <div class="key-label">新的到期时间</div>
      <DatePicker v-model="expireTime" showTime class="w-full" placeholder="留空表示改为永不过期"/>
      <small class="key-tip">续租只更新到期时间，不产生新密钥。典型用途：轮换后客户端切换进度不及预期，延长旧密钥双活窗口。</small>
    </div>
    <template #footer>
      <Button label="取消" text @click="visible = false"/>
      <Button label="确认续租" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import InputText from 'primevue/inputtext'
import { tenantSessionKeyEntityRenewPost } from '../../../../../api/tenantSessionKeyEntity'

const emit = defineEmits(['renewed'])

const visible = ref(false)
const id = ref('')
const kid = ref('')
const expireTime = ref<any>(null)

const open = (row: any) => {
  id.value = row.id
  kid.value = row.kid
  expireTime.value = null
  visible.value = true
}

const confirm = async () => {
  const expireTimeLong = expireTime.value ? Math.floor(new Date(expireTime.value).getTime() / 1000) : null
  await tenantSessionKeyEntityRenewPost({ id: id.value, expireTime: expireTimeLong })
  visible.value = false
  emit('renewed')
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
