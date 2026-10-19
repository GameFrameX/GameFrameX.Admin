// tagsView 右键菜单操作集：刷新 / 关闭当前 / 关闭其它 / 关闭全部 / 全屏，以及菜单用到的工具方法。
// 从 src/layout/navigationBars/tagsView/tagsView.vue 原样迁入，逻辑零改动。
// 与容器（tagsView.vue）存在双向调用，故通过 deps 注入：setTagsViewHighlight / addTagsView / tagsViewList / getRoutePath。
import {nextTick, type Ref} from 'vue';
import {useRouter, useRoute} from 'vue-router';
import {showWarning} from '/@/utils/message';
import {useTagsViewRoutesStore} from '/@/stores/tagsViewRoutes';
import {useKeepAliveNamesStore} from '/@/stores/keepAliveNames';
import mittBus from '/@/utils/mitt';
import {t} from '/@/i18n/index';

export interface UseTagsContextMenuDeps {
  tagsViewList: Ref<RouteItem[]>;
  getRoutePath: () => string;
  setTagsViewHighlight: (v: RouteToFrom) => string;
  addTagsView: (path: string, to?: RouteToFrom) => void;
}

export function useTagsContextMenu(deps: UseTagsContextMenuDeps) {
  const router = useRouter();
  const route = useRoute();
  const stores = useTagsViewRoutesStore();
  const storesKeepALiveNames = useKeepAliveNamesStore();
  const {tagsViewList, getRoutePath, setTagsViewHighlight, addTagsView} = deps;

  // 处理 url，地址栏链接有参数时，tagsview 右键菜单刷新功能失效问题，感谢 @ZzZz-RIPPER、@dejavuuuuu
  // https://gitee.com/lyt-top/vue-next-admin/issues/I5K3YO
  // https://gitee.com/lyt-top/vue-next-admin/issues/I61VS9
  const transUrlParams = (v: RouteItem) => {
    let params = v.query && Object.keys(v.query).length > 0 ? v.query : v.params;
    if (!params) return '';
    let path = '';
    for (let [key, value] of Object.entries(params)) {
      if (v.meta?.isDynamic) path += `/${value}`;
      else path += `&${key}=${value}`;
    }
    // 判断是否是动态路由（xxx/:id/:name"）isDynamic
    if (v.meta?.isDynamic) {
      /**
       *
       * isFnClick 用于判断是通过方法调用，还是直接右键菜单点击（此处只针对动态路由）
       * 原因：
       * 1、右键菜单点击时，路由的 path 还是原始定义的路由格式，如：/params/dynamic/details/:t/:id/:tagsViewName
       * 2、通过事件调用时，路由的 path 不是原始定义的路由格式，如：/params/dynamic/details/vue-next-admin/111/我是动态路由测试tagsViewName(非国际化)
       *
       * 所以右侧菜单点击时，需要处理路径拼接 v.path.split(':')[0]，得到路径 + 参数的完整路径
       */
      return v.isFnClick ? decodeURI(v.path) : `${v.path.split(':')[0]}${path.replace(/^\//, '')}`;
    } else {
      return `${v.path}${path.replace(/^&/, '?')}`;
    }
  };
  // 2、刷新当前 tagsView：
  const refreshCurrentTagsView = async (fullPath: string) => {
    const decodeURIPath = decodeURI(fullPath);
    let item: RouteToFrom = {};
    tagsViewList.value.forEach((v: RouteItem) => {
      v.transUrl = transUrlParams(v);
      if (v.transUrl) {
        if (v.transUrl === transUrlParams(v)) item = v;
      } else {
        if (v.path === decodeURIPath) item = v;
      }
    });
    if (!item) return false;
    await storesKeepALiveNames.delCachedView(item);
    mittBus.emit('onTagsViewRefreshRouterView', fullPath);
    nextTick(() => {
      if (item.meta?.isKeepAlive) storesKeepALiveNames.addCachedView(item);
    });
  };
  // 3、关闭当前 tagsView：如果是设置了固定的（isAffix），不可以关闭
  const closeCurrentTagsView = (path: string) => {
    tagsViewList.value.map((v: RouteItem, k: number, arr: RouteItems) => {
      if (!v.meta?.isAffix) {
        if (v.path === path) {
          storesKeepALiveNames.delCachedView(v);
          tagsViewList.value.splice(k, 1);
          setTimeout(() => {
            if (tagsViewList.value.length === k && getRoutePath() === path) {
              // 最后一个且高亮时
              if (arr[arr.length - 1].meta.isDynamic) {
                // 动态路由（xxx/:id/:name"）
                if (k !== arr.length) router.push({name: arr[k].name, params: arr[k].params});
                else router.push({name: arr[arr.length - 1].name, params: arr[arr.length - 1].params});
              } else {
                // 普通路由
                if (k !== arr.length) router.push({path: arr[k].path, query: arr[k].query});
                else router.push({path: arr[arr.length - 1].path, query: arr[arr.length - 1].query});
              }
            } else {
              // 非最后一个且高亮时，跳转到下一个
              if (tagsViewList.value.length !== k && getRoutePath() === path) {
                if (arr[k].meta.isDynamic) {
                  // 动态路由（xxx/:id/:name"）
                  router.push({name: arr[k].name, params: arr[k].params});
                } else {
                  // 普通路由
                  router.push({path: arr[k].path, query: arr[k].query});
                }
              }
            }
          }, 0);
        }
      }
    });
  };
  // 4、关闭其它 tagsView：如果是设置了固定的（isAffix），不进行关闭
  const closeOtherTagsView = (path: string) => {
    const currentList = [...tagsViewList.value];
    tagsViewList.value = [];
    currentList.map((v: RouteItem) => {
      if (v.meta?.isAffix && !v.meta.isHide) {
        v.url = setTagsViewHighlight(v);
        storesKeepALiveNames.delOthersCachedViews(v);
        tagsViewList.value.push({...v});
      }
    });
    addTagsView(path, <RouteToFrom>route);
  };
  // 5、关闭全部 tagsView：如果是设置了固定的（isAffix），不进行关闭
  const closeAllTagsView = () => {
    const currentList = [...tagsViewList.value];
    storesKeepALiveNames.delAllCachedViews();
    tagsViewList.value = [];
    currentList.map((v: RouteItem) => {
      if (v.meta?.isAffix && !v.meta.isHide) {
        v.url = setTagsViewHighlight(v);
        tagsViewList.value.push({...v});
        router.push({path: tagsViewList.value[tagsViewList.value.length - 1].path});
      }
    });
  };
  // 6、开启当前页面全屏
  const openCurrenFullscreen = async (path: string) => {
    const item = tagsViewList.value.find((v: RouteItem) => v.path === path);
    if (!item) return;
    if (item.meta?.isDynamic) await router.push({name: item.name!, params: item.params});
    else await router.push({name: item.name!, query: item.query});
    stores.setCurrenFullscreen(true);
  };
  // 当前项右键菜单点击，拿 `当前点击的路由路径` 对比 `tagsView 路由数组`，取当前点击项的详细路由信息
  // 防止 tagsView 非当前页演示时，操作异常
  // https://gitee.com/lyt-top/vue-next-admin/issues/I61VS9
  const getCurrentRouteItem = (item: RouteItem): any => {
    let resItem: RouteToFrom = {};
    tagsViewList.value.forEach((v: RouteItem) => {
      v.transUrl = transUrlParams(v);
      if (v.transUrl) {
        // 动态路由、普通路由带参数
        if (v.transUrl === transUrlParams(v) && v.transUrl === item.commonUrl) resItem = v;
      } else {
        // 路由不带参数
        if (v.path === decodeURI(item.path)) resItem = v;
      }
    });
    if (!resItem) return null;
    else return resItem;
  };
  // 当前项右键菜单点击
  const onCurrentContextmenuClick = async (item: RouteItem) => {
    item.commonUrl = transUrlParams(item);
    if (!getCurrentRouteItem(item)) return showWarning(t('message.layout.tagsView.invalidPathWarning'));
    const {path, name, params, query, meta, url} = getCurrentRouteItem(item);
    switch (item.contextMenuClickId) {
      case 0:
        // 刷新当前
        if (meta.isDynamic) await router.push({name, params});
        else await router.push({path, query});
        refreshCurrentTagsView(route.fullPath);
        break;
      case 1:
        // 关闭当前
        closeCurrentTagsView(path);
        break;
      case 2:
        // 关闭其它
        if (meta.isDynamic) await router.push({name, params});
        else await router.push({path, query});
        closeOtherTagsView(path);
        break;
      case 3:
        // 关闭全部
        closeAllTagsView();
        break;
      case 4:
        // 开启当前页面全屏
        openCurrenFullscreen(path);
        break;
    }
  };

  return {
    transUrlParams,
    refreshCurrentTagsView,
    closeCurrentTagsView,
    closeOtherTagsView,
    closeAllTagsView,
    openCurrenFullscreen,
    onCurrentContextmenuClick,
  };
}
