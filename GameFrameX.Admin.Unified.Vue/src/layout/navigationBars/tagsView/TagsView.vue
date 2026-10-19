<template>
  <div class="layout-navbars-tagsview">
    <div ref="scrollbarRef" class="tagsview-scrollbar" @wheel.prevent="onHandleScroll">
      <ul class="layout-navbars-tagsview-ul" :class="setTagsStyle" ref="tagsUlRef">
        <TagsViewItem
            v-for="(v, k) in tagsViewList"
            :key="k"
            :item="v"
            :active="isActive(v)"
            :index="k"
            :ref="
							(el) => {
								if (el) tagsRefs[k] = (el as any)?.$el ?? el;
							}
						"
            @contextmenu="onContextmenu"
            @mousedown="onMousedownMenu"
            @click="onTagsClick"
            @refresh="refreshCurrentTagsView(route.fullPath)"
            @close="closeCurrentTagsView"
        />
      </ul>
    </div>
    <Contextmenu ref="contextmenuRef" @currentContextmenuClick="onCurrentContextmenuClick"/>
  </div>
</template>

<script setup lang="ts" name="layoutTagsView">
import {
  defineAsyncComponent,
  reactive,
  onMounted,
  computed,
  ref,
  nextTick,
  onBeforeMount,
  onUnmounted,
  watch
} from 'vue';
import {useRoute, useRouter, onBeforeRouteUpdate} from 'vue-router';
import {storeToRefs} from 'pinia';
import {useTagsViewRoutesStore} from '../../../stores/tagsViewRoutes';
import {useThemeConfigStore} from '../../../stores/themeConfig';
import {useKeepAliveNamesStore} from '../../../stores/keepAliveNames';
import {useRoutesListStore} from '../../../stores/routesList';
import {isObjectValueEqual} from '../../../utils/arrayOperation';
import mittBus from '../../../utils/mitt';
import TagsViewItem from './components/TagsViewItem.vue';
import {useTagsContextMenu} from './composables/useTagsContextMenu';
import {useTagsViewScroll} from './composables/useTagsViewScroll';

// 引入组件
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Contextmenu = defineAsyncComponent(() => import('/@/layout/navigationBars/tagsView/ContextMenu.vue'));

// 定义变量内容
const storesThemeConfig = useThemeConfigStore();
const storesTagsViewRoutes = useTagsViewRoutesStore();
const storesRoutesList = useRoutesListStore();
const {themeConfig} = storeToRefs(storesThemeConfig);
const {tagsViewRoutes, tagsViewList} = storeToRefs(storesTagsViewRoutes);
const {routesList} = storeToRefs(storesRoutesList);
const storesKeepALiveNames = useKeepAliveNamesStore();
const route = useRoute();
const router = useRouter();
const state = reactive<TagsViewState>({
  routeActive: '',
  routePath: route.path,
  sortable: '',
  tagsRefsIndex: 0,
  tagsViewRoutesList: [],
});

// 滚动定位 + 拖拽支持
const {
  tagsRefs,
  scrollbarRef,
  tagsUlRef,
  onHandleScroll,
  getTagsRefsIndex,
  initSortable,
} = useTagsViewScroll({tagsViewList, state});

const contextmenuRef = ref();

// 动态设置 tagsView 风格样式
const setTagsStyle = computed(() => 'tags-style-one');
// 获取布局配置信息
const getThemeConfig = computed(() => {
  return themeConfig.value;
});
// 设置 tagsView 高亮
const isActive = (v: RouteItem) => {
  return v.path === state.routePath;
};
// 处理 tagsView 高亮（多标签详情时使用，单标签详情未使用）
const setTagsViewHighlight = (v: RouteToFrom) => {
  let params = v.query && Object.keys(v.query).length > 0 ? v.query : v.params;
  if (!params || Object.keys(params).length <= 0) return v.path;
  let path = '';
  for (let i in params) {
    path += params[i];
  }
  // 判断是否是动态路由（xxx/:id/:name"）
  return `${v.meta?.isDynamic ? v.meta.isDynamicPath : v.path}-${path}`;
};
// 获取 pinia 中的 tagsViewRoutes 列表
const getTagsViewRoutes = async () => {
  state.routeActive = await setTagsViewHighlight(route);
  state.routePath = (await route.meta.isDynamic) ? route.meta.isDynamicPath : route.path;
  state.tagsViewRoutesList = tagsViewRoutes.value;
  initTagsView();
};
// pinia 中获取路由信息：如果是设置了固定的（isAffix），进行初始化显示
const initTagsView = async () => {
  if (tagsViewList.value.length > 0) {
    // tagsViewList already hydrated from sessionStorage by persistedstate plugin
    // Re-initialize cached views from hydrated list
    tagsViewList.value.forEach((v: RouteItem) => {
      if (v.meta?.isKeepAlive) storesKeepALiveNames.addCachedView(v);
    });
  } else {
    await state.tagsViewRoutesList.map((v: RouteItem) => {
      if (v.meta?.isAffix && !v.meta.isHide) {
        v.url = setTagsViewHighlight(v);
        tagsViewList.value.push({...v});
        storesKeepALiveNames.addCachedView(v);
      }
    });
    await addTagsView(route.path, <RouteToFrom>route);
  }
  // 初始化当前元素(li)的下标
  getTagsRefsIndex(state.routePath);
};
// 处理可开启多标签详情，单标签详情（动态路由（xxx/:id/:name"），普通路由处理）
const solveAddTagsView = async (path: string, to?: RouteToFrom) => {
  let isDynamicPath = to?.meta?.isDynamic ? to.meta.isDynamicPath : path;
  let current = tagsViewList.value.filter(
      (v: RouteItem) =>
          v.path === isDynamicPath &&
          isObjectValueEqual(
              to?.meta?.isDynamic ? (v.params ? v.params : null) : v.query ? v.query : null,
              to?.meta?.isDynamic ? (to?.params ? to?.params : null) : to?.query ? to?.query : null
          )
  );
  if (current.length <= 0) {
    // 防止：Avoid app logic that relies on enumerating keys on a component instance. The keys will be empty in production mode to avoid performance overhead.
    let findItem = state.tagsViewRoutesList.find((v: RouteItem) => v.path === isDynamicPath);
    if (!findItem) return false;
    if (findItem.meta.isAffix) return false;
    if (findItem.meta.isLink && !findItem.meta.isIframe) return false;
    to?.meta?.isDynamic ? (findItem.params = to.params) : (findItem.query = to?.query);
    findItem.url = setTagsViewHighlight(findItem);
    tagsViewList.value.push({...findItem});
    await storesKeepALiveNames.addCachedView(findItem);
  }
};
// 处理单标签时，第二次的值未覆盖第一次的 tagsViewList 值
const singleAddTagsView = (path: string, to?: RouteToFrom) => {
  let isDynamicPath = to?.meta?.isDynamic ? to.meta.isDynamicPath : path;
  tagsViewList.value.forEach((v) => {
    if (
        v.path === isDynamicPath &&
        !isObjectValueEqual(
            to?.meta?.isDynamic ? (v.params ? v.params : null) : v.query ? v.query : null,
            to?.meta?.isDynamic ? (to?.params ? to?.params : null) : to?.query ? to?.query : null
        )
    ) {
      to?.meta?.isDynamic ? (v.params = to.params) : (v.query = to?.query);
      v.url = setTagsViewHighlight(v);
    }
  });
};
// 1、添加 tagsView：未设置隐藏（isHide）也添加到在 tagsView 中（可开启多标签详情，单标签详情）
const addTagsView = (path: string, to?: RouteToFrom) => {
  // 防止拿取不到路由信息
  nextTick(async () => {
    // 修复：https://gitee.com/lyt-top/vue-next-admin/issues/I3YX6G
    let item: RouteItem;
    if (to?.meta?.isDynamic) {
      // 动态路由（xxx/:id/:name"）：参数不同，开启多个 tagsview
      await singleAddTagsView(path, to);
      if (tagsViewList.value.some((v: RouteItem) => v.path === to?.meta?.isDynamicPath)) {
        return false;
      }
      item = state.tagsViewRoutesList.find((v: RouteItem) => v.path === to?.meta?.isDynamicPath);
    } else {
      // 普通路由：参数不同，开启多个 tagsview
      await singleAddTagsView(path, to);
      if (tagsViewList.value.some((v: RouteItem) => v.path === path)) {
        return false;
      }
      item = state.tagsViewRoutesList.find((v: RouteItem) => v.path === path);
    }
    if (!item) return false;
    if (item?.meta.isLink && !item.meta.isIframe) return false;
    if (to?.meta?.isDynamic) item.params = to?.params ? to?.params : route.params;
    else item.query = to?.query ? to?.query : route.query;
    item.url = setTagsViewHighlight(item);
    await storesKeepALiveNames.addCachedView(item);
    await tagsViewList.value.push({...item});
  });
};

// 右键菜单操作集（刷新 / 关闭 / 关闭其它 / 关闭全部 / 全屏）从 useTagsContextMenu 注入
const {
  refreshCurrentTagsView,
  closeCurrentTagsView,
  onCurrentContextmenuClick,
} = useTagsContextMenu({
  tagsViewList,
  getRoutePath: () => state.routePath,
  setTagsViewHighlight,
  addTagsView,
});

// 右键点击时：传 x,y 坐标值到子组件中（props）
const onContextmenu = (v: RouteItem, e: MouseEvent) => {
  contextmenuRef.value.openContextmenu(v, e);
};
// 鼠标按下时，判断是鼠标中键就关闭当前 tasgview
const onMousedownMenu = (v: RouteItem, e: MouseEvent) => {
  if (!v.meta?.isAffix && e.button === 1) {
    const item = Object.assign({}, {contextMenuClickId: 1, ...v});
    onCurrentContextmenuClick(item);
  }
};
// 当前的 tagsView 项点击时
const onTagsClick = (v: RouteItem, k: number) => {
  state.tagsRefsIndex = k;
  router.push(v);
};
// 页面加载前
onBeforeMount(() => {
  // 监听非本页面调用 0 刷新当前，1 关闭当前，2 关闭其它，3 关闭全部 4 当前页全屏
  mittBus.on('onCurrentContextmenuClick', (data: RouteItem) => {
    // 通过方法点击关闭 tagsView
    data.isFnClick = true;
    onCurrentContextmenuClick(data);
  });
});
// 页面卸载时
onUnmounted(() => {
  // 取消非本页面调用监听
  mittBus.off('onCurrentContextmenuClick', () => {
  });
});
// 页面加载时
onMounted(() => {
  // 初始化 pinia 中的 tagsViewRoutes 列表
  getTagsViewRoutes();
  initSortable();
});
// 路由更新时（组件内生命钩子）
onBeforeRouteUpdate(async (to) => {
  state.routeActive = setTagsViewHighlight(to);
  state.routePath = to.meta.isDynamic ? to.meta.isDynamicPath : to.path;
  await addTagsView(to.path, <RouteToFrom>to);
  getTagsRefsIndex(state.routePath);
});
// 监听路由的变化，动态赋值给 tagsView
watch(
    () => tagsViewRoutes.value,
    (val) => {
      if (val.length === state.tagsViewRoutesList.length) return false;
      getTagsViewRoutes();
    },
    {
      deep: true,
    }
);
</script>

<style scoped lang="scss">
.layout-navbars-tagsview {
  background-color: var(--p-content-background);
  border-bottom: 1px solid var(--p-content-border-color);
  position: relative;
  z-index: $z-index-sticky;

  .tagsview-scrollbar {
    overflow-x: auto;
    overflow-y: hidden;
  }

  &-ul {
    list-style: none;
    margin: 0;
    padding: 0;
    min-height: 44px;
    display: flex;
    align-items: center;
    color: var(--p-text-color);
    font-size: 12px;
    white-space: nowrap;
    padding: 0 15px;
  }
}
</style>
