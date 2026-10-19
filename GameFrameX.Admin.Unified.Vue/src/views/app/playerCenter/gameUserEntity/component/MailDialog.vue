<template>
  <div class="gameUserEntity-container">
    <Dialog v-model:visible="dialogVisible" modal :style="{ width: 'min(800px, calc(100vw - 32px))' }" :header="props.title" draggable>
      <Form :key="formKey" ref="formRef" :initialValues="ruleForm" :resolver="resolver" @submit="onFormSubmit">
        <div v-show="false">
          <InputText name="id" v-model="ruleForm.id"/>
        </div>
        <div style="display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
          <FormLabel name="status">{{ t('message.gameBusiness.area.status') }}</FormLabel>
          <FormField name="status" v-slot="{ props: fProps, invalid, error }">
            <Select filter v-bind="fProps" v-model="ruleForm.status" showClear :options="statusOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.user.pleaseSelectStatus')" class="w-full"/>
            <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
          </FormField>
          <FormLabel name="email">{{ t('message.gameBusiness.user.email') }}</FormLabel>
          <FormField name="email" v-slot="{ props: fProps, invalid, error }">
            <InputText v-bind="fProps" v-model="ruleForm.email" :placeholder="t('message.gameBusiness.user.pleaseEnterEmail')" maxlength="64" class="w-full"/>
            <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
          </FormField>
          <FormLabel name="emailPassword">{{ t('message.gameBusiness.user.password') }}</FormLabel>
          <FormField name="emailPassword" v-slot="{ props: fProps, invalid, error }">
            <InputText v-bind="fProps" v-model="ruleForm.emailPassword" :placeholder="t('message.gameBusiness.user.pleaseEnterPassword')" maxlength="64" class="w-full"/>
            <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
          </FormField>
          <FormLabel name="name">{{ t('message.gameBusiness.user.nickname') }}</FormLabel>
          <FormField name="name" v-slot="{ props: fProps, invalid, error }">
            <InputText v-bind="fProps" v-model="ruleForm.name" :placeholder="t('message.gameBusiness.user.pleaseEnterNickname')" maxlength="64" class="w-full"/>
            <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
          </FormField>
        </div>
      </Form>
      <template #footer>
        <div class="flex gap-2 justify-content-end">
          <Button severity="secondary" @click="cancel">{{ t('message.common.button.cancel') }}</Button>
          <Button @click="submit">{{ t('message.common.button.confirm') }}</Button>
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import Form from '@primevue/forms/form'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import {buildFormResolver} from '../../../../../utils/formResolver.ts'
import {useRequiredFields} from '../../../../../composables/useRequiredFields.ts'
import type {FormRules} from '../../../../../composables/types/crud.ts'
import { tenantGameUserEntityAddByMailPost } from '../../../../../api/tenantGameUserEntity'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';

const getEnumStatusData = ref<any>([])

const statusOptions = computed(() =>
  getEnumStatusData.value.map((item: any) => ({
    label: `[${item.value}] ${item.describe}`,
    value: item.value
  }))
)

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])

const formRef = ref()
const formKey = ref(0)
const dialogVisible = ref(false)
const ruleForm = ref<any>({})

const rules = ref<FormRules>({
  email: [{required: true, message: () => t('message.gameBusiness.user.pleaseEnterEmail'), trigger: 'blur'}],
  emailPassword: [{required: true, message: () => t('message.gameBusiness.user.pleaseEnterPassword'), trigger: 'blur'}],
})
useRequiredFields(rules.value)

const resolver = buildFormResolver(rules)

const onFormSubmit = async ({valid}: { valid: boolean }) => {
  if (!valid) return
  await tenantGameUserEntityAddByMailPost(ruleForm.value)
  closeDialog()
}

const openDialog = (row: any) => {
  formKey.value++
  dialogVisible.value = true
}

const closeDialog = () => {
  emit('reloadTable')
  dialogVisible.value = false
}

const cancel = () => {
  dialogVisible.value = false
}

const submit = () => {
  formRef.value?.submit()
}

onMounted(async () => {
  getEnumStatusData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameUserStatus' })).data.data ?? []
})

defineExpose({openDialog})
</script>
