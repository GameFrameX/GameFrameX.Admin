<template>
  <div class="sys-server-container">
    <div class="flex gap-2">
      <div class="flex-1" style="min-width: 300px">
        <Card>
          <template #header>
            <div style="padding: 12px 20px; font-weight: 600">{{ t('system.server.serverInfo') }}</div>
          </template>
          <template #content>
            <table class="systemInfo_table">
              <tbody>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.hostName') }}</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.hostName }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.log.os') }}</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.systemOs }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.osArchitecture') }}</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.osArchitecture }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.processorCount') }}</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.processorCount }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.systemRunTime') }}</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.systemRunTime }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.externalAddress') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.remoteIp }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.internalAddress') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.localIp }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.runFramework') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.frameworkDescription }}</td>
                </tr>
              </tbody>
            </table>
          </template>
        </Card>
      </div>
      <div class="flex-1" style="min-width: 300px">
        <Card>
          <template #header>
            <div style="padding: 12px 20px; font-weight: 600">{{ t('system.server.usageInfo') }}</div>
          </template>
          <template #content>
            <div class="flex">
              <div class="flex-1" style="text-align: center">
                <div class="inline-block text-center">
                  <Knob
                      v-model="ramPercent"
                      :size="120"
                      :strokeWidth="8"
                      valueTemplate="{value}%"
                  />
                  <div style="font-size: 12px; margin-top: -10px">
                    {{ t('system.server.used') }}: {{ state.machineUseInfo.usedRam }}<br/>
                    {{ t('system.server.free') }}: {{ state.machineUseInfo.freeRam }}<br/>
                    {{ t('system.server.memoryUsage') }}
                  </div>
                </div>
              </div>
              <div class="flex-1" style="text-align: center">
                <div class="inline-block text-center">
                  <Knob
                      v-model="cpuPercent"
                      :size="120"
                      :strokeWidth="8"
                      valueTemplate="{value}%"
                  />
                  <div style="font-size: 12px; margin-top: -10px">
                    {{ t('system.server.cpuUsage') }}
                  </div>
                </div>
              </div>
            </div>

            <table class="systemInfo_table">
              <tbody>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.startTime') }}：</td>
                  <td class="systemInfo_td">{{ state.machineUseInfo.startTime }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.systemRunTime') }}</td>
                  <td class="systemInfo_td">{{ state.machineUseInfo.runTime }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.websiteDir') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.wwwroot }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.devEnvironment') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.environment }}</td>
                </tr>
                <tr>
                  <td class="systemInfo_td">{{ t('system.server.environmentVar') }}：</td>
                  <td class="systemInfo_td">{{ state.machineBaseInfo.stage }}</td>
                </tr>
              </tbody>
            </table>
          </template>
        </Card>
      </div>
    </div>

    <Card style="margin-top: 8px">
      <template #header>
        <div style="padding: 10px 20px; font-weight: 600">{{ t('system.server.assemblyInfo') }}</div>
      </template>
      <template #content>
        <div style="padding: 0 10px 10px">
          <Tag v-for="d in state.assemblyInfo" :key="d.name" rounded style="margin: 4px">
            {{ d.name }} <span style="color: var(--p-text-color); font-size: 12px; margin-left: 3px">v{{ d.version }}</span>
          </Tag>
          <div v-if="state.assemblyInfo.length === 0" class="empty-monitor-text">-</div>
        </div>
      </template>
    </Card>

    <Card style="margin-top: 8px">
      <template #header>
        <div style="padding: 12px 20px; font-weight: 600">{{ t('system.server.diskInfo') }}</div>
      </template>
      <template #content>
        <div class="flex flex-wrap">
          <div v-for="d in state.machineDiskInfo" :key="d.diskName" class="flex-1" style="min-width: 150px; text-align: center; padding: 10px">
            <Knob
                :modelValue="d.availablePercent"
                :size="120"
                :strokeWidth="8"
                valueTemplate="{value}%"
                readonly
            />
            <div style="font-size: 12px; margin-top: -10px">
              {{ t('system.server.used') }}: {{ d.used }}GB<br/>
              {{ t('system.server.free') }}: {{ d.availableFreeSpace }}GB<br/>
              {{ d.diskName }}
            </div>
          </div>
          <div v-if="state.machineDiskInfo.length === 0" class="empty-monitor-text">-</div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script lang="ts" setup name="systemServer">
import {onActivated, onDeactivated, onMounted, reactive, computed} from 'vue';

import {
  systemServerServerBaseGet,
  systemServerServerUsedGet,
  systemServerServerDiskGet,
  systemServerAssemblyListGet,
} from '../../../../api/systemServer';

const emptyMachineBaseInfo = () => ({
  hostName: '-',
  systemOs: '-',
  osArchitecture: '-',
  processorCount: '-',
  systemRunTime: '-',
  remoteIp: '-',
  localIp: '-',
  frameworkDescription: '-',
  environment: '-',
  wwwroot: '-',
  stage: '-',
});

const emptyMachineUseInfo = () => ({
  freeRam: '0GB',
  usedRam: '0GB',
  totalRam: '0GB',
  ramRate: '0%',
  cpuRate: '0%',
  startTime: '-',
  runTime: '-',
});

const state = reactive({
  machineBaseInfo: emptyMachineBaseInfo() as any,
  machineUseInfo: emptyMachineUseInfo() as any,
  machineDiskInfo: [] as any,
  assemblyInfo: [] as any,
  timer: null as any,
});

const ramPercent = computed({
  get: () => {
    const rate = state.machineUseInfo.ramRate;
    if (!rate) return 0;
    return parseInt(rate.substr(0, rate.length - 1));
  },
  set: () => {},
});

const cpuPercent = computed({
  get: () => {
    const rate = state.machineUseInfo.cpuRate;
    if (!rate) return 0;
    return parseInt(rate.substr(0, rate.length - 1));
  },
  set: () => {},
});

onMounted(async () => {
  await Promise.all([
    loadMachineBaseInfo(),
    loadMachineUseInfo(),
    loadMachineDiskInfo(),
    loadAssemblyInfo(),
  ]);
});

const resolveResult = <T,>(res: any, fallback: T): T => {
  const data = res?.data ?? res;
  const result = data?.data ?? data;
  return result == null ? fallback : result;
};

const resolveArrayResult = (res: any) => {
  const result = resolveResult(res, []);
  return Array.isArray(result) ? result : [];
};

const loadMachineBaseInfo = async () => {
  try {
    const res = await systemServerServerBaseGet();
    state.machineBaseInfo = {...emptyMachineBaseInfo(), ...resolveResult(res, {})};
  } catch (error) {
    console.error('[systemServer] loadMachineBaseInfo failed', error);
    state.machineBaseInfo = emptyMachineBaseInfo();
  }
};

const loadMachineUseInfo = async () => {
  try {
    const res = await systemServerServerUsedGet();
    state.machineUseInfo = {...emptyMachineUseInfo(), ...resolveResult(res, {})};
  } catch (error) {
    console.error('[systemServer] loadMachineUseInfo failed', error);
    state.machineUseInfo = emptyMachineUseInfo();
  }
};

const loadMachineDiskInfo = async () => {
  try {
    const res = await systemServerServerDiskGet();
    state.machineDiskInfo = resolveArrayResult(res);
  } catch (error) {
    console.error('[systemServer] loadMachineDiskInfo failed', error);
    state.machineDiskInfo = [];
  }
};

const loadAssemblyInfo = async () => {
  try {
    const res = await systemServerAssemblyListGet();
    state.assemblyInfo = resolveArrayResult(res);
  } catch (error) {
    console.error('[systemServer] loadAssemblyInfo failed', error);
    state.assemblyInfo = [];
  }
};

const refreshData = () => {
  loadMachineUseInfo();
};

onActivated(() => {
  state.timer = setInterval(() => {
    refreshData();
  }, 10000);
});

onDeactivated(() => {
  clearInterval(state.timer);
});
</script>

<style lang="scss" scoped>
.systemInfo_table {
  width: 100%;
  min-height: 40px;
  line-height: 40px;
  text-align: center;
}

.systemInfo_td {
  border-bottom: 1px solid var(--p-content-border-color);
}

.empty-monitor-text {
  width: 100%;
  padding: 20px;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>
