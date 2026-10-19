<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameGoodsEntityAddPost" :updateApi="tenantGameGoodsEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <FormLabel name="configId">{{ t('message.gameBusiness.goods.configId') }}</FormLabel>
      <FormField name="configId" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.configId" :placeholder="t('message.gameBusiness.goods.pleaseEnterConfigId')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel name="classifyId">{{ t('message.gameBusiness.commodity.category') }}</FormLabel>
      <FormField name="classifyId" v-slot="{ props: fProps, invalid, error }">
        <Select v-bind="fProps" v-model="ruleForm.classifyId" showClear filter :options="filteredClassifyIdOptions" optionLabel="label" optionValue="value" :placeholder="t('message.gameBusiness.goods.pleaseSelectClassify')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.name')" :placeholder="t('message.gameBusiness.pleaseEnterName')" maxlength="64"/>
      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.description')" :placeholder="t('message.gameBusiness.pleaseEnterDescription')" maxlength="512"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameGoodsEntityAddPost, tenantGameGoodsEntityUpdatePost } from '../../../../../api/tenantGameGoodsEntity'
import { tenantGameGoodsCatalogueEntitySelectPost } from '../../../../../api/tenantGameGoodsCatalogueEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const props = defineProps({
  title: {type: String, default: ''},
  appId: {type: [Number, String], default: undefined},
})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  configId: [{required: true, message: t('message.gameBusiness.goods.pleaseEnterConfigId'), trigger: 'blur'}],
  name: [{required: true, message: t('message.gameBusiness.pleaseEnterName'), trigger: 'blur'}],
}

const gameGoodsCatalogueEntityClassifyIdDropdownList = ref<any>([])
const filteredClassifyIdList = ref<any>([])

const filteredClassifyIdOptions = computed(() =>
  filteredClassifyIdList.value.map((item: any) => ({
    label: `${item.value} - ${item.label}`,
    value: item.id
  }))
)

const getGameGoodsCatalogueEntityClassifyIdDropdownList = async (appId?: number) => {
  let list = await tenantGameGoodsCatalogueEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE});
  gameGoodsCatalogueEntityClassifyIdDropdownList.value = list.data.data ?? [];
  filteredClassifyIdList.value = [...gameGoodsCatalogueEntityClassifyIdDropdownList.value];
};

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    getGameGoodsCatalogueEntityClassifyIdDropdownList(form.appId)
  }
}

const isEditor = computed({
  get: () => dialogRef.value?.isEditor,
  set: (val) => { if (dialogRef.value) dialogRef.value.isEditor = val },
})

const openDialog = (row: any) => {
  dialogRef.value?.openDialog(row)
}

defineExpose({openDialog, isEditor})
</script>
