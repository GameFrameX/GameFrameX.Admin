<template>
  <small :id="helpId" class="form-help-text">
    <slot>{{ text }}</slot>
  </small>
</template>

<script lang="ts" setup>
import {computed, useId} from 'vue'

const props = defineProps<{
  /** 辅助说明文本；与默认 slot 二选一，slot 优先 */
  text?: string
  /** 显式指定渲染元素 id；不传则用 useId() 生成稳定 id，便于和输入控件 aria-describedby 关联 */
  id?: string
}>()

const autoId = useId()
const helpId = computed(() => props.id ?? autoId)

defineExpose({id: helpId})
</script>

<style scoped>
.form-help-text {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  line-height: 1.4;
  color: var(--p-text-muted-color);
  word-break: break-word;
}
</style>
