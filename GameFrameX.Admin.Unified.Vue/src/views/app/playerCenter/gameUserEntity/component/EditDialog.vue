<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameUserEntityUpdatePost" :updateApi="tenantGameUserEntityUpdatePost" :rules="rules" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm }">
      <FormLabel name="status">{{ t('message.gameBusiness.area.status') }}</FormLabel>
      <FormField name="status" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.status" showClear :options="statusOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.user.pleaseSelectStatus')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.user.nickname')" :placeholder="t('message.gameBusiness.user.pleaseEnterNickname')" maxlength="64"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.redeemcode.description')" :placeholder="t('message.gameBusiness.redeemcode.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameUserEntityUpdatePost } from '../../../../../api/tenantGameUserEntity'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const getEnumStatusData = ref<any>([])

const statusOptions = computed(() =>
  getEnumStatusData.value.map((item: any) => ({
    label: `[${item.value}] ${item.describe}`,
    value: item.value
  }))
)

const rules = {
  name: [{required: true, message: t('message.gameBusiness.user.pleaseEnterNickname'), trigger: 'blur'}],
}

onMounted(async () => {
  getEnumStatusData.value = (await systemEnumEnumDataListGet({ EnumName: 'GameUserStatus' })).data.data ?? []
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
