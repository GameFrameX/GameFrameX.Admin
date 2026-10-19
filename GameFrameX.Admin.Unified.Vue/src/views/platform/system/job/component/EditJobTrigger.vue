<template>
  <div class="sys-jobTrigger-container">
    <Dialog v-model:visible="state.dialogVisible" draggable modal :style="{ width: 'min(700px, calc(100vw - 32px))' }"
            :header="props.title">
      <form @submit.prevent>
        <div class="grid grid-cols-2" style="gap: 16px;">
          <div class="field">
            <label>{{ t('system.job.triggerId') }} <span class="text-red-500">*</span></label>
            <InputText v-model="state.ruleForm.triggerId" :placeholder="t('system.job.triggerId')" class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.triggerType') }}</label>
            <Select filter v-model="state.ruleForm.triggerType" :options="triggerTypeOptions" optionLabel="label"
                    optionValue="value" class="w-full"/>
          </div>
          <div v-if="state.ruleForm.triggerType == 'GameFrameX.Core.Job.PeriodTrigger'" class="field">
            <label>{{ t('system.job.periodMs') }}</label>
            <InputNumber v-model="periodValue" :placeholder="t('system.job.periodMs')" :min="1000" :step="1000"
                         class="w-full"/>
          </div>
          <div v-else class="field col-span-2">
            <label>{{ t('system.job.cronTrigger') }}</label>
            <div class="flex gap-2 align-items-center">
              <InputText v-model="cronValue" :placeholder="t('system.job.cronExpression')" class="flex-1"/>
              <Menu ref="macroMenuRef" :model="macroMenuItems" :popup="true"/>
              <Button severity="secondary" @click="($refs.macroMenuRef as any)?.toggle($event)">Macro</Button>
              <Button severity="secondary" @click="state.showCronDialog = true">
                {{ t('system.job.cronExpression') }}
              </Button>
            </div>
          </div>
          <div class="field">
            <label>{{ t('system.job.startTime') }}</label>
            <DatePicker v-model="state.ruleForm.startTime" showTime :placeholder="t('system.job.startTime')"
                        class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.endTime') }}</label>
            <DatePicker v-model="state.ruleForm.endTime" showTime :placeholder="t('system.job.endTime')" class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.maxRunCount') }}</label>
            <InputNumber v-model="state.ruleForm.maxNumberOfRuns" :placeholder="t('system.job.maxRunCount')"
                         class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.maxErrorCount') }}</label>
            <InputNumber v-model="state.ruleForm.maxNumberOfErrors" :placeholder="t('system.job.maxErrorCount')"
                         class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.retryCount') }}</label>
            <InputNumber v-model="state.ruleForm.numRetries" :placeholder="t('system.job.retryCount')" class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.retryIntervalMs') }}</label>
            <InputNumber v-model="state.ruleForm.retryTimeout" :placeholder="t('system.job.retryIntervalMs')"
                         class="w-full"/>
          </div>
          <div class="field">
            <label>{{ t('system.job.startNow') }}</label>
            <div class="flex gap-4 mt-1">
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.startNow" :value="true"/>
                <label>{{ t('common.label.yes') }}</label>
              </div>
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.startNow" :value="false"/>
                <label>{{ t('common.label.no') }}</label>
              </div>
            </div>
          </div>
          <div class="field">
            <label>{{ t('system.job.runOnceOnStart') }}</label>
            <div class="flex gap-4 mt-1">
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.runOnStart" :value="true"/>
                <label>{{ t('common.label.yes') }}</label>
              </div>
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.runOnStart" :value="false"/>
                <label>{{ t('common.label.no') }}</label>
              </div>
            </div>
          </div>
          <div class="field col-span-2">
            <label>
              {{ t('system.job.resetRunCount') }}
              <span v-tooltip="t('system.job.resetRunCountTip')" style="cursor: help">
                <i class="fa-regular fa-circle-question" style="font-size: 14px; vertical-align: middle"></i>
              </span>
            </label>
            <div class="flex gap-4 mt-1">
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.resetOnlyOnce" :value="true"/>
                <label>{{ t('common.label.yes') }}</label>
              </div>
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="state.ruleForm.resetOnlyOnce" :value="false"/>
                <label>{{ t('common.label.no') }}</label>
              </div>
            </div>
          </div>
          <div class="field col-span-2">
            <label>{{ t('system.job.description') }}</label>
            <Textarea v-model="state.ruleForm.description" :placeholder="t('system.job.description')"
                      :autoResize="true" rows="2" class="w-full"/>
          </div>
        </div>
      </form>
      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>

    <Dialog v-model:visible="state.showCronDialog" draggable modal
            :header="t('system.job.cronGenerator')">
      <cron-light v-model="cronValue"/>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemEditJobTrigger">

import {reactive, ref, computed} from 'vue';
import type {WritableComputedRef} from 'vue';
import {showWarning} from '../../../../../utils/message.ts';

import {
  systemJobUpdateJobTriggerPost,
  systemJobAddJobTriggerPost,
} from '../../../../../api/systemJob';
import {UpdateJobTriggerInput} from '../../../../../api/_shared/data-contracts.ts';

interface MacroData {
  key: string;
  description: string;
}

const props = defineProps({
  title: String,
});
const emits = defineEmits(['query']);
const state = reactive({
  dialogVisible: false,
  ruleForm: {} as UpdateJobTriggerInput,
  showCronDialog: false,
});

const macroData: MacroData[] = reactive([
  {key: '@secondly', description: t('system.job.periodTrigger')},
  {key: '@minutely', description: t('system.job.periodTrigger')},
  {key: '@hourly', description: t('system.job.periodTrigger')},
  {key: '@daily', description: t('system.job.periodTrigger')},
  {key: '@monthly', description: t('system.job.periodTrigger')},
  {key: '@weekly', description: t('system.job.periodTrigger')},
  {key: '@yearly', description: t('system.job.periodTrigger')},
  {key: '@workday', description: t('system.job.workdayDesc')},
]);

const macroMenuItems = macroData.map(item => ({
  label: `${item.key} - ${item.description}`,
  command: () => {
    cronValue.value = item.key;
  },
}));

const triggerTypeOptions = [
  {label: t('system.job.periodTrigger'), value: 'GameFrameX.Core.Job.PeriodTrigger'},
  {label: t('system.job.cronTrigger'), value: 'GameFrameX.Core.Job.CronTrigger'},
];

const periodValue: WritableComputedRef<number | undefined> = computed({
  get() {
    const defaultValue: number | undefined = undefined;
    if (state.ruleForm.triggerType != 'GameFrameX.Core.Job.PeriodTrigger') return defaultValue;
    if (!state.ruleForm.args) return defaultValue;
    const value: number | undefined = Number(state.ruleForm.args);
    if (Number.isNaN(value)) return defaultValue;
    return value;
  },
  set(value: number | undefined) {
    state.ruleForm.args = String(value);
  },
});

const cronValue: WritableComputedRef<string> = computed({
  get() {
    const defaultValue = '';
    if (state.ruleForm.triggerType != 'GameFrameX.Core.Job.CronTrigger') return defaultValue;
    if (!state.ruleForm.args) return defaultValue;
    const value = String(state.ruleForm.args);
    const parameters = value.split(',');
    if (parameters.length < 2) return defaultValue;
    else if (parameters.length == 2) {
      return parameters[0].replace(new RegExp('"', 'gm'), '').trim();
    } else {
      const temp = value.substring(0, value.lastIndexOf(','));
      return temp.replace(new RegExp('"', 'gm'), '').trim();
    }
  },
  set(value: string) {
    if (state.ruleForm.args == value) return;
    const newValue = value.trim();
    let cronStringFormatValue = -1;
    if (newValue.startsWith('@')) cronStringFormatValue = 0;
    else {
      if (newValue.split(' ').length == 6) cronStringFormatValue = 2;
      else cronStringFormatValue = 3;
    }
    state.ruleForm.args = `"${newValue}",${cronStringFormatValue}`;
  },
});

const openDialog = (row: any) => {
  const cloned = JSON.parse(JSON.stringify(row));
  // 回显：long(秒) → Date 供 DatePicker 显示
  if (cloned.startTime && typeof cloned.startTime === 'number') {
    cloned.startTime = new Date(cloned.startTime * 1000);
  }
  if (cloned.endTime && typeof cloned.endTime === 'number') {
    cloned.endTime = new Date(cloned.endTime * 1000);
  }
  state.ruleForm = cloned;
  state.dialogVisible = true;
};

const closeDialog = () => {
  emits('query');
  state.dialogVisible = false;
};

const cancel = () => {
  state.dialogVisible = false;
};

const submit = async () => {
  if (state.ruleForm.triggerType == 'GameFrameX.Core.Job.PeriodTrigger' && !periodValue.value) {
    showWarning(t('system.job.periodMsRequired'));
    return;
  } else if (state.ruleForm.triggerType == 'GameFrameX.Core.Job.CronTrigger' && !cronValue.value) {
    showWarning(t('system.job.cronExpressionRequired'));
    return;
  }

  const payload: any = {...state.ruleForm};
  // 提交：Date → long 秒；空值保持 null
  if (payload.startTime) {
    payload.startTime = Math.floor(new Date(payload.startTime).getTime() / 1000);
  }
  if (payload.endTime) {
    payload.endTime = Math.floor(new Date(payload.endTime).getTime() / 1000);
  }

  if (payload.id != undefined && payload.id > 0) {
    await systemJobUpdateJobTriggerPost(payload);
  } else {
    await systemJobAddJobTriggerPost(payload);
  }
  closeDialog();
};

defineExpose({openDialog});
</script>
