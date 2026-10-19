<template>
  <div class="layout-search-dialog">
    <Dialog v-model:visible="state.isShowSearch" modal :showHeader="false" :showFooter="false"
            @hide="closeSearch" position="top" class="search-dialog">
      <div class="search-dialog-content">
        <AutoComplete
            v-model="state.menuQuery"
            :suggestions="state.searchResults"
            @complete="onSearch"
            :placeholder="t('message.user.searchPlaceholder')"
            ref="layoutMenuAutocompleteRef"
            @item-select="onHandleSelect"
            class="search-autocomplete"
        >
          <template #prefix>
            <i class="fa-solid fa-magnifying-glass" style="font-size: 14px; margin-right: 8px"></i>
          </template>
          <template #option="{ option }">
            <div style="display: flex; align-items: center">
              <SvgIcon :name="option.meta?.icon" class="mr5"/>
              {{ t(option.meta?.titleCode || option.meta?.title) }}
            </div>
          </template>
        </AutoComplete>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="layoutBreadcrumbSearch">
import {reactive, ref, nextTick} from 'vue';
import {useRouter} from 'vue-router';
import {storeToRefs} from 'pinia';
import {useTagsViewRoutesStore} from '../../../stores/tagsViewRoutes';
import Dialog from 'primevue/dialog';
import AutoComplete from 'primevue/autocomplete';

// 定义变量内容
const storesTagsViewRoutes = useTagsViewRoutesStore();
const {tagsViewRoutes} = storeToRefs(storesTagsViewRoutes);
const layoutMenuAutocompleteRef = ref();
const router = useRouter();
const state = reactive({
  isShowSearch: false as boolean,
  menuQuery: '' as string,
  tagsViewList: [] as RouteItem[],
  searchResults: [] as RouteItem[],
});

// 搜索弹窗打开
const openSearch = () => {
  state.menuQuery = '';
  state.isShowSearch = true;
  initTageView();
  nextTick(() => {
    setTimeout(() => {
      if (layoutMenuAutocompleteRef.value) {
        layoutMenuAutocompleteRef.value.$el.querySelector('input')?.focus();
      }
    });
  });
};
// 搜索弹窗关闭
const closeSearch = () => {
  state.isShowSearch = false;
};
// 菜单搜索数据过滤 (PrimeVue AutoComplete @complete handler)
const onSearch = (event: { query: string }) => {
  const queryString = event.query;
  if (!queryString) {
    state.searchResults = [...state.tagsViewList];
  } else {
    state.searchResults = state.tagsViewList.filter(createFilter(queryString));
  }
};
// 菜单搜索过滤
const createFilter = (queryString: string) => {
  return (restaurant: RouteItem) => {
    return (
        restaurant.path.toLowerCase().indexOf(queryString.toLowerCase()) > -1 ||
        restaurant.meta!.title!.toLowerCase().indexOf(queryString.toLowerCase()) > -1 ||
        t(restaurant.meta!.titleCode || restaurant.meta!.title).indexOf(queryString.toLowerCase()) > -1
    );
  };
};
// 初始化菜单数据
const initTageView = () => {
  if (state.tagsViewList.length > 0) return false;
  tagsViewRoutes.value.map((v: RouteItem) => {
    if (!v.meta?.isHide && v.type !== 1) state.tagsViewList.push({...v});
  });
};
// 当前菜单选中时
const onHandleSelect = (event: { value: RouteItem }) => {
  const item = event.value;
  let {path, redirect} = item;
  if (item.meta?.isLink && !item.meta?.isIframe) window.open(item.meta?.isLink);
  else if (redirect) router.push(redirect);
  else router.push(path);
  closeSearch();
};

// 暴露变量
defineExpose({
  openSearch,
});
</script>

<style scoped lang="scss">
.layout-search-dialog {
  position: relative;

  .search-dialog-content {
    display: flex;
    justify-content: center;
    padding: 20px 0;
  }

  .search-autocomplete {
    width: 100%;
  }

  :deep(.p-dialog) {
    width: min(600px, calc(100vw - 32px));

    .p-dialog-header {
      display: none;
    }
  }
}
</style>
