<template>
  <ContextMenu ref="cmRef" :model="menuModel">
    <template #item="{ item, props }">
      <a class="flex align-items-center" v-bind="props.action">
        <SvgIcon :name="item.data.icon"/>
        <span class="ml-2">{{ t(item.label) }}</span>
      </a>
    </template>
  </ContextMenu>
</template>

<script setup lang="ts" name="layoutTagsViewContextmenu">
import {ref, computed} from 'vue';
import ContextMenu from 'primevue/contextmenu';

const emit = defineEmits(['currentContextmenuClick']);

const cmRef = ref();
const currentItem = ref<RouteItem>();

const baseItems = [
  {contextMenuClickId: 0, txt: 'message.tagsView.refresh', icon: 'fa-solid fa-rotate-right'},
  {contextMenuClickId: 1, txt: 'message.tagsView.close', icon: 'fa-solid fa-xmark'},
  {contextMenuClickId: 2, txt: 'message.tagsView.closeOther', icon: 'fa-solid fa-circle-xmark'},
  {contextMenuClickId: 3, txt: 'message.tagsView.closeAll', icon: 'fa-solid fa-folder-minus'},
  {contextMenuClickId: 4, txt: 'message.tagsView.fullscreen', icon: 'fa-solid fa-expand'},
];

const menuModel = computed(() =>
    baseItems.map((item) => ({
      label: item.txt,
      data: {icon: item.icon},
      visible: !(item.contextMenuClickId === 1 && currentItem.value?.meta?.isAffix),
      command: () => {
        emit('currentContextmenuClick', Object.assign({}, {contextMenuClickId: item.contextMenuClickId}, currentItem.value));
      },
    }))
);

const openContextmenu = (item: RouteItem, event: MouseEvent) => {
  currentItem.value = item;
  cmRef.value.show(event);
};

defineExpose({openContextmenu});
</script>
