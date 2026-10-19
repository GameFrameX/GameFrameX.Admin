<template>
  <div class="home-notice">
    <div class="head-row">
      <span class="title">{{ t('message.home.noticeTitle') }}</span>
      <span class="mono-tag mono">NOTICE</span>
    </div>
    <ul v-if="notices.length > 0" class="notice-list">
      <li v-for="item in notices" :key="item.id">
        <span class="n-title" :title="item.title">{{ item.title }}</span>
        <span class="n-date">{{ formatNoticeTime(item.publicTime) }}</span>
      </li>
    </ul>
    <div v-else class="empty">{{ t('message.home.noticeEmpty') }}</div>
  </div>
</template>

<script setup lang="ts" name="HomeNoticeCard">
// HOME-001 公告卡：真实接口拉最近 4 条（tenantNoticePageReceivedGet），
// 空态是邀请行动而非空白装饰。
import type {HomeNoticeItem} from '/@/views/tenant/home/composables/usePlatformHomeData';
import {formatUnix} from '/@/utils/formatTime';

defineProps<{ notices: HomeNoticeItem[] }>();

const formatNoticeTime = (unix: number | null): string => {
  if (unix == null || unix === 0) {
    return '—';
  }
  return formatUnix(unix).slice(5, 10);
};
</script>

<style scoped lang="scss">
.home-notice {
  .mono {
    font-family: ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    font-size: 11px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }

  .head-row {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    margin-bottom: 6px;

    .title {
      font-weight: 600;
      color: var(--p-text-color);
      font-size: 14px;
    }

    .mono-tag {
      color: var(--p-text-muted-color);
    }
  }

  .notice-list {
    list-style: none;
    margin: 0;
    padding: 0;

    li {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: baseline;
      padding: 9px 2px;
      border-bottom: 1px dashed var(--p-content-border-color);

      &:last-child {
        border-bottom: 0;
      }
    }

    .n-title {
      color: var(--p-text-color);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      min-width: 0;
    }

    .n-date {
      color: var(--p-text-muted-color);
      font-size: 12px;
      white-space: nowrap;
    }
  }

  .empty {
    color: var(--p-text-muted-color);
    padding: 18px 2px 6px;
    font-size: 13px;
  }
}
</style>
