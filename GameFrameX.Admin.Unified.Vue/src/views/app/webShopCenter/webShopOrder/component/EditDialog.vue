<template>
  <div class="gameGoodsEntity-container">
    <Dialog v-model:visible="visible" modal draggable :header="props.title"
            :style="{ width: 'min(450px, calc(100vw - 32px))' }">
      <p>{{ t('message.gameBusiness.webshop.confirmRedo') }}</p>
      <template #footer>
        <Button severity="secondary" @click="close">{{ t('message.common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('message.common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>
<script lang="ts" setup>
import {ref} from 'vue'
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import { tenantGameWebShopOrderEntityRedoPost } from '../../../../../api/tenantGameWebShopOrderEntity'

var props = defineProps({
  title: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['reloadTable'])

const visible = ref(false)
const orderId = ref<any>()

const close = () => {
  visible.value = false
}

const openDialog = (row: any) => {
  orderId.value = row.id
  visible.value = true
}

const submit = async () => {
  await tenantGameWebShopOrderEntityRedoPost({id: orderId.value})
  emit('reloadTable')
  close()
}

defineExpose({openDialog})
</script>
