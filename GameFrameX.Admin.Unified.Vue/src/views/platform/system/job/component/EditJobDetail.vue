<template>
  <div class="sys-jobDetail-container">
    <Dialog v-model:visible="state.dialogVisible" draggable modal :style="{ width: 'min(900px, calc(100vw - 32px))' }"
            :header="props.title">
      <Tabs v-model:value="state.selectedTabName">
        <TabList>
          <Tab value="0">{{ t('system.job.jobInfo') }}</Tab>
          <Tab value="1" :disabled="!isScriptCreateType">{{ t('system.job.scriptCode') }}</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <div style="display: grid; grid-template-columns: auto 1fr; gap: 12px 16px; align-items: start;">
              <FormLabel required>{{ t('system.job.jobId') }}</FormLabel>
              <div><InputText v-model="state.ruleForm.jobId" :placeholder="t('system.job.jobId')" :disabled="isEdit" class="w-full"/></div>
              <FormLabel required>{{ t('system.job.groupName') }}</FormLabel>
              <div><InputText v-model="state.ruleForm.groupName" :placeholder="t('system.job.groupName')" class="w-full"/></div>
              <FormLabel>{{ t('system.job.createType') }}</FormLabel>
              <div class="flex gap-4">
                <div v-if="isEdit" class="flex align-items-center gap-2">
                  <RadioButton v-model="state.ruleForm.createType" :value="JobCreateTypeEnum.Value0" :disabled="isEdit"/>
                  <label>{{ t('system.job.builtIn') }}</label>
                </div>
                <div class="flex align-items-center gap-2">
                  <RadioButton v-model="state.ruleForm.createType" :value="JobCreateTypeEnum.Value1" :disabled="isEdit"/>
                  <label>{{ t('system.job.script') }}</label>
                </div>
                <div class="flex align-items-center gap-2">
                  <RadioButton v-model="state.ruleForm.createType" :value="JobCreateTypeEnum.Value2" :disabled="isEdit"/>
                  <label>{{ t('system.job.httpRequest') }}</label>
                </div>
              </div>
              <FormLabel>{{ t('system.job.executeMode') }}</FormLabel>
              <div class="flex gap-4">
                <div class="flex align-items-center gap-2">
                  <RadioButton v-model="state.ruleForm.concurrent" :value="true"/>
                  <label>{{ t('system.job.parallel') }}</label>
                </div>
                <div class="flex align-items-center gap-2">
                  <RadioButton v-model="state.ruleForm.concurrent" :value="false"/>
                  <label>{{ t('system.job.serial') }}</label>
                </div>
              </div>
              <template v-if="!isEdit && !isHttpCreateType">
                <FormLabel>
                  {{ t('system.job.scanTrigger') }}
                  <span v-tooltip="t('system.job.scanTriggerTip')" style="cursor: help">
                    <i class="fa-regular fa-circle-question" style="font-size: 14px; vertical-align: middle"></i>
                  </span>
                </FormLabel>
                <div class="flex gap-4">
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.ruleForm.includeAnnotations" :value="true"/>
                    <label>{{ t('common.label.yes') }}</label>
                  </div>
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.ruleForm.includeAnnotations" :value="false"/>
                    <label>{{ t('common.label.no') }}</label>
                  </div>
                </div>
              </template>
              <FormLabel>{{ t('system.job.description') }}</FormLabel>
              <div><Textarea v-model="state.ruleForm.description" :placeholder="t('system.job.description')" :autoResize="true" rows="1" class="w-full"/></div>
              <template v-if="!isHttpCreateType">
                <FormLabel>{{ t('system.job.extraData') }}</FormLabel>
                <div><Textarea v-model="state.ruleForm.properties" :placeholder="t('system.job.extraData')" :autoResize="true" rows="3" class="w-full"/></div>
              </template>
              <template v-if="isHttpCreateType">
                <FormLabel>{{ t('system.job.requestUrl') }}</FormLabel>
                <div><InputText v-model="state.httpJobMessage.requestUri" :placeholder="t('system.job.requestUrl')" class="w-full"/></div>
                <FormLabel>{{ t('system.job.requestMethod') }}</FormLabel>
                <div class="flex gap-4">
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.httpJobMessage.httpMethod" :value="httpMethodDef.get"/>
                    <label>Get</label>
                  </div>
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.httpJobMessage.httpMethod" :value="httpMethodDef.post"/>
                    <label>Post</label>
                  </div>
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.httpJobMessage.httpMethod" :value="httpMethodDef.put"/>
                    <label>Put</label>
                  </div>
                  <div class="flex align-items-center gap-2">
                    <RadioButton v-model="state.httpJobMessage.httpMethod" :value="httpMethodDef.delete"/>
                    <label>Delete</label>
                  </div>
                </div>
                <FormLabel>{{ t('system.job.requestBody') }}</FormLabel>
                <div><Textarea v-model="state.httpJobMessage.body" :placeholder="t('system.job.requestBody')" :autoResize="true" rows="3" class="w-full"/></div>
              </template>
            </div>
          </TabPanel>
          <TabPanel value="1">
            <NarrowScreenHint/>
            <div ref="monacoEditorRef" style="width: 100%; height: 500px"></div>
          </TabPanel>
        </TabPanels>
      </Tabs>
      <template #footer>
        <Button severity="secondary" @click="cancel">{{ t('common.button.cancel') }}</Button>
        <Button @click="submit">{{ t('common.button.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemEditJobDetail">

import {reactive, ref, computed} from 'vue';
import * as monaco from 'monaco-editor'
import NarrowScreenHint from '/@/components/narrow-screen-hint/index.vue';
import {jobScriptCodeTemplate} from './JobScriptCode';

import {
  systemJobUpdateJobDetailPost,
  systemJobAddJobDetailPost,
} from '../../../../../api/systemJob';
import {JobCreateTypeEnum, UpdateJobDetailInput} from '../../../../../api/_shared/data-contracts.ts';

const httpMethodDef = {
  get: '{"Method":"GET"}',
  post: '{"Method":"POST"}',
  put: '{"Method":"PUT"}',
  delete: '{"Method":"DELETE"}',
};

const props = defineProps({
  title: String,
});
const emits = defineEmits(['query']);
const monacoEditorRef = ref();
const state = reactive({
  dialogVisible: false,
  selectedTabName: '0',
  ruleForm: {} as UpdateJobDetailInput,
  httpJobMessage: {requestUri: '', httpMethod: httpMethodDef.get, body: ''} as HttpJobMessage,
});

const isEdit = computed(() => {
  return state.ruleForm.id != undefined && state.ruleForm.id > 0;
});

const isScriptCreateType = computed(() => {
  return state.ruleForm.createType === JobCreateTypeEnum.Value1;
});

const isHttpCreateType = computed(() => {
  return state.ruleForm.createType === JobCreateTypeEnum.Value2;
});

var monacoEditor: any = null;
const initMonacoEditor = () => {
  monacoEditor = monaco.editor.create(monacoEditorRef.value, {
    theme: 'vs-dark',
    value: '',
    language: 'csharp',
    formatOnPaste: true,
    wordWrap: 'on',
    wrappingIndent: 'indent',
    folding: true,
    foldingHighlight: true,
    foldingStrategy: 'indentation',
    showFoldingControls: 'always',
    disableLayerHinting: true,
    emptySelectionClipboard: false,
    selectionClipboard: false,
    automaticLayout: true,
    codeLens: false,
    scrollBeyondLastLine: false,
    colorDecorators: true,
    accessibilitySupport: 'auto',
    lineNumbers: 'on',
    lineNumbersMinChars: 5,
    readOnly: false,
  });
};

const openDialog = (row: any) => {
  state.selectedTabName = '0';
  state.ruleForm = JSON.parse(JSON.stringify(row));
  state.dialogVisible = true;

  if (row.id && state.ruleForm.createType === JobCreateTypeEnum.Value2) {
    state.httpJobMessage = getHttpJobMessage(state.ruleForm.properties);
  }

  setTimeout(() => {
    if (monacoEditor == null) initMonacoEditor();
    monacoEditor.setValue(row.id == undefined ? jobScriptCodeTemplate : state.ruleForm.scriptCode);
  }, 1);
};

const closeDialog = () => {
  emits('query');
  state.dialogVisible = false;
};

const cancel = () => {
  state.dialogVisible = false;
};

const submit = async () => {
  if (state.ruleForm.createType === JobCreateTypeEnum.Value1) {
    state.ruleForm.scriptCode = monacoEditor.getValue();
  } else {
    state.ruleForm.scriptCode = '';
  }

  if (state.ruleForm.createType === JobCreateTypeEnum.Value2) {
    const httpJobPropValue = JSON.stringify({
      RequestUri: state.httpJobMessage.requestUri,
      HttpMethod: JSON.parse(state.httpJobMessage.httpMethod + ''),
      Body: state.httpJobMessage.body,
      ClientName: 'HttpJob',
      EnsureSuccessStatusCode: true,
    });
    const prop = {HttpJob: httpJobPropValue};
    state.ruleForm.properties = JSON.stringify(prop);
  }

  if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
    await systemJobUpdateJobDetailPost(state.ruleForm);
  } else {
    await systemJobAddJobDetailPost(state.ruleForm);
  }
  closeDialog();
};

const getHttpJobMessage = (properties: string | undefined | null): HttpJobMessage => {
  if (properties === undefined || properties === null || properties === '') return {};
  const propData = JSON.parse(properties);
  const httpJobMessageNet = JSON.parse(propData['HttpJob']);
  return {
    requestUri: httpJobMessageNet.RequestUri,
    httpMethod: JSON.stringify(httpJobMessageNet.HttpMethod),
    body: httpJobMessageNet.Body,
  };
};

defineExpose({httpMethodDef, openDialog, getHttpJobMessage});
</script>
