<template>
  <div class="layout-parent">
    <router-view v-slot="{ Component }">
      <transition :name="setTransitionName" mode="out-in">
        <keep-alive :include="getKeepAliveNames">
          <component :is="Component" :key="state.refreshRouterViewKey" class="w100" v-show="!isIframePage"/>
        </keep-alive>
      </transition>
    </router-view>
    <transition :name="setTransitionName" mode="out-in">
      <Iframes class="w100" v-show="isIframePage" :refreshKey="state.iframeRefreshKey" :name="setTransitionName"
               :list="state.iframeList"/>
    </transition>
  </div>
</template>

<script setup lang="ts" name="layoutParentView">
import {defineAsyncComponent, computed, reactive, onBeforeMount, onUnmounted, nextTick, watch, onMounted} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {storeToRefs} from 'pinia';
import {useKeepAliveNamesStore} from '../../stores/keepAliveNames';
import {useThemeConfigStore} from '../../stores/themeConfig';
import {useTagsViewRoutesStore} from '../../stores/tagsViewRoutes';
import mittBus from '../../utils/mitt';

// 引入组件
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const Iframes = defineAsyncComponent(() => import('/@/layout/routerView/IframesView.vue'));

// 定义变量内容
const route = useRoute();
const router = useRouter();
const storesKeepAliveNames = useKeepAliveNamesStore();
const storesThemeConfig = useThemeConfigStore();
const storesTagsViewRoutes = useTagsViewRoutesStore();
const {cachedViews} = storeToRefs(storesKeepAliveNames);
const {themeConfig} = storeToRefs(storesThemeConfig);
const {tagsViewList} = storeToRefs(storesTagsViewRoutes);
const state = reactive<ParentViewState>({
  refreshRouterViewKey: '',
  iframeRefreshKey: '',
  keepAliveNameList: [],
  iframeList: [],
});

// 设置主界面切换动画
const setTransitionName = computed(() => {
  return 'fadeLeft';
});
// 获取组件缓存列表(name值)
const getKeepAliveNames = computed(() => {
  return cachedViews.value;
});
// 设置 iframe 显示/隐藏
const isIframePage = computed(() => {
  return route.meta.isIframe;
});
// 获取 iframe 组件列表(未进行渲染)
const getIframeListRoutes = async () => {
  router.getRoutes().forEach((v) => {
    if (v.meta.isIframe) {
      v.meta.isIframeOpen = false;
      v.meta.loading = true;
      state.iframeList.push({...v});
    }
  });
};
onBeforeMount(() => {
  mittBus.on('onTagsViewRefreshRouterView', (fullPath: string) => {
    state.refreshRouterViewKey = '';
    state.iframeRefreshKey = '';
    nextTick(() => {
      state.refreshRouterViewKey = fullPath;
      state.iframeRefreshKey = fullPath;
    });
  });
});
// 页面加载时
onMounted(() => {
  getIframeListRoutes();
  // https://gitee.com/lyt-top/vue-next-admin/issues/I58U75
  // https://gitee.com/lyt-top/vue-next-admin/issues/I59RXK
  // https://gitee.com/lyt-top/vue-next-admin/pulls/40
  nextTick(() => {
    setTimeout(() => {
      let tagsViewArr: RouteItem[] = tagsViewList.value || [];
      cachedViews.value = tagsViewArr.filter((item) => item.meta?.isKeepAlive).map((item) => item.name as string);
    }, 0);
  });
});
// 页面卸载时
onUnmounted(() => {
  mittBus.off('onTagsViewRefreshRouterView', () => {
  });
});
// 监听路由变化，防止 tagsView 多标签时，切换动画消失
// https://toscode.gitee.com/lyt-top/vue-next-admin/pulls/38/files
watch(
    () => route.fullPath,
    () => {
      state.refreshRouterViewKey = decodeURI(route.fullPath);
    },
    {
      immediate: true,
    }
);
</script>
