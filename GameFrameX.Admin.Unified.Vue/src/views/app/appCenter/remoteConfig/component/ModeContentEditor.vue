<template>
  <div>
    <Tabs v-model:value="currentModeCode">
      <TabList>
        <Tab v-for="modeDef in modeDefs" :key="modeDef.modeCode" :value="modeDef.modeCode">
          <span>{{ modeDef.displayName }}</span>
          <Tag v-if="activeModeCode === modeDef.modeCode" severity="success" style="margin-left: 6px;" :value="t('message.client.inUse')"/>
        </Tab>
      </TabList>
    </Tabs>

    <div v-if="activeModeDef" :key="`mode-content-${currentModeCode}`" class="mode-content-editor__content">
      <div v-for="keyDef in contentKeyDefs" :key="`${currentModeCode}-${keyDef.keyName}`" class="mode-content-editor__row">
        <span v-if="keyDef.description" v-tooltip.top="keyDef.description" class="mode-content-editor__label">
          {{ keyDef.displayName }}
        </span>
        <span v-else class="mode-content-editor__label">{{ keyDef.displayName }}</span>
        <div class="mode-content-editor__control">
          <ToggleSwitch v-if="keyDef.valueType === 'bool'"
                        :key="`switch-${currentModeCode}-${keyDef.keyName}`"
                        :modelValue="getValue(keyDef)"
                        @update:modelValue="setValue(keyDef.keyName, $event)"/>
          <InputText v-else
                     :key="`${keyDef.valueType}-${currentModeCode}-${keyDef.keyName}`"
                     :modelValue="getValue(keyDef)"
                     @update:modelValue="setValue(keyDef.keyName, $event)"
                     :placeholder="t('message.common.form.pleaseInput')"
                     class="w-full"/>
          <Message v-if="longValueErrors?.[keyDef.keyName]" severity="error" size="small" variant="simple">
            {{ longValueErrors[keyDef.keyName] }}
          </Message>
        </div>
      </div>
    </div>

    <div class="mode-content-editor__mode-row">
      <span class="mode-content-editor__label">{{ t('message.client.currentUsingMode') }}</span>
      <div class="mode-content-editor__radio-group">
        <div v-for="modeDef in modeDefs" :key="modeDef.modeCode" class="flex align-items-center gap-2">
          <RadioButton :modelValue="activeModeCode"
                       :value="modeDef.modeCode"
                       :inputId="`mode-${modeDef.modeCode}`"
                       name="activeModeCode"
                       @update:modelValue="emit('update:activeModeCode', $event)"/>
          <label :for="`mode-${modeDef.modeCode}`">{{ modeDef.displayName }}</label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {computed} from 'vue'
import Message from 'primevue/message'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import RadioButton from 'primevue/radiobutton'

interface ContentKeyDef {
  id: number;
  keyName: string;
  displayName: string;
  valueType: 'string' | 'bool' | 'long';
  description?: string;
  sortOrder: number;
}

interface ModeDef {
  id: number;
  modeCode: string;
  displayName: string;
  description?: string;
  sortOrder: number;
}

const props = defineProps<{
  modeDefs: ModeDef[]
  contentKeyDefs: ContentKeyDef[]
  currentModeCode: string
  activeModeCode: string
  values: Record<string, any>
  longValueErrors?: Record<string, string>
}>()

const emit = defineEmits<{
  (e: 'update:currentModeCode', value: string): void
  (e: 'update:activeModeCode', value: string): void
  (e: 'updateValue', keyName: string, value: any): void
}>()

const currentModeCode = computed({
  get: () => props.currentModeCode,
  set: (value: string) => emit('update:currentModeCode', value),
})

const activeModeDef = computed(() => props.modeDefs.find(x => x.modeCode === props.currentModeCode))

const getDefaultValue = (def: ContentKeyDef) => {
  if (def.valueType === 'bool') return false
  if (def.valueType === 'long') return '0'
  return ''
}

const getValue = (def: ContentKeyDef) => props.values?.[def.keyName] ?? getDefaultValue(def)

const setValue = (keyName: string, value: any) => {
  emit('updateValue', keyName, value)
}
</script>

<style scoped>
.mode-content-editor__content {
  display: grid;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--p-content-border-color);
  border-top: 0;
}

.mode-content-editor__row,
.mode-content-editor__mode-row {
  display: grid;
  grid-template-columns: 140px minmax(0, 1fr);
  gap: 8px 12px;
  align-items: center;
}

.mode-content-editor__mode-row {
  margin-top: 12px;
}

.mode-content-editor__label {
  min-width: 0;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.mode-content-editor__control {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 8px;
}

.mode-content-editor__radio-group {
  display: flex;
  min-width: 0;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}

@media (max-width: 640px) {
  .mode-content-editor__row,
  .mode-content-editor__mode-row {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
