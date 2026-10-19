<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantWechatUserAddPost(params)"
    :updateApi="(params: any) => tenantWechatUserUpdatePost(params)"
    idCheckMode="system"
    @reloadTable="emit('query')"
  >
    <template #default="{ ruleForm }">
      <FormLabel>{{ t('common.label.nickname') }}</FormLabel>
      <div><InputText v-model="ruleForm.nickName" :placeholder="t('common.label.nickname')" class="w100"/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>

import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {tenantWechatUserAddPost, tenantWechatUserUpdatePost} from '../../../../../api/tenantWechatUser';

import InputText from 'primevue/inputtext';

var props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['query'])
const dialogRef = ref()

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
