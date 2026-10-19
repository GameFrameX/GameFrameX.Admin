<template>
  <Dialog v-model:visible="visible" modal header="轮换签名密钥" :style="{ width: 'min(420px, calc(100vw - 32px))' }">
    <div class="rotate-form">
      <div class="rotate-label">新密钥到期时间</div>
      <DatePicker v-model="expireTime" showTime class="w-full" placeholder="留空表示永不过期"/>
      <small class="rotate-tip">留空表示永不过期；有值则新密钥在该时刻失效，旧密钥到期后自然退场。</small>
    </div>
    <template #footer>
      <Button label="取消" text @click="visible = false"/>
      <Button label="确认轮换" @click="confirm"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'

const emit = defineEmits(['rotate'])

const visible = ref(false)
const expireTime = ref<any>(null)
const appId = ref(0)

const open = (row: any) => {
  appId.value = row.appId
  expireTime.value = null
  visible.value = true
}

const confirm = () => {
  // 到期时间存为 Unix 秒（long）；留空 = null（永不过期）
  const expireTimeLong = expireTime.value ? Math.floor(new Date(expireTime.value).getTime() / 1000) : null
  emit('rotate', {appId: appId.value, expireTime: expireTimeLong})
  visible.value = false
}

defineExpose({open})
</script>

<style scoped>
.rotate-form {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 4px 0 12px;
}

.rotate-label {
  font-size: 14px;
  font-weight: 500;
}

.rotate-tip {
  color: var(--p-text-muted-color);
  font-size: 12px;
}
</style>
