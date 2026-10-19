<template>
  <div class="reward-items-editor">
    <div v-for="(item, index) in rows" :key="index" class="reward-items-editor-row">
      <Select filter v-model="item._catalogueId" :options="catalogueOptions" optionLabel="value" optionValue="id"
              :placeholder="t('message.gameBusiness.goods.pleaseSelectCatalogue')" showClear class="catalogue-select"
              :disabled="disabled || !appId" @change="onCatalogueChange(item)"/>
      <Select v-model="item.id" :options="getFilteredGoods(item._catalogueId)" optionLabel="label" optionValue="id"
              :placeholder="rewardPlaceholder" filter class="goods-select"
              :disabled="disabled || !appId" @change="onGoodsChange(item)"/>
      <Button type="button" severity="danger" outlined class="remove-button" :aria-label="t('message.common.button.delete')"
              :disabled="disabled" @click="removeItem(index)">
        <component :is="faMinus"/>
      </Button>
    </div>
    <Button type="button" class="add-button" :disabled="disabled || !appId" @click="addItem">
      <component :is="faPlus" class="mr-1"/>{{ addLabel }}
    </Button>
  </div>
</template>

<script setup lang="ts">
import {computed, ref, watch} from 'vue'
import {faMinus, faPlus} from '../../utils/icons'
import { tenantGameGoodsEntityPagePost } from '../../api/tenantGameGoodsEntity'
import { tenantGameGoodsCatalogueEntitySelectPost } from '../../api/tenantGameGoodsCatalogueEntity'
import {MAX_SELECT_SIZE} from '../../constants/pagination'
import {normalizeAppId, type AppWorkspaceId} from '../../utils/appWorkspace'
import type {RewardItem} from './rewardItems'

const props = withDefaults(defineProps<{
  appId?: number | string
  disabled?: boolean
  rewardPlaceholder?: string
  addLabel?: string
}>(), {
  disabled: false,
  rewardPlaceholder: () => t('message.gameBusiness.rewardTemplate.pleaseSelectRewardItem'),
  addLabel: () => t('message.gameBusiness.rewardTemplate.addRewardItem'),
})

const model = defineModel<RewardItem[]>({default: () => []})
const goodsOptions = ref<any[]>([])
const catalogueOptions = ref<any[]>([])

const appId = computed(() => normalizeAppId(props.appId))

const rows = computed(() => Array.isArray(model.value) ? model.value : [])

const ensureRowDefaults = (item: RewardItem) => {
  if (item.count === undefined) item.count = 1
  if (item.star === undefined) item.star = 0
}

const loadOptions = async (scopedAppId?: AppWorkspaceId) => {
  if (!scopedAppId) {
    goodsOptions.value = []
    catalogueOptions.value = []
    return
  }
  // 物品选项走 page 接口：selectString 按基类声明类型序列化会裁剪派生字段 classifyId（后端问题 26），
  // 而目录过滤依赖 classifyId，故改用含完整字段的 page 响应映射 {id, label: name, classifyId}。
  const [goodsRes, catalogueRes] = await Promise.all([
    tenantGameGoodsEntityPagePost({page: 1, pageSize: MAX_SELECT_SIZE, appId: scopedAppId}),
    tenantGameGoodsCatalogueEntitySelectPost({appId: scopedAppId, pageSize: MAX_SELECT_SIZE}),
  ])
  goodsOptions.value = (goodsRes.data.data?.items ?? []).map((item: any) => ({
    id: item.id,
    label: item.name,
    classifyId: item.classifyId,
  }))
  catalogueOptions.value = catalogueRes.data.data ?? []
}

const getFilteredGoods = (catalogueId: number | string | undefined) => {
  if (!catalogueId) return goodsOptions.value
  return goodsOptions.value.filter((item: any) => item.classifyId == catalogueId)
}

const onCatalogueChange = (item: RewardItem) => {
  item.id = undefined
  item.itemId = 0
}

const onGoodsChange = (item: RewardItem) => {
  item.itemId = 0
}

const addItem = () => {
  model.value = [...rows.value, {count: 1, star: 0}]
}

const removeItem = (index: number) => {
  model.value = rows.value.filter((_, itemIndex) => itemIndex !== index)
}

watch(appId, (value) => {
  loadOptions(value)
}, {immediate: true})

watch(model, (value) => {
  if (!Array.isArray(value)) {
    model.value = []
    return
  }
  value.forEach(ensureRowDefaults)
}, {deep: true, immediate: true})
</script>

<style scoped lang="scss">
.reward-items-editor {
  display: grid;
  gap: 10px;
  min-width: 0;
  width: 100%;
}

.reward-items-editor-row {
  display: grid;
  grid-template-columns: minmax(96px, 160px) minmax(0, 1fr) 44px;
  gap: 8px;
  align-items: center;
  min-width: 0;
}

.catalogue-select,
.goods-select,
.add-button {
  width: 100%;
  min-width: 0;
}

.remove-button {
  width: 44px;
  height: 44px;
  padding: 0;
}

.add-button {
  justify-self: start;
  max-width: 220px;
}

@include bp-down(sm) {
  .reward-items-editor-row {
    grid-template-columns: minmax(84px, 120px) minmax(0, 1fr) 44px;
  }

  .add-button {
    width: 100%;
    max-width: none;
  }
}
</style>
