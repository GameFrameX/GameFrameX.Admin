<template>
  <Drawer v-model:visible="isShowDrawer" :header="t('message.gameBusiness.mail.mailDetail')">
    <div style="margin-left: 20px">
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.title') }}: </label>
        <b>{{ formInfo.title }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.subTitle') }}: </label>
        <b>{{ formInfo.subTitle }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.body') }}: </label>
        <b>{{ formInfo.body }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.areaId') }}: </label>
        <b>{{ getAreaName(formInfo.areaId) }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.expirationDate') }}: </label>
        {{ formatUnix(formInfo.expirationTime) }}
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.mailType') }}: </label>
        <b>{{ getEnumDesc(formInfo.mailType, getEnumMailTypeData_Index) }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.sendToAll') }}: </label>
        <Tag :value="t('message.gameBusiness.yes')" v-if="formInfo.isAllPost"/>
        <Tag :value="t('message.gameBusiness.no')" severity="danger" v-else/>
      </div>
      <div class="mb10" v-if="!formInfo.isAllPost">
        <label class="font-bold">{{ t('message.gameBusiness.mail.playerIdList') }}: </label>
        <b>{{ formInfo.playerIdListJson }}</b>
      </div>
      <div class="mb10">
        <label class="font-bold">{{ t('message.gameBusiness.mail.attachmentList') }}:</label>
      </div>
      <div v-for="x in formInfo.attachmentsJson" :key="x.id" style="margin-left: 40px; margin-bottom: 10px;">
        {{ t('message.gameBusiness.mail.propItem') }}:
        <Tag :value="getItemName(x.id)"/>
        {{ t('message.gameBusiness.mail.propCount') }}:
        <Tag :value="x.count"/>
        <template v-if="x.star != 0">{{ t('message.gameBusiness.goods.starLevel') }}:
          <Tag :value="x.star"/>
        </template>
      </div>
    </div>
    <template #footer>
      <Button severity="secondary" @click="cancel">{{ t('message.common.button.close') }}</Button>
    </template>
  </Drawer>
</template>
<style scoped>
.attachment-item {
  width: 100%;
  margin-bottom: 10px;
}
</style>
<script lang="ts" setup>
import {ref, onMounted} from "vue";

import {systemEnumEnumDataListGet} from '../../../../../../api/systemEnum';
import { tenantGameAreaEntitySelectStringPost } from "../../../../../../api/tenantGameAreaEntity";
import { tenantGameGoodsEntitySelectStringPost } from "../../../../../../api/tenantGameGoodsEntity";
import commonFunction from "../../../../../../utils/commonFunction.ts";
import {formatUnix} from "../../../../../../utils/formatTime.ts";
import {normalizeAppId} from "../../../../../../utils/appWorkspace.ts";
import {parseRewardItems} from "../../../../../../components/reward-items-editor/rewardItems.ts";
import {MAX_SELECT_SIZE} from '../../../../../../constants/pagination.ts';

const {getEnumDesc} = commonFunction();
// eslint-disable-next-line @typescript-eslint/naming-convention -- TODO(命名治理): 存量 PascalCase 命名（枚举映射表/C# 风格函数式命名），待配套治理 issue 改名
const getEnumMailTypeData_Index = ref<any>([]);
const getGoodsList = ref<any>([]);
const formInfo = ref<any>({});
// Area list cache
const areaList = ref<any>([]);
const isShowDrawer = ref(false);
const isEditor = ref(false);

// Open dialog
const openDialog = async (row: any) => {
  formInfo.value = row;
  formInfo.value.attachmentsJson = parseRewardItems(formInfo.value.attachmentsJson);

  const appId = resolveAppId(formInfo.value);
  await Promise.all([
    getAreaList(appId),
    loadGoodsList(appId),
  ]);
  isShowDrawer.value = true;
};

// Get area name
const getAreaName = (areaId: number) => {
  for (let i = 0; i < areaList.value.length; i++) {
    const item = areaList.value[i];
    if (item.id == areaId) {
      return item.value;
    }
  }
  return areaId;
};
// Get item name
const getItemName = (id: number) => {
  var goodItem = getGoodsList.value.filter((x: any) => x.id == id);
  if (goodItem.length > 0) {
    return goodItem[0].value
  }

}
const resolveAppId = (row: any) => {
  return normalizeAppId(row?.appId)
}

const getAreaList = async (appId?: number) => {
  const res = await tenantGameAreaEntitySelectStringPost({appId, pageSize: MAX_SELECT_SIZE});
  areaList.value = res.data.data ?? [];
};

const loadGoodsList = async (appId?: number) => {
  getGoodsList.value = (await tenantGameGoodsEntitySelectStringPost({appId, pageSize: MAX_SELECT_SIZE})).data.data ?? [];
}

// Cancel
const cancel = () => {
  isShowDrawer.value = false;
};

// On mounted
onMounted(async () => {
  getEnumMailTypeData_Index.value =
      (await systemEnumEnumDataListGet({ EnumName: "GameMailType" })).data
          .data ?? [];
});

// Expose to parent
defineExpose({openDialog, isEditor});
</script>
