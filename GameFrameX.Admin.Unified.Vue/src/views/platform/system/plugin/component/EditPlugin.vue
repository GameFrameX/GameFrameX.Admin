<template>
  <BaseEditDialog
    ref="dialogRef"
    :title="props.title"
    :addApi="(params: any) => systemPluginAddPost(params)"
    :updateApi="(params: any) => systemPluginUpdatePost(params)"
    :beforeSubmit="beforeSubmit"
    idCheckMode="system"
    @reloadTable="emits('query')"
  >
    <template #default="{ ruleForm }">
      <div style="grid-column: 1 / -1">
        <Tabs v-model:value="selectedTabName">
          <TabList>
            <Tab value="0">{{ t('system.plugin.pluginName') }}</Tab>
            <Tab value="1">{{ t('system.plugin.csharpCode') }}</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <div style="height: 500px; overflow-y: auto">
                <div class="grid" style="gap: 0px">
                  <div class="col-12 mb20">
                    <div class="field mb20">
                      <label>{{ t('system.plugin.pluginName') }}</label>
                      <InputText v-model="ruleForm.name" :placeholder="t('system.plugin.pluginName')" class="w100"/>
                    </div>
                  </div>
                  <div class="col-12 mb20">
                    <div class="field mb20">
                      <label>{{ t('system.plugin.pluginCode') }}</label>
                      <InputText v-model="ruleForm.assemblyName" :placeholder="t('system.plugin.assemblyName')" class="w100"/>
                    </div>
                  </div>
                  <div class="col-6 mb20">
                    <div class="field mb20">
                      <label>{{ t('common.label.sort') }}</label>
                      <InputNumber v-model="ruleForm.orderNo" :placeholder="t('common.label.sort')" class="w100"/>
                    </div>
                  </div>
                  <div class="col-6 mb20">
                    <div class="field mb20">
                      <label>{{ t('common.label.status') }}</label>
                      <div class="mt-1"><ToggleSwitch v-model="ruleForm.isEnabled"/></div>
                    </div>
                  </div>
                  <div class="col-12 mb20">
                    <div class="field mb20">
                      <label>{{ t('common.label.remark') }}</label>
                      <Textarea v-model="ruleForm.remark" :placeholder="t('common.form.pleaseInput')" class="w100" rows="3"/>
                    </div>
                  </div>
                </div>
              </div>
            </TabPanel>
            <TabPanel value="1">
              <NarrowScreenHint/>
              <div ref="monacoEditorRef" style="width: 100%; height: 500px"></div>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </BaseEditDialog>
</template>

<script lang="ts" setup name="systemEditPlugin">

import {ref} from 'vue'
import {showWarning} from '../../../../../utils/message.ts'
import * as monaco from 'monaco-editor'
import NarrowScreenHint from '/@/components/narrow-screen-hint/index.vue'
import BaseEditDialog from '/@/components/base-edit-dialog/index.vue'
import {systemPluginAddPost, systemPluginUpdatePost} from '../../../../../api/systemPlugin';

const props = defineProps({title: String})
const emits = defineEmits(['query'])

const dialogRef = ref()
const monacoEditorRef = ref()
const selectedTabName = ref('0')
var monacoEditor: any = null

const beforeSubmit = (form: any) => {
  form.csharpCode = monacoEditor?.getValue() ?? ''
  if (form.csharpCode.length < 100) {
    showWarning(t('system.plugin.csharpCodeRequired'))
    return false
  }
  return form
}

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
  })
}

defineExpose({
  openDialog: (row: any) => {
    dialogRef.value?.openDialog(row)
    setTimeout(() => {
      if (monacoEditor == null) initMonacoEditor()
    }, 1)
  }
})
</script>
