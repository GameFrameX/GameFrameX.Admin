<template>
  <Dialog v-model:visible="visible" modal header="新增会话密钥" :style="{ width: 'min(460px, calc(100vw - 32px))' }">
    <div class="key-form">
      <div class="key-label">应用 ID</div>
      <InputNumber v-model="appId" :useGrouping="false" class="w-full" placeholder="密钥归属的应用（密钥域）"/>
      <div class="key-label">生效时间</div>
      <DatePicker v-model="notBeforeTime" showTime class="w-full" placeholder="留空表示立即生效"/>
      <div class="key-label">过期时间</div>
      <DatePicker v-model="expireTime" showTime class="w-full" placeholder="留空表示永不过期"/>
      <div class="key-label">备注</div>
      <InputText v-model="remark" class="w-full" placeholder="备注（可选）"/>
      <small class="key-tip">密钥明文由服务端随机生成，提交后在弹窗中一次性展示；同一应用可并存多把密钥（多版本客户端各持一把）。</small>
    </div>
    <template #footer>
      <Button label="取消" text @click="visible = false"/>
      <Button label="确认新增" :disabled="appId == null" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import { tenantSessionKeyEntityAddPost } from '../../../../../api/tenantSessionKeyEntity'

const emit = defineEmits(['added'])

const visible = ref(false)
const appId = ref<number | null>(null)
const notBeforeTime = ref<any>(null)
const expireTime = ref<any>(null)
const remark = ref('')

const open = () => {
  appId.value = null
  notBeforeTime.value = null
  expireTime.value = null
  remark.value = ''
  visible.value = true
}

const confirm = async () => {
  const toLong = (value: any) => (value ? Math.floor(new Date(value).getTime() / 1000) : null)
  const res = await tenantSessionKeyEntityAddPost({
    appId: appId.value != null ? String(appId.value) : undefined,
    notBeforeTime: toLong(notBeforeTime.value),
    expireTime: toLong(expireTime.value),
    remark: remark.value || null,
  })
  visible.value = false
  emit('added', res.data)
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
