<template>
  <BaseEditDialog
      ref="dialogRef"
      :title="props.title"
      width="min(860px, calc(100vw - 32px))"
      scrollable
      :defaultForm="defaultForm"
      :addApi="tenantGameWebShopEntityAddPost"
      :updateApi="tenantGameWebShopEntityUpdatePost"
      :beforeOpen="beforeOpen"
      :beforeSubmit="beforeSubmit"
      @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
      <TextField v-model="ruleForm.name" name="name" :label="t('message.gameBusiness.webshop.itemName')" :placeholder="t('message.gameBusiness.webshop.itemName')" maxlength="256"/>

      <FormLabel name="logo">{{ t('message.gameBusiness.webshop.itemImage') }}</FormLabel>
      <FormField name="logo" v-slot="{ props: fProps, invalid, error }">
        <div>
          <InputText v-bind="fProps" v-model="ruleForm.logo" :placeholder="t('message.gameBusiness.webshop.itemLogo')" maxlength="256" class="w-full"/>
          <img :src="ruleForm.logo" alt="" aria-hidden="true" style="width:100px;height:100px;object-fit:fill;margin-top:8px;" v-if="ruleForm.logo"/>
        </div>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <TextField v-model="ruleForm.description" name="description" :label="t('message.gameBusiness.webshop.itemDescription')" :placeholder="t('message.gameBusiness.webshop.itemDescription')" maxlength="256"/>
      <TextField v-model="ruleForm.keyWords" name="keyWords" :label="t('message.gameBusiness.webshop.itemKeywords')" :placeholder="t('message.gameBusiness.webshop.itemKeywords')" maxlength="256"/>

      <FormLabel>{{ t('message.gameBusiness.webshop.itemProps') }}</FormLabel>
      <div>
        <div style="display: grid; grid-template-columns: 8fr 6fr 6fr 4fr; gap: 10px">
          <Select :placeholder="t('message.gameBusiness.webshop.selectItem')" v-model="oneItemData.id" :options="getGoodsList" optionLabel="value" optionValue="id" filter class="w-full" @change="onGoodsListChange"/>
          <InputText v-model="oneItemData.count" :placeholder="t('message.gameBusiness.webshop.itemCount')" class="w-full"/>
          <InputText v-model="oneItemData.star" :placeholder="t('message.gameBusiness.webshop.itemStar')" class="w-full"/>
          <Button @click="addOneItem">{{ t('message.gameBusiness.webshop.add') }}</Button>
        </div>
        <div style="margin-top: 20px;">
          <CrudDataTable :value="itemDataList" style="width: 100%" showGridlines stripedRows :showPagination="false">
            <Column field="Label" :header="t('message.gameBusiness.name')" style="width: 200px" alignHeader="center" align="center"/>
            <Column field="Count" :header="t('message.gameBusiness.webshop.count')"/>
            <Column field="Star" :header="t('message.gameBusiness.goods.starLevel')"/>
            <Column :header="t('message.common.table.action')" style="width: 140px" alignHeader="center" align="center">
              <template #body="{ data }">
                <Button link severity="danger" @click="delOneItem(data)">
                  <i :class="faTrash.className" class="mr-1"></i>{{ t('message.common.button.delete') }}
                </Button>
              </template>
            </Column>
          </CrudDataTable>
        </div>
      </div>

      <FormLabel name="price">{{ t('message.gameBusiness.webshop.retailPrice') }}</FormLabel>
      <FormField name="price" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.price" :placeholder="t('message.gameBusiness.webshop.retailPrice')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel name="realPrice">{{ t('message.gameBusiness.webshop.realPrice') }}</FormLabel>
      <FormField name="realPrice" v-slot="{ props: fProps, invalid, error }">
        <InputText v-bind="fProps" v-model="ruleForm.realPrice" :placeholder="t('message.gameBusiness.webshop.realPrice')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>

      <FormLabel>{{ t('message.gameBusiness.webshop.isUp') }}</FormLabel>
      <div><ToggleSwitch v-model="ruleForm.isUp"/></div>

      <FormLabel name="weight">{{ t('message.gameBusiness.webshop.weight') }}</FormLabel>
      <FormField name="weight" v-slot="{ props: fProps, invalid, error }">
        <InputNumber v-bind="fProps" v-model="ruleForm.weight" :placeholder="t('message.gameBusiness.webshop.weight')" class="w-full"/>
        <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
      </FormField>
    </template>
  </BaseEditDialog>
</template>
<script lang="ts" setup>
import {ref} from 'vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Button from 'primevue/button'
import Column from 'primevue/column'
import Message from 'primevue/message'
import FormField from '@primevue/forms/formfield'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import CrudDataTable from '/@/components/crud-data-table/index.vue'
import {TextField} from '../../../../../components/crud-form-fields'
import {faTrash} from '../../../../../utils/icons.ts'
import { tenantGameGoodsEntitySelectStringPost } from '../../../../../api/tenantGameGoodsEntity'
import { tenantGameWebShopEntityAddPost, tenantGameWebShopEntityUpdatePost } from '../../../../../api/tenantGameWebShopEntity'
import {normalizeAppId} from '../../../../../utils/appWorkspace.ts'
import {MAX_SELECT_SIZE} from '../../../../../constants/pagination.ts'

const props = defineProps({
  title: {
    type: String,
    default: '',
  },
  appId: {
    type: [Number, String],
    default: undefined,
  },
});
const emit = defineEmits(['reloadTable']);
const dialogRef = ref()

const getGoodsList = ref<any>([]);
const oneItemData = ref<any>({})
const itemDataList = ref<any>([])

const defaultForm = () => ({
  id: 0,
  appId: normalizeAppId(props.appId),
  itemJson: '[]',
})

const beforeOpen = (form: any, isEditor: boolean) => {
  itemDataList.value = []
  oneItemData.value = {}
  if (form.appId) {
    loadGoods(form.appId)
  } else {
    getGoodsList.value = []
  }
  if (isEditor) {
    try {
      const items = JSON.parse(form.itemJson);
      if (Array.isArray(items)) {
        items.forEach((ele: any) => {
          itemDataList.value.push(ele)
        });
      }
    } catch {
      form.itemJson = '[]'
    }
  }
  return form
}

const beforeSubmit = (form: any) => {
  form.itemJson = JSON.stringify(itemDataList.value)
  return form
}

const loadGoods = async (appId?: number) => {
  getGoodsList.value = (await tenantGameGoodsEntitySelectStringPost({appId: appId, pageSize: MAX_SELECT_SIZE})).data.data ?? []
}

const addOneItem = () => {
  var selectItem = oneItemData.value
  if (!selectItem.id || selectItem.id == 0) {
    return
  }
  var item = getGoodsList.value.find((x: any) => x.id == selectItem.id)
  if (item != null) {
    itemDataList.value.push({
      Id: item.id,
      Label: item.label,
      Count: selectItem.count,
      Star: selectItem.star
    })
    oneItemData.value = {}
  }
}

const delOneItem = (row: any) => {
  itemDataList.value = itemDataList.value.filter((x: any) => x.Id != row.Id)
}

const onGoodsListChange = (itemId: number) => {
  var goodItem = getGoodsList.value.filter((x: any) => x.id == itemId)
  if (goodItem.length > 0) {
    oneItemData.value = goodItem[0]
    oneItemData.value.star = 0
  }
}

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
})
</script>
