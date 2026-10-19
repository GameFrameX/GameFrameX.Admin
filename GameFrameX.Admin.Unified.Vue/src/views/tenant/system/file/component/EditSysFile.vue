<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantFileUpdatePost(params)"
    :updateApi="(params: any) => tenantFileUpdatePost(params)"
    idCheckMode="system"
    @reloadTable="emits('query')"
  >
    <template #default="{ ruleForm }">
      <FormLabel>{{ t('system.file.fileName') }}</FormLabel>
      <div><InputText v-model="ruleForm.fileName" :placeholder="t('system.file.fileName')" class="w100"/></div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="systemEditFile">

import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {tenantFileUpdatePost} from '../../../../../api/tenantFile';

const props = defineProps({title: String, systemFileId: Number})
const emits = defineEmits(['query'])
const dialogRef = ref()

defineExpose({openDialog: (row: any) => dialogRef.value?.openDialog(row)})
</script>
