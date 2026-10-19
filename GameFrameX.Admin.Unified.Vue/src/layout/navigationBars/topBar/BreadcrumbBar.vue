<template>
  <div v-if="isShowBreadcrumb" class="layout-navbars-breadcrumb">
    <Breadcrumb :model="breadcrumbItems" class="layout-navbars-breadcrumb-hide">
      <template #item="{ item }">
        <span v-if="item.isLast" class="layout-navbars-breadcrumb-span">
          <div>{{ item.label }}</div>
        </span>
        <a v-else @click.prevent="onBreadcrumbClick(item.routeItem)">
          {{ item.label }}
        </a>
      </template>
    </Breadcrumb>
  </div>
</template>

<script setup lang="ts" name="layoutBreadcrumb">
import {reactive, computed, onMounted, watch} from 'vue';
import {onBeforeRouteUpdate, useRoute, useRouter} from 'vue-router';
import other from '../../../utils/other';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../../../stores/themeConfig';
import {useRoutesListStore} from '../../../stores/routesList';
import Breadcrumb from 'primevue/breadcrumb';

// 定义变量内容
const stores = useRoutesListStore();
const storesThemeConfig = useThemeConfigStore();
const {themeConfig} = storeToRefs(storesThemeConfig);
const {routesList} = storeToRefs(stores);
const route = useRoute();
const router = useRouter();
const state = reactive<BreadcrumbState>({
  breadcrumbList: [],
  routeSplit: [],
  routeSplitFirst: '',
  routeSplitIndex: 1,
});

// 构建 PrimeVue Breadcrumb model
const breadcrumbItems = computed(() => {
  return state.breadcrumbList.map((v: RouteItem, k: number) => {
    const label = v.meta.tagsViewName
      ? v.meta.tagsViewName
      : t(v.meta.titleCode || v.meta.title);
    return {
      label: label,
      icon: v.meta.icon,
      isLast: k === state.breadcrumbList.length - 1,
      routeItem: v,
    };
  });
});

const isShowBreadcrumb = computed(() => {
  return true;
});
// 面包屑点击时
const onBreadcrumbClick = (v: RouteItem) => {
  const {redirect} = v;
  if (redirect) router.push(redirect);
};
// 展开/收起左侧菜单点击
const onThemeConfigChange = () => {
  themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
};
// 处理面包屑数据
const getBreadcrumbList = (arr: RouteItems) => {
  arr.forEach((item: RouteItem) => {
    state.routeSplit.forEach((v: string, k: number, arrs: string[]) => {
      if (state.routeSplitFirst === item.path) {
        state.routeSplitFirst += `/${arrs[state.routeSplitIndex]}`;
        state.breadcrumbList.push(item);
        state.routeSplitIndex++;
        if (item.children) getBreadcrumbList(item.children);
      }
    });
  });
};
// 当前路由字符串切割成数组，并删除第一项空内容
const initRouteSplit = (path: string) => {
  state.breadcrumbList = [routesList.value[0]];
  state.routeSplit = path.split('/');
  state.routeSplit.shift();
  state.routeSplitFirst = `/${state.routeSplit[0]}`;
  state.routeSplitIndex = 1;
  getBreadcrumbList(routesList.value);
  if (route.name === 'home' || (route.name === 'notFound' && state.breadcrumbList.length > 1)) state.breadcrumbList.shift();
  if (state.breadcrumbList.length > 0)
    state.breadcrumbList[state.breadcrumbList.length - 1].meta.tagsViewName = other.setTagsViewNameI18n(<RouteToFrom>route);
};
// 页面加载时
onMounted(() => {
  initRouteSplit(route.path);
});
// 监听 isShowBreadcrumb 变化时刷新面包屑数据
watch(isShowBreadcrumb, (val) => {
  if (val) initRouteSplit(route.path);
});
// 路由更新时
onBeforeRouteUpdate((to) => {
  initRouteSplit(to.path);
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb {
  flex: 1;
  height: 100%;
  display: flex;
  align-items: center;

  // 移动端隐藏面包屑，腾出顶栏空间（C143，阈值见 theme/breakpoints.scss）
  @include bp-down(md) {
    display: none;
  }

  .layout-navbars-breadcrumb-span {
    display: flex;
    opacity: 0.7;
    color: var(--p-text-color);
  }

  :deep(.p-breadcrumb) {
    background: transparent;
    padding: 0;

    .p-breadcrumb-list {
      .p-breadcrumb-item {
        .p-breadcrumb-item-link {
          color: var(--p-text-color);
          opacity: 0.7;

          &:hover {
            color: var(--p-primary-color);
          }
        }
      }

      .p-breadcrumb-separator {
        opacity: 0.7;
        color: var(--p-text-color);
      }
    }
  }
}
</style>
