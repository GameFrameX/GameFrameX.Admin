<template>
  <Dialog v-model:visible="dialogVisible" modal :draggable="draggable" :header="title" :style="{ width: width }" class="base-edit-dialog">
    <Form :key="formKey" ref="formRef" :initialValues="ruleForm" :resolver="resolver" @submit="onFormSubmit">
      <ScrollPanel v-if="scrollable" :style="{ maxHeight: scrollMaxHeight }">
        <!-- id 走 hidden input：v-show 隐藏仍会被浏览器当用户名自动填充（表单含 Password 字段时），
             导致新增被填成登录账号、误判为编辑走 update 接口（E2E 实测 400） -->
        <InputText type="hidden" name="id" v-model="ruleForm.id" autocomplete="off"/>
        <div class="base-edit-dialog-grid" :style="gridVars">
          <slot :ruleForm="ruleForm" :isEditor="isEditor"/>
        </div>
      </ScrollPanel>
      <template v-else>
        <InputText type="hidden" name="id" v-model="ruleForm.id" autocomplete="off"/>
        <div class="base-edit-dialog-grid" :style="gridVars">
          <slot :ruleForm="ruleForm" :isEditor="isEditor"/>
        </div>
      </template>
    </Form>
    <template #footer>
      <slot name="footer">
        <div class="base-edit-dialog-footer">
          <Button severity="secondary" @click="cancel">{{ t('message.common.button.cancel') }}</Button>
          <Button :loading="submitting" @click="submit">{{ t('message.common.button.confirm') }}</Button>
        </div>
      </slot>
    </template>
  </Dialog>
</template>

<script lang="ts" setup>
import {computed} from 'vue'
import {useEditDialog} from '../../composables/useEditDialog'
import {useRequiredFields} from '../../composables/useRequiredFields'
import type {FormRules, ApiFn} from '../../composables/types/crud'

const props = withDefaults(defineProps<{
  title?: string
  width?: string | number
  labelWidth?: string | number
  fieldGap?: string
  draggable?: boolean
  scrollable?: boolean
  scrollMaxHeight?: string
  addApi: ApiFn
  updateApi: ApiFn
  rules?: FormRules
  defaultForm?: () => any
  idCheckMode?: 'game' | 'system'
  cloneStrategy?: 'json' | 'structured' | ((row: any) => any)
  beforeOpen?: (form: any, isEditor: boolean) => any
  beforeSubmit?: (form: any, isEditor: boolean) => any
  afterSubmit?: (form: any, isEditor: boolean) => void
}>(), {
  title: '',
  width: 'min(720px, calc(100vw - 32px))',
  labelWidth: '120px',
  fieldGap: '12px 16px',
  draggable: true,
  scrollable: false,
  scrollMaxHeight: '65vh',
  idCheckMode: 'game',
})

// 表单网格的标签列宽 / 间距通过 CSS 变量下发给字段组（BaseEditFieldGroup）共用，
// 保证弹窗内分组内外标签对齐、间距一致；超长标签请按页面传 labelWidth 覆盖。
const gridVars = computed(() => {
  const lw = props.labelWidth
  return {
    '--bed-label-width': typeof lw === 'number' ? `${lw}px` : (lw || '120px'),
    '--bed-gap': props.fieldGap || '12px 16px',
  } as Record<string, string>
})

const emit = defineEmits<{
  (e: 'reloadTable'): void
}>()

const {
  dialogVisible,
  ruleForm,
  formRef,
  formKey,
  isEditor,
  submitting,
  resolver,
  onFormSubmit,
  openDialog,
  closeDialog,
  cancel,
  submit: doSubmit,
} = useEditDialog({
  addApi: props.addApi,
  updateApi: props.updateApi,
  rules: props.rules,
  defaultForm: props.defaultForm,
  idCheckMode: props.idCheckMode,
  cloneStrategy: props.cloneStrategy,
  beforeOpen: props.beforeOpen,
  beforeSubmit: props.beforeSubmit,
  afterSubmit: (form, editor, response) => {
    props.afterSubmit?.(form, editor, response)
    emit('reloadTable')
  },
})

useRequiredFields(props.rules)

async function submit() {
  await doSubmit()
}

defineExpose({openDialog, isEditor, ruleForm, submit, cancel})
</script>

<style scoped>
:deep(.p-select),
:deep(.p-inputnumber) {
  width: 100%;
}

/* ScrollPanel 自定义滚动条悬浮在右侧，给内容留右内边距避免出现滚动时压住字段右边缘 */
:deep(.p-scrollpanel-content) {
  padding-right: 10px;
}

.base-edit-dialog-grid {
  display: grid;
  grid-template-columns: var(--bed-label-width, 120px) minmax(0, 1fr);
  gap: var(--bed-gap, 12px 16px);
  align-items: start;
}

.base-edit-dialog-footer {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: flex-end;
}

@media (max-width: 640px) {
  .base-edit-dialog-grid {
    grid-template-columns: minmax(0, 1fr);
  }

  .base-edit-dialog-footer {
    :deep(.p-button) {
      flex: 1 1 120px;
      min-height: 44px;
    }
  }
}
</style>
