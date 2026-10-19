<template>
  <Dialog v-model:visible="visible" modal header="密钥一次性回显" :style="{ width: 'min(520px, calc(100vw - 32px))' }" @hide="reveal = null">
    <div class="key-form" v-if="reveal">
      <div>
        <label class="key-label">Key ID</label>
        <div class="flex align-items-center gap-2">
          <InputText :modelValue="reveal.kid" readonly class="w-full"/>
          <Button link @click="copyToClipboard(String(reveal.kid))">
            <i class="fa-solid fa-copy"></i>
          </Button>
        </div>
      </div>
      <div>
        <label class="key-label">密钥明文（HMAC Secret）</label>
        <div class="flex align-items-center gap-2">
          <InputText :modelValue="reveal.secret" readonly class="w-full"/>
          <Button link @click="copyToClipboard(String(reveal.secret))">
            <i class="fa-solid fa-copy"></i>
          </Button>
        </div>
      </div>
      <div class="text-orange-500 text-sm">
        <i class="fa-solid fa-triangle-exclamation mr-1"></i>
        这是该密钥明文的唯一一次展示，关闭后无法再次查看；请立即复制并配置到客户端签名通道。丢失只能重新轮换。
      </div>
    </div>
    <template #footer>
      <Button label="我已保存" @click="visible = false"/>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import { showSuccess, showWarning } from '../../../../../utils/message.ts'

const visible = ref(false)
const reveal = ref<any>(null)

const open = (data: any) => {
  reveal.value = data
  visible.value = true
}

// 剪贴板 API 失败时降级选区复制；再失败必须如实告警——本弹窗是密钥明文唯一一次
// 展示，假「已复制」会让操作者关窗后密钥永久丢失（只能 Rotate 重建）
const fallbackCopy = (text: string) => {
  const textarea = document.createElement('textarea')
  textarea.value = text
  textarea.style.position = 'fixed'
  textarea.style.opacity = '0'
  document.body.appendChild(textarea)
  textarea.select()
  const ok = document.execCommand('copy')
  document.body.removeChild(textarea)
  return ok
}

const copyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text).then(() => {
    showSuccess('已复制')
  }).catch(() => {
    if (fallbackCopy(text)) {
      showSuccess('已复制')
    } else {
      showWarning('复制失败，请手动选中文本复制后再关闭')
    }
  })
}

defineExpose({ open })
</script>

<style scoped>
.key-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 4px 0 8px;
}

.key-label {
  display: block;
  margin-bottom: 6px;
  font-size: 14px;
  font-weight: 500;
}
</style>
