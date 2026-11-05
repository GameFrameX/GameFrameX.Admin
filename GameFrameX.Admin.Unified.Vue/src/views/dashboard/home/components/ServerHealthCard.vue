<template>
  <section class="server-health-card" aria-labelledby="server-health-title" :aria-busy="serverStore.loading">
    <header>
      <h2 id="server-health-title">{{ t('message.home.serverHealth') }}</h2>
      <Button text :label="t('message.common.button.refresh')" :loading="serverStore.loading" @click="refreshServers"/>
    </header>
    <p class="description">{{ t('message.home.serverHealthDescription') }}</p>
    <p v-if="serverStore.loadFailed" role="alert">{{ t('message.menu.loadServersFailed') }}</p>
    <p v-else-if="serverStore.loading || !serverStore.loaded" role="status">{{ t('message.home.loadingSummary') }}</p>
    <p v-else-if="serverStore.servers.length === 0" role="status">{{ t('message.menu.noServersAvailable') }}</p>
    <template v-else>
      <dl class="status-counts">
        <div v-for="status in statusCounts" :key="status.value" :data-server-status="status.value">
          <dt>{{ status.label }}</dt>
          <dd>{{ status.count.toLocaleString() }}</dd>
        </div>
      </dl>
      <p class="selected-server">{{ t('message.menu.selectServer') }}: {{ selectedServer?.name ?? '—' }}</p>
    </template>
  </section>
</template>

<script setup lang="ts">
import {computed, onMounted, ref} from 'vue';
import Button from 'primevue/button';
import {useServerStore} from '../../../../stores/server';
import {GameServerStatus} from '../../../../api/gameArea/types';
import {systemEnumEnumDataListGet} from '../../../../api/systemEnum';
import type {EnumItem} from '../../../../api/systemEnum/types';
import commonFunction from '../../../../utils/commonFunction';
import {t} from '../../../../i18n/index';

const serverStore = useServerStore();
const statusDefinitions = ref<EnumItem[]>([]);
const {getEnumDesc} = commonFunction();
const statusValues = [GameServerStatus.Online, GameServerStatus.Offline, GameServerStatus.Maintenance, GameServerStatus.Full, GameServerStatus.Unknown];
const statusCounts = computed(() => statusValues.map(value => ({
  value,
  label: getEnumDesc(value, statusDefinitions.value) ?? GameServerStatus[value],
  count: serverStore.servers.filter(server => server.serverStatus === value).length,
})));
const selectedServer = computed(() => serverStore.servers.find(server => server.id === serverStore.selectedServerId));

async function refreshServers(): Promise<void> {
  await serverStore.loadServers(true).catch(() => {
    // store 负责清除失效数据，卡片显示错误且保留重试入口。
  });
}

onMounted(async () => {
  try {
    // 与区服列表复用枚举描述端点及共享格式化函数，不依赖业务页组件。
    const response = await systemEnumEnumDataListGet({enumName: 'GameServerStatus'});
    statusDefinitions.value = response.data.data ?? [];
  } catch {
    // 描述查询失败时显示枚举名称，保留真实状态计数。
  }
});
</script>

<style scoped lang="scss">
.server-health-card {
  margin-bottom: 18px;
  padding: 16px 18px;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
  background: var(--p-content-background);
  color: var(--p-text-color);
  header { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
  h2 { margin: 0; font-size: 16px; font-weight: 600; }
  .description, .selected-server { color: var(--p-text-muted-color); font-size: 13px; }
  .status-counts {
    display: grid;
    grid-template-columns: repeat(5, minmax(0, 1fr));
    gap: 16px;
    dt { color: var(--p-text-muted-color); font-size: 13px; }
    dd { margin: 6px 0 0; font-size: 24px; font-variant-numeric: tabular-nums; }
  }
  @include bp-down(sm) {
    .status-counts { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  }
}
</style>
