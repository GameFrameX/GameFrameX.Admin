<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => globalMenuAddPost(params)"
    :updateApi="(params: any) => globalMenuUpdatePost(params)"
    :rules="rules"
    idCheckMode="system"
    scrollable
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <div style="grid-column: 1 / -1; display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
        <FormLabel>{{ t('system.menu.parentMenu') }}</FormLabel>
        <div>
          <CascadeSelect v-model="ruleForm.pid" :options="cascadeMenuOptions" optionLabel="label" optionValue="value" optionGroupLabel="label" optionGroupChildren="items" :placeholder="t('system.menu.selectParentMenu')" class="w100" showClear/>
        </div>
        <FormLabel name="type">{{ t('system.menu.menuType') }}</FormLabel>
        <FormField name="type" v-slot="{ props: fProps, invalid, error }">
          <div class="flex gap-4">
            <div class="flex align-items-center">
              <RadioButton v-bind="fProps" v-model="ruleForm.type" :value="1" name="type" inputId="type_1"/>
              <label for="type_1" class="ml-2">{{ t('system.menu.directory') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-bind="fProps" v-model="ruleForm.type" :value="2" name="type" inputId="type_2"/>
              <label for="type_2" class="ml-2">{{ t('system.menu.menu') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-bind="fProps" v-model="ruleForm.type" :value="3" name="type" inputId="type_3"/>
              <label for="type_3" class="ml-2">{{ t('system.menu.button') }}</label>
            </div>
          </div>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
        <FormLabel name="title">{{ t('system.menu.menuName') }}</FormLabel>
        <FormField name="title" v-slot="{ props: fProps, invalid, error }">
          <InputText v-bind="fProps" v-model="ruleForm.title" :placeholder="t('system.menu.menuName')" class="w-full"/>
          <Message v-if="invalid" severity="error" size="small" variant="simple">{{ error }}</Message>
        </FormField>
        <FormLabel>{{ t('system.menu.titleCode') }}</FormLabel>
        <div><InputText v-model="ruleForm.titleCode" :placeholder="t('system.menu.titleCodePlaceholder')" class="w-full"/></div>
      </div>
      <!-- 目录/菜单字段 -->
      <template v-if="ruleForm.type === 1 || ruleForm.type === 2">
        <div style="grid-column: 1 / -1; display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start; margin-top: 20px;">
          <FormLabel>{{ t('system.menu.routeName') }}</FormLabel>
          <div><InputText v-model="ruleForm.name" :placeholder="t('system.menu.routeName')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.routePath') }}</FormLabel>
          <div><InputText v-model="ruleForm.path" :placeholder="t('system.menu.routePath')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.componentPath') }}</FormLabel>
          <div><InputText v-model="ruleForm.component" :placeholder="t('system.menu.componentPath')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.menuIcon') }}</FormLabel>
          <div>
            <IconSelector v-model="ruleForm.icon" :size="getGlobalComponentSize" :placeholder="t('system.menu.menuIcon')" type="all"/>
          </div>
          <FormLabel>{{ t('system.menu.redirect') }}</FormLabel>
          <div><InputText v-model="ruleForm.redirect" :placeholder="t('system.menu.redirectAddress')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.linkUrl') }}</FormLabel>
          <div><InputText v-model="ruleForm.outLink" :placeholder="t('system.menu.linkUrlTip')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.menuSort') }}</FormLabel>
          <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
          <FormLabel>{{ t('system.menu.isHidden') }}</FormLabel>
          <div class="flex gap-4">
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isHide" :value="true" name="isHide" inputId="isHide_true"/>
              <label for="isHide_true" class="ml-2">{{ t('system.menu.hidden') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isHide" :value="false" name="isHide" inputId="isHide_false"/>
              <label for="isHide_false" class="ml-2">{{ t('system.menu.notHidden') }}</label>
            </div>
          </div>
          <FormLabel>{{ t('system.menu.isCache') }}</FormLabel>
          <div class="flex gap-4">
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isKeepAlive" :value="true" name="isKeepAlive" inputId="isKeepAlive_true"/>
              <label for="isKeepAlive_true" class="ml-2">{{ t('system.menu.cached') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isKeepAlive" :value="false" name="isKeepAlive" inputId="isKeepAlive_false"/>
              <label for="isKeepAlive_false" class="ml-2">{{ t('system.menu.notCached') }}</label>
            </div>
          </div>
          <FormLabel>{{ t('system.menu.isFixed') }}</FormLabel>
          <div class="flex gap-4">
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isAffix" :value="true" name="isAffix" inputId="isAffix_true"/>
              <label for="isAffix_true" class="ml-2">{{ t('system.menu.fixed') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isAffix" :value="false" name="isAffix" inputId="isAffix_false"/>
              <label for="isAffix_false" class="ml-2">{{ t('system.menu.notFixed') }}</label>
            </div>
          </div>
          <FormLabel>{{ t('system.menu.isIframe') }}</FormLabel>
          <div class="flex gap-4">
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isIframe" :value="true" name="isIframe" inputId="isIframe_true"/>
              <label for="isIframe_true" class="ml-2">{{ t('system.menu.iframe') }}</label>
            </div>
            <div class="flex align-items-center">
              <RadioButton v-model="ruleForm.isIframe" :value="false" name="isIframe" inputId="isIframe_false"/>
              <label for="isIframe_false" class="ml-2">{{ t('system.menu.notIframe') }}</label>
            </div>
          </div>
        </div>
      </template>
      <!-- 按钮字段 -->
      <template v-if="ruleForm.type === 3">
        <div style="grid-column: 1 / -1; display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start; margin-top: 20px;">
          <FormLabel>{{ t('system.menu.permission') }}</FormLabel>
          <div><InputText v-model="ruleForm.permission" :placeholder="t('system.menu.permission')" class="w-full"/></div>
          <FormLabel>{{ t('system.menu.menuSort') }}</FormLabel>
          <div><InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/></div>
        </div>
      </template>
      <!-- 公共字段 -->
      <div style="grid-column: 1 / -1; display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start; margin-top: 20px;">
        <FormLabel>{{ t('common.label.enable') }}</FormLabel>
        <div><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
        <FormLabel>{{ t('common.label.remark') }}</FormLabel>
        <div><Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w-full" autoResize/></div>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>

import {computed, ref} from 'vue'
import FormField from '@primevue/forms/formfield'
import Message from 'primevue/message'
import CascadeSelect from 'primevue/cascadeselect'
import IconSelector from '/@/components/icon-selector/index.vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import other from '../../../../../utils/other.ts'
import {globalMenuAddPost, globalMenuUpdatePost} from '../../../../../api/globalMenu';
import {GlobalMenuEntity} from '../../../../../api/_shared/data-contracts.ts'

import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import RadioButton from 'primevue/radiobutton';
import ToggleSwitch from 'primevue/toggleswitch';

var props = defineProps({
  title: {type: String, default: ''},
  menuData: {type: Array<GlobalMenuEntity>, default: () => []},
})
const emit = defineEmits(['query'])
const dialogRef = ref()

const getGlobalComponentSize = computed(() => {
  return other.globalComponentSize()
})

const convertMenuToCascade = (data: GlobalMenuEntity[]): any[] => {
  if (!data) return []
  return data.map((item: any) => ({
    value: item.id,
    label: item.title ?? '',
    items: item.children && item.children.length > 0 ? convertMenuToCascade(item.children) : undefined,
  }))
}

const cascadeMenuOptions = computed(() => convertMenuToCascade(props.menuData))

const rules = {
  type: [{required: true, message: t('system.menu.menuType') + t('common.form.required'), trigger: 'blur'}],
  title: [{required: true, message: t('system.menu.menuName') + t('common.form.required'), trigger: 'blur'}],
}

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
