<template>
  <Dialog v-model:visible="visible" modal :header="title"
          :draggable="false" :dismissableMask="false" class="onlinePlayerSegment-edit-dialog w-full md:w-40rem">
    <div class="flex flex-column gap-3">
      <div class="font-medium text-color-secondary">
        {{ t('message.onlineCenter.playerSegment.editHint') }}
      </div>
      <div class="form-grid">
        <label>{{ t('message.onlineCenter.playerSegment.name') }}</label>
        <InputText v-model="form.name" :placeholder="t('message.onlineCenter.playerSegment.namePlaceholder')" />
        <label>{{ t('common.label.description') }}</label>
        <InputText v-model="form.description" :placeholder="t('message.onlineCenter.playerSegment.descriptionPlaceholder')" />
      </div>

      <div class="flex justify-content-between align-items-center">
        <span class="font-medium">{{ t('message.onlineCenter.playerSegment.conditions') }}</span>
        <Button :label="t('message.onlineCenter.playerSegment.addCondition')" icon="fa-solid fa-plus" size="small"
                outlined @click="addCondition" />
      </div>
      <div v-for="(condition, idx) in conditions" :key="idx" class="flex gap-2 align-items-center">
        <Select v-model="condition.field" :options="fieldOptions" optionLabel="label" optionValue="value"
                :placeholder="t('message.onlineCenter.playerSegment.fieldPlaceholder')" class="flex-1" />
        <Select v-model="condition.op" :options="opOptions" optionLabel="label" optionValue="value"
                :placeholder="t('message.onlineCenter.playerSegment.opPlaceholder')" class="w-8rem" />
        <InputText v-model="condition.value" :placeholder="t('message.onlineCenter.playerSegment.valuePlaceholder')" class="flex-1" />
        <Button icon="fa-solid fa-xmark" severity="secondary" text :disabled="conditions.length <= 1"
                @click="removeCondition(idx)" />
      </div>
      <small class="text-color-secondary">{{ t('message.onlineCenter.playerSegment.conditionHint') }}</small>
    </div>
    <template #footer>
      <Button :label="t('message.common.button.cancel')" text @click="close" />
      <Button :label="t('message.common.button.confirm')" icon="fa-solid fa-check" :loading="loading" @click="confirm" />
    </template>
  </Dialog>
</template>

<script lang="ts" setup name="onlinePlayerSegmentEditDialog">
/**
 * 玩家分群编辑弹层（C343 / GFX-759）。
 * 条件构建器：field/op 走后端 OnlineRolloutSemanticMap 白名单（三值 × 三操作符），
 * 序列化为 List<OnlineSegmentCondition> JSON 存 conditionsJson；空条件 = 不限条件。
 * 编辑不会自动重同步 Online——保存后须显式 syncOnline 生效（后端语义，UI 不隐藏该事实）。
 */
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import { showSuccess } from '../../../../../utils/message.ts'
import { tenantGameOnlinePlayerSegmentAddPost, tenantGameOnlinePlayerSegmentUpdatePost } from '../../../../../api/tenantGameOnlinePlayerSegment'
import type { UpdateTenantGameOnlinePlayerSegmentRequest } from '../../../../../api/tenantGameOnlinePlayerSegment/types'
import { ONLINE_SEGMENT_FIELD_OPTIONS, ONLINE_SEGMENT_OP_OPTIONS } from '../constants'

const props = withDefaults(defineProps<{
  title?: string
  appId?: string | number
}>(), { title: '', appId: undefined })

const visible = ref(false)
const loading = ref(false)
const isEditor = ref(false)
const editId = ref<number | string>()
const editRowVersion = ref<number | string>()
const form = ref({ name: '', description: '' })
const conditions = ref<Array<{ field: string; op: string; value: string }>>([{ field: '', op: '', value: '' }])
const emit = defineEmits(['reloadTable'])

const fieldOptions = ONLINE_SEGMENT_FIELD_OPTIONS.map((o) => ({ value: o.value, label: t(o.labelKey) }))
const opOptions = ONLINE_SEGMENT_OP_OPTIONS.map((o) => ({ value: o.value, label: t(o.labelKey) }))

const close = () => {
  visible.value = false
  reset()
}

const reset = () => {
  form.value = { name: '', description: '' }
  conditions.value = [{ field: '', op: '', value: '' }]
}

const addCondition = () => {
  conditions.value.push({ field: '', op: '', value: '' })
}

const removeCondition = (idx: number) => {
  if (conditions.value.length <= 1) {
    return
  }
  conditions.value.splice(idx, 1)
}

const validate = () => {
  if (!form.value.name.trim()) {
    return false
  }
  for (const condition of conditions.value) {
    // 行内任一格已填 → 三格必须齐（field 白名单 + op 白名单 + value 非空）
    const partiallyFilled = condition.field || condition.op || condition.value
    if (partiallyFilled && (!condition.field || !condition.op || !condition.value.trim())) {
      return false
    }
  }
  return true
}

/** 序列化条件：全空行丢弃；全空 = 不限条件（conditionsJson 不传） */
const buildConditionsJson = (): string | undefined => {
  const effective = conditions.value.filter((c) => c.field && c.op && c.value.trim())
  if (effective.length === 0) {
    return undefined
  }
  return JSON.stringify(effective.map((c) => ({ field: c.field, op: c.op, value: c.value.trim() })))
}

const confirm = async () => {
  if (!validate()) {
    return
  }
  loading.value = true
  try {
    if (isEditor.value) {
      const input: UpdateTenantGameOnlinePlayerSegmentRequest = {
        id: editId.value as number | string,
        rowVersion: editRowVersion.value,
        appId: props.appId as string | number,
        name: form.value.name.trim(),
        description: form.value.description.trim() || undefined,
        conditionsJson: buildConditionsJson(),
      }
      await tenantGameOnlinePlayerSegmentUpdatePost(input)
      showSuccess(t('message.onlineCenter.playerSegment.updateSuccess'))
    } else {
      await tenantGameOnlinePlayerSegmentAddPost({
        appId: props.appId as string | number,
        name: form.value.name.trim(),
        description: form.value.description.trim() || undefined,
        conditionsJson: buildConditionsJson(),
      })
      showSuccess(t('message.onlineCenter.playerSegment.addSuccess'))
    }
    emit('reloadTable')
    close()
  } catch {
    // 全局拦截器已弹错误提示
  } finally {
    loading.value = false
  }
}

defineExpose({
  openDialog: (row: any) => {
    reset()
    isEditor.value = !!row?.id
    editId.value = row?.id
    editRowVersion.value = row?.rowVersion
    if (row?.id) {
      form.value.name = row.name ?? ''
      form.value.description = row.description ?? ''
      if (row.conditionsJson) {
        try {
          const parsed = JSON.parse(row.conditionsJson) as Array<{ field: string; op: string; value: string }>
          if (Array.isArray(parsed) && parsed.length > 0) {
            conditions.value = parsed.map((c) => ({ field: c.field ?? '', op: c.op ?? '', value: c.value ?? '' }))
          }
        } catch {
          // 不可解析历史数据：留给空构建器，保存时覆盖
        }
      }
    }
    visible.value = true
  },
})
</script>
