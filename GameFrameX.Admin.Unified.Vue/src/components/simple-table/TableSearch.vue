<template>
  <div class="table-search-container" v-if="props.search.length > 0">
    <form @submit.prevent ref="tableSearchRef" class="table-form">
      <div class="grid" style="gap: 20px">
        <div :class="getColClass" class="mb20" v-for="(val, key) in search" :key="key"
             v-show="key < 3 || state.isToggle">
          <template v-if="val.type !== ''">
            <div class="field mb20">
              <label class="block mb-1">{{ val.label }}</label>
              <InputText
                  v-model="state.form[val.prop]"
                  v-bind="val.comProps"
                  :placeholder="val.placeholder"
                  class="w-full"
                  v-if="val.type === 'input'"
                  @keyup.enter="onSearch"
                  @change="val.change"
              />
              <DatePicker
                  v-model="state.form[val.prop]"
                  v-bind="val.comProps"
                  :placeholder="val.placeholder"
                  v-else-if="val.type === 'date'"
                  @change="val.change"
                  class="w-full"
              />
              <Select
                  v-model="state.form[val.prop]"
                  v-bind="val.comProps"
                  :options="val.options"
                  optionLabel="label"
                  optionValue="value"
                  :placeholder="val.placeholder"
                  v-else-if="val.type === 'select'"
                  @change="val.change"
                  class="w-full"
                  filter
                  showClear
              />
              <CascadeSelect
                  v-else-if="val.type === 'cascader' && val.cascaderData"
                  :options="val.cascaderData"
                  optionGroupLabel="label"
                  v-bind="val.comProps"
                  :placeholder="val.placeholder"
                  @change="val.change"
                  class="w-full"
                  v-model="state.form[val.prop]"
              />
            </div>
          </template>
        </div>
        <div :class="getBtnColClass" class="mb20">
          <div class="table-form-btn">
            <label class="block mb-1">&nbsp;</label>
            <div>
              <div v-if="search.length > 3" class="table-form-btn-toggle mb10" @click="state.isToggle = !state.isToggle">
                <span>{{
                    state.isToggle ? t('message.common.button.collapse') : t('message.common.button.expand')
                  }}</span>
                <SvgIcon :name="state.isToggle ? 'fa-solid fa-chevron-up' : 'fa-solid fa-chevron-down'"/>
              </div>
              <Button @click="onSearch">
                <i :class="faMagnifyingGlass" style="margin-right: 5px"></i>
                {{ t('message.common.button.search') }}
              </Button>
              <Button severity="secondary" @click="onReset" style="margin-left: 12px;">
                <i :class="faArrowsRotate" style="margin-right: 5px"></i>
                {{ t('message.common.button.reset') }}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts" name="makeTableDemoSearch">
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import DatePicker from 'primevue/datepicker';
import Select from 'primevue/select';
import CascadeSelect from 'primevue/cascadeselect';
import { faArrowsRotate, faMagnifyingGlass } from '../../utils/icons';
import {reactive, ref, onMounted, watch, computed} from 'vue';


const props = defineProps({
  search: {
    type: Array<TableSearchType>,
    default: () => [],
  },
  reset: {
    type: Object,
    default: () => {
    },
  },
  param: {
    type: Object,
    default: () => {
    },
  },
});

const emit = defineEmits(['search', 'reset']);

const tableSearchRef = ref<HTMLFormElement>();
const state = reactive({
  form: {} as any,
  isToggle: false,
});

const getColClass = computed(() => {
  return 'col-12 sm:col-5 md:col-5 lg:col-6 xl:col-4';
});

const getBtnColClass = computed(() => {
  return 'col-12 sm:col-9 md:col-9 lg:col-6 xl:col-4';
});

watch(
    () => props.param,
    (value) => {
      if (value) {
        state.form = Object.assign({}, {...value});
      }
    },
    {
      immediate: true,
      deep: true,
    }
);

const onSearch = () => {
  emit('search', state.form);
};

const onReset = () => {
  Object.keys(state.form).forEach((key) => {
    if (props.param && props.param[key] !== undefined) {
      state.form[key] = props.param[key];
    } else {
      state.form[key] = undefined;
    }
  });
  emit('reset', state.form);
  emit('search', state.form);
};

onMounted(() => {
});
</script>

<style scoped lang="scss">
.table-search-container {
  display: flex;

  .table-form {
    flex: 1;

    .table-form-btn-toggle {
      white-space: nowrap;
      user-select: none;
      display: flex;
      align-items: center;
      color: var(--p-primary-color);
      cursor: pointer;
    }
  }
}
</style>
