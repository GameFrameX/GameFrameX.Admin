<template>
  <div :class="{ 'menu-collapsed': state.isCollapse }">
    <PanelMenu
        v-model:expandedKeys="expandedKeys"
        :model="menuItems"
        multiple
    >
      <template #item="{ item, active, hasSubmenu }">
        <router-link v-if="item.route" v-slot="{ href, navigate, isExactActive }" :to="resolveMenuRoute(item.route)" custom>
          <a v-ripple class="flex align-items-center cursor-pointer menu-nav-item" :class="{ 'menu-item-active': isExactActive }" :href="href" @click="navigate">
            <SvgIcon v-if="item.icon" :name="item.icon" class="mr-2"/>
            <span class="menu-label">{{ item.label }}</span>
          </a>
        </router-link>
        <a v-else-if="item.url" v-ripple class="flex align-items-center cursor-pointer menu-nav-item" :href="item.url" :target="item.target">
          <SvgIcon v-if="item.icon" :name="item.icon" class="mr-2"/>
          <span class="menu-label">{{ item.label }}</span>
        </a>
        <span v-else class="flex align-items-center menu-nav-item"
              role="button"
              tabindex="0"
              :aria-label="item.label"
              :aria-haspopup="hasSubmenu ? 'menu' : undefined"
              :aria-expanded="hasSubmenu ? state.isCollapse && flyoutParentKey === item.key : undefined"
              @click="onParentItemActivate($event, item)"
              @focus="onParentItemHover($event, item)"
              @blur="onParentItemLeave"
              @keydown="onParentItemKeydown($event, item)"
              @mouseenter="onParentItemHover($event, item)"
              @mouseleave="onParentItemLeave">
          <SvgIcon v-if="item.icon" :name="item.icon" class="mr-2"/>
          <span class="menu-label">{{ item.label }}</span>
          <SvgIcon
              v-if="hasSubmenu"
              :name="active ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'"
              :size="12"
              class="menu-toggle-icon"
          />
        </span>
      </template>
    </PanelMenu>
    <Popover ref="flyoutRef" class="menu-flyout" @mouseenter="onFlyoutEnter" @mouseleave="onFlyoutLeave">
      <div class="flyout-menu">
        <template v-for="child in flyoutChildren" :key="child.key">
          <router-link v-if="child.route" :to="resolveMenuRoute(child.route)" custom v-slot="{ href, navigate, isExactActive }">
            <a class="flyout-item" :class="{ 'flyout-item-active': isExactActive }" :href="href" @click="navigate(); closeFlyout()">
              <SvgIcon v-if="child.icon" :name="child.icon" class="mr-2"/>
              <span>{{ child.label }}</span>
            </a>
          </router-link>
          <a v-else-if="child.url" class="flyout-item" :href="child.url" :target="child.target" @click="closeFlyout()">
            <SvgIcon v-if="child.icon" :name="child.icon" class="mr-2"/>
            <span>{{ child.label }}</span>
          </a>
          <span v-else class="flyout-item flyout-parent-label">
            <SvgIcon v-if="child.icon" :name="child.icon" class="mr-2"/>
            <span>{{ child.label }}</span>
          </span>
        </template>
      </div>
    </Popover>
  </div>
</template>

<script setup lang="ts" name="navigationMenuVertical">
import { reactive, computed, ref, onMounted, onUnmounted, watch } from 'vue';
import { useRoute, useRouter, onBeforeRouteUpdate, RouteRecordRaw } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useThemeConfigStore } from '../../stores/themeConfig';
import PanelMenu from 'primevue/panelmenu';
import Popover from 'primevue/popover';
import SvgIcon from '/@/components/svg-icon/index.vue';
import { convertRoutesToMenuItems, computeExpandedKeys } from './menuConverter';
import type { NavMenuItem } from './menuConverter';
import { useCurrentApp } from '../../composables/useCurrentApp';
import { normalizeAppId, shouldCarryAppId } from '../../utils/appWorkspace';

const props = defineProps({
  menuList: {
    type: Array<RouteRecordRaw>,
    default: () => [],
  },
});

const storesThemeConfig = useThemeConfigStore();
const { themeConfig } = storeToRefs(storesThemeConfig);
const route = useRoute();
const router = useRouter();
const { currentAppId } = useCurrentApp();

const state = reactive({
  isCollapse: false,
});

const expandedKeys = ref<Record<string, boolean>>({});

const menuItems = computed(() => {
  return convertRoutesToMenuItems(<RouteItems>props.menuList, t);
});

const resolveMenuRoute = (path: string) => {
  const appId = normalizeAppId(route.query.appId) ?? normalizeAppId(currentAppId.value);
  if (appId === undefined || !shouldCarryAppId(router.resolve(path))) {
    return path;
  }
  return { path, query: { appId: appId } };
};

watch(
    () => themeConfig.value.isCollapse,
    (isCollapse) => {
      document.body.clientWidth < 1024 // 移动端阈值，与 theme/breakpoints.scss --bp-lg 一致
          ? (state.isCollapse = false)
          : (state.isCollapse = isCollapse);
    },
    { immediate: true },
);

// Flyout for collapsed menu
const flyoutRef = ref();
const flyoutChildren = ref<NavMenuItem[]>([]);
const flyoutParentKey = ref<string>();
let hideTimer: ReturnType<typeof setTimeout> | null = null;

const onParentItemHover = (event: Event, item: NavMenuItem) => {
  if (!state.isCollapse || !item.items?.length) return;
  if (hideTimer) {
    clearTimeout(hideTimer);
    hideTimer = null;
  }
  flyoutChildren.value = item.items as NavMenuItem[];
  flyoutParentKey.value = item.key;
  flyoutRef.value?.show(event);
};

const onParentItemActivate = (event: Event, item: NavMenuItem) => {
  if (!state.isCollapse || !item.items?.length) return;
  event.preventDefault();
  onParentItemHover(event, item);
};

const onParentItemKeydown = (event: KeyboardEvent, item: NavMenuItem) => {
  if (!state.isCollapse || !item.items?.length) return;
  if (event.key === 'Enter' || event.key === ' ') {
    onParentItemActivate(event, item);
  }
  if (event.key === 'Escape') {
    closeFlyout();
  }
};

const onParentItemLeave = () => {
  scheduleHideFlyout();
};

const onFlyoutEnter = () => {
  if (hideTimer) {
    clearTimeout(hideTimer);
    hideTimer = null;
  }
};

const onFlyoutLeave = () => {
  scheduleHideFlyout();
};

const scheduleHideFlyout = () => {
  hideTimer = setTimeout(() => {
    flyoutRef.value?.hide();
    flyoutChildren.value = [];
    flyoutParentKey.value = undefined;
  }, 200);
};

const closeFlyout = () => {
  if (hideTimer) {
    clearTimeout(hideTimer);
    hideTimer = null;
  }
  flyoutRef.value?.hide();
  flyoutChildren.value = [];
  flyoutParentKey.value = undefined;
};

onUnmounted(() => {
  if (hideTimer) {
    clearTimeout(hideTimer);
  }
});

const getTargetPath = (r: { path: string; meta?: any }) => {
  const path = r.meta?.isDynamic ? r.meta.isDynamicPath : r.path;
  const pathSplit = path.split('/');
  return pathSplit.length >= 4 && r.meta?.isHide
      ? pathSplit.splice(0, 3).join('/')
      : path;
};

const updateExpandedKeys = (r: { path: string; meta?: any } = route) => {
  expandedKeys.value = computeExpandedKeys(menuItems.value, getTargetPath(r));
};

onMounted(() => {
  updateExpandedKeys();
});

onBeforeRouteUpdate((to) => {
  updateExpandedKeys(to);
  const clientWidth = document.body.clientWidth;
  if (clientWidth < 1024) themeConfig.value.isCollapse = false; // 移动端阈值，与 theme/breakpoints.scss --bp-lg 一致
});
</script>

<style scoped lang="scss">
// 图纸目录条目：安静行。hover 淡墨洗；激活 = 墨色文字 + 左缘 3px 装订墨条（当前图纸装订在读位）
.menu-nav-item {
  position: relative;
  padding: 0 12px;
  min-height: 38px;
  width: 100%;
  min-width: 0;
  text-decoration: none;
  color: var(--p-text-color);
  transition: background-color 0.15s ease, color 0.15s ease;

  &:hover {
    background: color-mix(in srgb, var(--p-primary-100) 55%, transparent);
    color: var(--p-primary-color);
  }

  &:focus-visible {
    outline: 2px solid var(--p-primary-color);
    outline-offset: -2px;
  }
}

.menu-label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.menu-item-active {
  background: color-mix(in srgb, var(--p-primary-100) 55%, transparent);
  color: var(--p-primary-color);
  font-weight: 600;

  // 装订墨条：左缘 3px 墨线
  &::before {
    content: '';
    position: absolute;
    left: 0;
    top: 7px;
    bottom: 7px;
    width: 3px;
    background: var(--p-primary-color);
  }
}

.menu-toggle-icon {
  margin-left: auto;
  font-size: 12px;
  opacity: 0.6;
}

// PanelMenu chrome 安静化：纸面上不留组件默认底色/分隔线
:deep(.p-panelmenu) {
  .p-panelmenu-header {
    .p-panelmenu-header-content {
      background: transparent;
      border: 0;
      border-radius: 0;
    }
  }

  .p-panelmenu-content {
    background: transparent;
    border: 0;
    padding: 2px 0 6px;
  }
}

.menu-collapsed {
  width: 64px;

  .menu-label {
    display: none;
  }

  :deep(.p-panelmenu-panel) {
    .p-panelmenu-header {
      .p-panelmenu-header-content {
        justify-content: center;
        padding: 0;

        .p-panelmenu-header-toggle-icon,
        .p-menuitem-icon,
        .menu-label {
          display: none;
        }

        .menu-nav-item {
          justify-content: center;
          padding: 0;
          min-height: 44px;
          border-radius: 0;

          // 折叠 rail：装订墨条改为贴左缘全线
          &.menu-item-active::before {
            top: 10px;
            bottom: 10px;
          }
        }
      }
    }

    .p-panelmenu-content {
      display: none;
    }
  }
}
</style>

<style lang="scss">
// flyout 纸片：纸面 + 发丝线 + 单层纸影（纸叠纸，纸面剧作卡世界）
.menu-flyout {
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
  box-shadow: 0 8px 22px color-mix(in srgb, var(--p-surface-900) 10%, transparent);

  :deep(.p-popover-content) {
    padding: 0;
  }
}

.flyout-menu {
  min-width: 160px;
  padding: 0.25rem 0;
}

.flyout-item {
  display: flex;
  align-items: center;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
  text-decoration: none;
  color: var(--p-text-color);
  white-space: nowrap;

  &:hover {
    background: color-mix(in srgb, var(--p-primary-100) 55%, transparent);
    color: var(--p-primary-color);
  }
}

.flyout-item-active {
  color: var(--p-primary-color);
  font-weight: 600;
}

.flyout-parent-label {
  cursor: default;
  font-weight: 600;
  color: var(--p-text-muted-color);
}
</style>
