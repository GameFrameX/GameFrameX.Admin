<template>
  <ConfirmDialog />
  <ConfirmDialog group="danger">
    <template #message="slotProps">
      <div class="flex align-items-start gap-3 p-3">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500 mt-1" />
        <div>
          <p v-if="slotProps.message.header" class="font-semibold text-lg mb-2">{{ slotProps.message.header }}</p>
          <p class="mb-0">{{ slotProps.message.message }}</p>
        </div>
      </div>
    </template>
  </ConfirmDialog>
</template>

<script setup lang="ts">
import ConfirmDialog from 'primevue/confirmdialog'
</script>
