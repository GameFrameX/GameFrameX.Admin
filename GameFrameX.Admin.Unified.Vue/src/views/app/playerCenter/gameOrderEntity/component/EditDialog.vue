<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" scrollable scrollMaxHeight="65vh"
                  :addApi="tenantGameOrderEntityAddPost" :updateApi="tenantGameOrderEntityUpdatePost" :rules="rules"
                  :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="status">{{ t('message.gameBusiness.order.orderStatus') }}</FormLabel>
      <FormField name="status" v-slot="{ props: fProps, invalid, error }">
        <Select filter v-bind="fProps" v-model="ruleForm.status" :options="statusOptions" optionLabel="label"
                optionValue="value" :placeholder="t('message.gameBusiness.order.pleaseSelectOrderStatus')" class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel>{{ t('message.gameBusiness.order.moneyAmountCents') }}</FormLabel>
      <FormField name="amount" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.amount" class="w-full" />
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.payChannel" name="payChannel" :label="t('message.gameBusiness.order.payChannel')" />

      <TextField v-model="ruleForm.sku" name="sku" :label="t('message.gameBusiness.order.commoditySku')" />

      <FormLabel>{{ t('message.gameBusiness.order.isSandboxEnv') }}</FormLabel>
      <FormField name="isSandbox">
        <ToggleSwitch v-model="ruleForm.isSandbox" />
      </FormField>

      <TextField v-model="ruleForm.orderId" name="orderId" :label="t('message.gameBusiness.order.thirdPartyOrderId')" />
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed, onMounted} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameOrderEntityAddPost, tenantGameOrderEntityUpdatePost } from '../../../../../api/tenantGameOrderEntity'
import {systemEnumEnumDataListGet} from '../../../../../api/systemEnum';
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const enumStatusData = ref<any>([])
const statusOptions = computed(() =>
  (enumStatusData.value || []).map((item: any) => ({label: `[${item.value}] ${item.describe}`, value: item.value}))
)

const rules = {
  status: [{required: true, message: t('message.gameBusiness.pleaseSelectOrderStatus'), trigger: 'blur'}],
}

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
}

onMounted(async () => {
  enumStatusData.value = (await systemEnumEnumDataListGet({ EnumName: 'OrderStatusEnum' })).data.data ?? []
})

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>