<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => tenantGameRedeemCodeUseRecordEntityAddPost(params)"
    :updateApi="(params: any) => tenantGameRedeemCodeUseRecordEntityUpdatePost(params)"
    @reloadTable="emit('reloadTable')"
  >
    <template #default="{ ruleForm }">
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import { tenantGameRedeemCodeUseRecordEntityAddPost, tenantGameRedeemCodeUseRecordEntityUpdatePost } from '../../../../../api/tenantGameRedeemCodeUseRecordEntity'

const props = defineProps({title: {type: String, default: ''}})
const emit = defineEmits(['reloadTable'])
const dialogRef = ref()

defineExpose({
  openDialog: (row: any) => dialogRef.value?.openDialog(row),
  get isEditor() { return dialogRef.value?.isEditor },
  set isEditor(val) { if (dialogRef.value) dialogRef.value.isEditor = val },
})
</script>
