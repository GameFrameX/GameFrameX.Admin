<template>
  <div class="online-cursor-list">
    <div v-if="state.snapshotTime != null" class="online-cursor-list-snapshot flex align-items-center mb-2">
      <i class="fa-solid fa-clock text-color-secondary text-sm mr-1"></i>
      <small class="text-color-secondary">{{ t('message.onlineCenter.cursorList.snapshotTime') }}：{{ formatSnapshot(state.snapshotTime) }}</small>
    </div>
    <DataTable :value="state.items" v-block-loading="state.loading" showGridlines stripedRows scrollable>
      <Column v-for="col in columns" :key="col.field" :field="col.field" :header="col.header"
              :headerStyle="colStyle(col)" :bodyStyle="colStyle(col)">
        <template #body="{ data, index }">
          <slot :name="`body-${col.field}`" :data="data" :index="index">
            <span v-if="col.sensitive" class="online-cursor-list-sensitive cursor-pointer select-none"
                  :title="t('message.onlineCenter.cursorList.sensitiveHint')"
                  @click="toggleReveal(col.field, index)">
              {{ isRevealed(col.field, index) ? cellText(data, col.field) : SENSITIVE_MASK }}
            </span>
            <template v-else>{{ cellText(data, col.field) }}</template>
          </slot>
        </template>
      </Column>
      <template #empty>
        <div class="text-center text-color-secondary p-4">
          {{ state.loaded ? t('message.onlineCenter.cursorList.empty') : t('message.onlineCenter.cursorList.notLoaded') }}
        </div>
      </template>
    </DataTable>
    <div v-if="state.hasMore" class="flex justify-content-center mt-3">
      <Button class="online-cursor-list-load-more" :label="t('message.onlineCenter.cursorList.loadMore')"
              icon="fa-solid fa-chevron-down" size="small" outlined :loading="state.loading" @click="loadMore" />
    </div>
  </div>
</template>

<script lang="ts" setup>
/**
 * Online 游标列表通用组件（C349 / GFX-756；C341/C342/C344 复用前置）。
 *
 * 与 crud-data-table（页码模型）互斥的独立组件：分页走 Cursor + HasMore 透传
 * （vault C2 S1.2），交互为「加载更多」；不做行去重/修补（重复即暴露服务端 bug）。
 * 状态机见同目录 cursorPager.ts（纯逻辑，node --test 单测覆盖）。
 */
import { reactive, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import { formatUnix } from '../../utils/formatTime.ts'
import {
  createCursorPagerState,
  beginLoad,
  applyFirstPage,
  appendPage,
  failLoad,
  buildRequestParams,
} from './cursorPager.ts'
import type { OnlineColumnConfig, OnlineCursorRequest } from './types'

/** 敏感列默认遮蔽文案（与后端脱敏显示字段口径一致）。 */
const SENSITIVE_MASK = '******'

const props = withDefaults(defineProps<{
  /** 查询工厂：组件透传 cursor/pageSize，调用方闭包拼业务过滤条件并解包信封。 */
  request: OnlineCursorRequest
  columns: OnlineColumnConfig[]
  pageSize?: number
}>(), {
  pageSize: 20,
})

const state = reactive(createCursorPagerState<Record<string, any>>())

// 已显明的敏感格集合（`${field}:${rowIndex}`；点击切换，重载即重置）
const revealed = ref(new Set<string>())

const colStyle = (col: OnlineColumnConfig) => {
  const style: Record<string, string> = {}
  if (col.width) {
    style.width = col.width
  }
  if (col.align) {
    style.textAlign = col.align
  }
  return style
}

const cellText = (data: Record<string, any>, field: string) => {
  const value = data?.[field]
  return value === null || value === undefined || value === '' ? '-' : String(value)
}

const isRevealed = (field: string, index: number) => revealed.value.has(`${field}:${index}`)

const toggleReveal = (field: string, index: number) => {
  const key = `${field}:${index}`
  if (revealed.value.has(key)) {
    revealed.value.delete(key)
  } else {
    revealed.value.add(key)
  }
}

/** 快照时间兼容 long 秒（formatUnix）与字符串（原样展示）。 */
const formatSnapshot = (value: string | number) => (typeof value === 'number' ? formatUnix(value) : String(value))

/** 首页/重查加载：重建状态（清空 items、回到无 cursor 语义）。 */
const reload = async () => {
  const fresh = createCursorPagerState<Record<string, any>>()
  Object.assign(state, fresh)
  revealed.value = new Set<string>()
  beginLoad(state)
  try {
    const page = await props.request(buildRequestParams(state, props.pageSize))
    applyFirstPage(state, page)
  } catch {
    failLoad(state)
  }
}

/** 加载更多：携带上一响应 nextCursor 原样透传。 */
const loadMore = async () => {
  if (state.loading || !state.hasMore) {
    return
  }
  beginLoad(state)
  try {
    const page = await props.request(buildRequestParams(state, props.pageSize))
    appendPage(state, page)
  } catch {
    failLoad(state)
  }
}

defineExpose({ reload, state })
</script>
