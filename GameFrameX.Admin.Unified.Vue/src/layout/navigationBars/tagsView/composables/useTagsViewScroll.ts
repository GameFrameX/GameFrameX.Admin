// tagsView 滚动定位与拖拽（sortable）支持：横向滚轮、当前标签滚动到位、拖拽排序、resize 重算。
// 从 src/layout/navigationBars/tagsView/tagsView.vue 原样迁入，逻辑零改动。
// 仅依赖 tagsViewList 与 state（tagsRefsIndex / sortable），不涉及右键菜单与路由状态调度。
import {ref, nextTick, onBeforeMount, onUnmounted, onBeforeUpdate, type Ref} from 'vue';
import Sortable from 'sortablejs';
import other from '/@/utils/other';
import mittBus from '/@/utils/mitt';

export function useTagsViewScroll(deps: { tagsViewList: Ref<RouteItem[]>; state: TagsViewState }) {
  const tagsRefs = ref<RefType>([]);
  const scrollbarRef = ref<HTMLElement>();
  const tagsUlRef = ref();
  const {tagsViewList, state} = deps;

  // 鼠标滚轮滚动
  const onHandleScroll = (e: WheelEvent) => {
    if (scrollbarRef.value) {
      const delta = Math.abs(e.deltaX) > Math.abs(e.deltaY) ? e.deltaX : e.deltaY;
      scrollbarRef.value.scrollLeft += delta;
    }
  };
  // tagsView 横向滚动
  const tagsViewmoveToCurrentTag = () => {
    nextTick(() => {
      if (tagsRefs.value.length <= 0) return false;
      if (!scrollbarRef.value) return false;
      // 当前 li 元素
      let liDom = tagsRefs.value[state.tagsRefsIndex];
      // 当前 li 元素下标
      let liIndex = state.tagsRefsIndex;
      // 当前 ul 下 li 元素总长度
      let liLength = tagsRefs.value.length;
      // 最前 li
      let liFirst = tagsRefs.value[0];
      // 最后 li
      let liLast = tagsRefs.value[tagsRefs.value.length - 1];
      // 当前滚动条元素
      let scrollRefs = scrollbarRef.value;
      // 当前滚动条滚动宽度
      let scrollS = scrollRefs.scrollWidth;
      // 当前滚动条偏移宽度
      let offsetW = scrollRefs.offsetWidth;
      // 当前滚动条偏移距离
      let scrollL = scrollRefs.scrollLeft;
      // 上一个 tags li dom
      let liPrevTag = tagsRefs.value[state.tagsRefsIndex - 1];
      // 下一个 tags li dom
      let liNextTag = tagsRefs.value[state.tagsRefsIndex + 1];
      // 上一个 tags li dom 的偏移距离
      let beforePrevL = 0;
      // 下一个 tags li dom 的偏移距离
      let afterNextL = 0;
      if (liDom === liFirst) {
        // 头部
        scrollRefs.scrollLeft = 0;
      } else if (liDom === liLast) {
        // 尾部
        scrollRefs.scrollLeft = scrollS - offsetW;
      } else {
        // 非头/尾部
        if (liIndex === 0) beforePrevL = liFirst.offsetLeft - 5;
        else beforePrevL = liPrevTag?.offsetLeft - 5;
        if (liIndex === liLength) afterNextL = liLast.offsetLeft + liLast.offsetWidth + 5;
        else afterNextL = liNextTag.offsetLeft + liNextTag.offsetWidth + 5;
        if (afterNextL > scrollL + offsetW) {
          scrollRefs.scrollLeft = afterNextL - offsetW;
        } else if (beforePrevL < scrollL) {
          scrollRefs.scrollLeft = beforePrevL;
        }
      }
    });
  };
  // 获取 tagsView 的下标：用于处理 tagsView 点击时的横向滚动
  const getTagsRefsIndex = (path: string | unknown) => {
    nextTick(async () => {
      // await 使用该写法，防止拿取不到 tagsViewList 列表数据不完整
      let list = await tagsViewList.value;
      state.tagsRefsIndex = list.findIndex((v: RouteItem) => {
        return v.path === path;
      });
      // 添加初始化横向滚动条移动到对应位置
      tagsViewmoveToCurrentTag();
    });
  };
  // 设置 tagsView 可以进行拖拽
  const initSortable = async () => {
    const el = <HTMLElement>document.querySelector('.layout-navbars-tagsview-ul');
    if (!el) return false;
    state.sortable.el && state.sortable.destroy();
    state.sortable = Sortable.create(el, {
      animation: 300,
      dataIdAttr: 'data-url',
      disabled: false,
      onEnd: () => {
        const sortEndList: RouteItem[] = [];
        state.sortable.toArray().map((val: string) => {
          tagsViewList.value.map((v: RouteItem) => {
            if (v.url === val) sortEndList.push({...v});
          });
        });
        tagsViewList.value = sortEndList;
      },
    });
  };
  // 拖动问题，https://gitee.com/lyt-top/vue-next-admin/issues/I3ZRRI
  const onSortableResize = async () => {
    await initSortable();
    if (other.isMobile()) state.sortable.el && state.sortable.destroy();
  };
  // 页面加载前
  onBeforeMount(() => {
    // 初始化，防止手机端直接访问时还可以拖拽
    onSortableResize();
    // 拖动问题，https://gitee.com/lyt-top/vue-next-admin/issues/I3ZRRI
    window.addEventListener('resize', onSortableResize);
    // 监听布局配置界面开启/关闭拖拽
    mittBus.on('openOrCloseSortable', () => {
      initSortable();
    });
  });
  // 页面卸载时
  onUnmounted(() => {
    // 取消监听布局配置界面开启/关闭拖拽
    mittBus.off('openOrCloseSortable', () => {
    });
    // 取消窗口 resize 监听
    window.removeEventListener('resize', onSortableResize);
  });
  // 页面更新时
  onBeforeUpdate(() => {
    tagsRefs.value = [];
  });

  return {
    tagsRefs,
    scrollbarRef,
    tagsUlRef,
    onHandleScroll,
    getTagsRefsIndex,
    initSortable,
  };
}
