<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => addNotice(params)"
    :updateApi="(params: any) => updateNotice(params)"
    :rules="rules"
    :defaultForm="defaultForm"
    :beforeSubmit="beforeSubmit"
    width="min(900px, calc(100vw - 32px))"
    scrollable
    scrollMaxHeight="70vh"
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.title" name="title" :label="t('system.notice.noticeTitle')" :placeholder="t('system.notice.noticeTitle')" maxlength="32"/>

      <FormLabel name="type">{{ t('common.label.type') }}</FormLabel>
      <FormField name="type" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.type" :options="typeOptions" optionLabel="label" optionValue="value"
                :placeholder="t('common.label.type')" editable class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="content">{{ t('system.notice.noticeContent') }}</FormLabel>
      <FormField name="content" v-slot="{ invalid, error }">
        <Editor v-model:getHtml="ruleForm.content"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import Editor from '/@/components/rich-editor/index.vue'
import {showError} from '../../../../../utils/message.ts'
import {tenantNoticeAddPost, tenantNoticeUpdatePost} from '../../../../../api/tenantNotice';

var props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['query'])
const dialogRef = ref()
const addNotice = (params: any) => tenantNoticeAddPost(params)
const updateNotice = (params: any) => tenantNoticeUpdatePost(params)

const typeOptions = [
  {label: t('system.notice.notice'), value: 1},
  {label: t('system.notice.announcement'), value: 2},
]

const defaultForm = () => ({type: 1, title: '', content: ''})

const rules = {
  title: [
    {required: true, message: t('system.notice.noticeTitle'), trigger: 'blur'},
    {max: 32, message: t('system.notice.noticeTitle'), trigger: 'blur'},
  ],
  type: [{required: true, message: t('common.label.type'), trigger: 'change'}],
}

const normalizeContent = (value: any) => {
  const content = String(value ?? '').trim()
  return content === '<p><br></p>' ? '' : content
}

const beforeSubmit = (form: any) => {
  const content = normalizeContent(form.content)
  if (!content) {
    showError(t('system.notice.contentRequired'))
    return false
  }
  // type 在 PrimeVue Select(editable) 下可能被还原成字符串，这里强制转回数字枚举，避免后端 NoticeTypeEnum 解析失败。
  const type = Number(form.type)
  return {...form, content, type: isNaN(type) ? form.type : type}
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>
