<template>
  <div class="sys-cache-container">
    <NoticeBar :text="t('system.cache.cacheNotice')" leftIcon="fa-solid fa-bell"
               background="var(--p-primary-100)" color="var(--p-primary-color)"/>
    <div class="flex gap-2" style="width: 100%">
      <div style="width: 33%; min-width: 250px">
        <Card class="mt8">
          <template #header>
            <div class="card-header">
              <span>{{ t('system.cache.cacheList') }}</span>
              <Button rounded text size="small" @click="onQuery" v-auth="'systemCache:keyList'">
                <i class="fa-solid fa-arrows-rotate"></i>
              </Button>
            </div>
          </template>
          <template #default>
            <div v-block-loading="state.loading">
              <Tree
                  :value="state.cacheData"
                  selectionMode="single"
                  v-model:selectionKeys="selectedNodeKey"
                  @node-select="onNodeSelect"
                  :expandedKeys="expandedKeys"
              />
            </div>
          </template>
        </Card>
      </div>
      <div style="flex: 1">
        <Card class="mt8">
          <template #header>
            <div class="card-header">
              <span>{{ `${t('system.cache.cacheData')}${state.cacheKey ? `【${state.cacheKey}】` : ''}` }}</span>
              <Button severity="danger" size="small" @click="delCache" v-auth="'systemCache:delete'">
                <i class="fa-solid fa-trash mr-1"></i>
                {{ t('system.cache.deleteCache') }}
              </Button>
            </div>
          </template>
          <template #default>
            <div v-block-loading="state.loading1">
              <vue-json-pretty :data="state.cacheValue" showLength showIcon showLineNumber showSelectController/>
            </div>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup name="systemCache">
import {faArrowsRotate, faTrash} from '../../../../utils/icons.ts';

import {onMounted, reactive, ref} from 'vue';
import {showConfirm, showSuccess, showWarning} from '../../../../utils/message.ts';
import NoticeBar from '/@/components/notice-bar/index.vue';
import VueJsonPretty from 'vue-json-pretty';
import 'vue-json-pretty/lib/styles.css';
import type {TreeNode} from 'primevue/treenode';

import {
  systemCacheKeyListGet,
  systemCacheDeletePost,
  systemCacheValueGet,
} from '../../../../api/systemCache';

const selectedNodeKey = ref<Record<string, boolean>>({});
const expandedKeys = ref<Record<string, boolean>>({});
const currentNode = ref<TreeNode & { data?: { cacheKey?: string } }>({} as any);
const state = reactive({
  loading: false,
  loading1: false,
  cacheData: [] as TreeNode[],
  cacheValue: undefined as any,
  cacheKey: undefined as string | undefined,
});

onMounted(async () => {
  onQuery();
});

const buildCacheTree = (keyList: string[]) => {
  const rootNodes: Array<TreeNode & { data?: { cacheKey?: string } }> = [];
  const nodeIndex = new Map<string, TreeNode & { data?: { cacheKey?: string } }>();
  const allExpanded: Record<string, boolean> = {};

  const ensureNode = (parentNodes: Array<TreeNode & { data?: { cacheKey?: string } }>, pathKey: string, label: string) => {
    let node = nodeIndex.get(pathKey);
    if (!node) {
      node = {
        key: pathKey,
        label,
        children: [],
      } as TreeNode & { data?: { cacheKey?: string } };
      nodeIndex.set(pathKey, node);
      parentNodes.push(node);
    } else if (!parentNodes.includes(node)) {
      parentNodes.push(node);
    }
    if (!node.children) {
      node.children = [];
    }
    return node;
  };

  keyList.forEach((cacheKey) => {
    if (!cacheKey) return;

    const segments = cacheKey.split(':');
    let parentNodes = rootNodes;
    let pathKey = '';

    segments.forEach((segment, index) => {
      pathKey = index === 0 ? segment : `${pathKey}:${segment}`;
      const node = ensureNode(parentNodes, pathKey, segment);

      if (index < segments.length - 1) {
        allExpanded[pathKey] = true;
      } else {
        node.data = {...(node.data ?? {}), cacheKey};
      }

      parentNodes = node.children as Array<TreeNode & { data?: { cacheKey?: string } }>;
    });
  });

  state.cacheData = rootNodes;
  expandedKeys.value = allExpanded;
};

const onQuery = async () => {
  state.cacheData = [];
  state.cacheValue = undefined;
  state.cacheKey = undefined;
  selectedNodeKey.value = {};

  state.loading = true;
  try {
    const res = await systemCacheKeyListGet();
    const keyList = (res.data.data ?? []) as string[];
    buildCacheTree(keyList);
  } finally {
    state.loading = false;
  }
};

const delCache = async () => {
  const cacheKey = currentNode.value?.data?.cacheKey;
  if (!cacheKey) {
    showWarning(t('system.cache.forbidDeleteTopCache'));
    return;
  }
  const confirmed = await showConfirm(`${t('system.cache.deleteCacheConfirm')}：【${cacheKey}】?`, t('common.message.tip'), {type: 'danger'});
  if (!confirmed) return;
  await systemCacheDeletePost({key: cacheKey});
  onQuery();
  state.cacheValue = undefined;
  state.cacheKey = undefined;
  showSuccess(t('common.message.deleteSuccess'));
};

const onNodeSelect = async (node: TreeNode) => {
  const cacheKey = (node as TreeNode & { data?: { cacheKey?: string } })?.data?.cacheKey;

  currentNode.value = node;
  if (!cacheKey) {
    state.cacheValue = undefined;
    state.cacheKey = undefined;
    return;
  }

  state.loading1 = true;
  try {
    const res = await systemCacheValueGet(cacheKey);
    const result = res.data.data;
    if (typeof result == 'string') {
      try {
        const obj = JSON.parse(result);
        if (typeof obj == 'object') {
          state.cacheValue = obj;
        } else {
          state.cacheValue = result;
        }
      } catch (e) {
        state.cacheValue = result;
      }
    } else {
      state.cacheValue = result;
    }
    state.cacheKey = cacheKey;
  } finally {
    state.loading1 = false;
  }
};
</script>

<style lang="scss" scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
