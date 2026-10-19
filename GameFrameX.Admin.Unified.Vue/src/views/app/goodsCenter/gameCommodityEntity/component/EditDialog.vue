<template>
  <BaseEditDialog ref="dialogRef" :title="props.title" :addApi="tenantGameCommodityEntityAddPost" :updateApi="tenantGameCommodityEntityUpdatePost" :rules="rules" :beforeOpen="beforeOpen" @reloadTable="emit('reloadTable')">
    <template #default="{ ruleForm, isEditor }">
      <TextField v-model="ruleForm.status" name="status" :label="t('message.gameBusiness.area.status')" :placeholder="t('message.gameBusiness.user.pleaseSelectStatus')" maxlength="64" :disabled="isEditor"/>
      <FormLabel name="categoryId">{{ t('message.gameBusiness.commodity.category') }}</FormLabel>
      <FormField name="categoryId" v-slot="{ props: fProps, invalid, error }">
        <CascadeSelect v-bind="fProps" v-model="ruleForm.categoryId" :options="cascadeOptions" optionLabel="label" optionValue="value" optionGroupLabel="label" optionGroupChildren="items" :placeholder="t('message.gameBusiness.goods.pleaseSelectClassify')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
      <FormLabel>{{ t('message.gameBusiness.commodity.costRmb') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.cost"/></div>
      <TextField v-model="ruleForm.number" name="number" :label="t('message.gameBusiness.commodity.number')" :placeholder="t('message.gameBusiness.commodity.pleaseEnterNumber')" maxlength="20"/>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref, computed} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import CascadeSelect from 'primevue/cascadeselect'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import { tenantGameCommodityEntityAddPost, tenantGameCommodityEntityUpdatePost } from '../../../../../api/tenantGameCommodityEntity'
import { tenantGameGoodsCatalogueEntitySelectPost } from '../../../../../api/tenantGameGoodsCatalogueEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const props = defineProps({title: {type: String, default: ''}, appId: {type: [Number, String], default: undefined}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

const rules = {
  status: [{required: true, message: t('message.gameBusiness.user.pleaseSelectStatus'), trigger: 'change'}],
  categoryId: [{required: true, message: t('message.gameBusiness.goods.pleaseSelectClassify'), trigger: 'change'}],
  number: [{required: true, message: t('message.gameBusiness.commodity.pleaseEnterNumber'), trigger: 'blur'}],
}

const convertTreeToCascade = (data: any[]): any[] => {
  if (!data) return []
  // select 接口返回扁平 {id, value(名称), label(描述)}（目录实体无层级），value 取 id、label 取名称
  return data.map((item: any) => ({
    value: item.id,
    label: item.value,
    items: item.children && item.children.length > 0 ? convertTreeToCascade(item.children) : undefined,
  }))
}

const gameGoodsCatalogueEntityTreeData = ref<any>([])
const cascadeOptions = computed(() => convertTreeToCascade(gameGoodsCatalogueEntityTreeData.value))

const getGameGoodsCatalogueEntityTreeData = async (appId?: number) => {
  let list = await tenantGameGoodsCatalogueEntitySelectPost({appId: appId, pageSize: MAX_SELECT_SIZE});
  gameGoodsCatalogueEntityTreeData.value = list.data.data ?? [];
};

const beforeOpen = (form: any, isEditor: boolean) => {
  const appId = normalizeAppId(props.appId)
  if (appId) form.appId = appId
  if (form.appId) {
    getGameGoodsCatalogueEntityTreeData(form.appId)
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
