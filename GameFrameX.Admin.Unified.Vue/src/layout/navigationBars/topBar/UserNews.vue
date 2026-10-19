<template>
  <div class="user-news-container">
    <Tabs value="0">
      <TabList>
        <Tab value="0">
          <i class="fa-solid fa-bell" style="font-size: 14px"></i>
          <span style="margin-left: 5px">{{ t('message.layout.userNews.siteMessage') }}</span>
        </Tab>
        <Tab value="1">
          <i class="fa-solid fa-location-dot" style="font-size: 14px"></i>
          <span style="margin-left: 5px">{{ t('message.layout.userNews.mine') }}</span>
        </Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <div class="notice-box">
            <template v-if="noticeList.length > 0">
              <div class="notice-item" v-for="(v, k) in noticeList" :key="k"
                   role="button" :tabindex="0"
                   :aria-label="`${t('message.layout.userNews.noticeTag')}${v.title}`"
                   @click="viewNoticeDetail(v)"
                   @keydown.enter.prevent="viewNoticeDetail(v)"
                   @keydown.space.prevent="viewNoticeDetail(v)"
                   v-show="v.readStatus == 1 ? false : true">
                <div class="notice-title">{{
                    v.type == 1 ? t('message.layout.userNews.noticeTag') : t('message.layout.userNews.announcementTag')
                  }}{{ v.title }}
                </div>
                <div class="notice-content">{{ removeHtmlSub(v.content) }}</div>
                <div class="notice-time">{{ v.publicTime }}</div>
                <Divider style="margin: 10px 0"/>
              </div>
            </template>
            <div class="empty-state" v-else>
              <i class="fa-solid fa-inbox" style="font-size: 40px; opacity: 0.3"></i>
              <p style="color: var(--p-text-muted-color); margin-top: 10px">{{ t('message.layout.userNews.empty') }}</p>
            </div>
          </div>
          <div class="notice-foot" @click="goToNotice" v-if="noticeList.length > 0"
               role="button" :tabindex="0"
               @keydown.enter.prevent="goToNotice"
               @keydown.space.prevent="goToNotice">
            {{ t('message.layout.userNews.goToNoticeCenter') }}
          </div>
        </TabPanel>
        <TabPanel value="1">
          <div style="height: 400px; overflow-y: auto; padding-right: 10px">
            <div class="empty-state">
              <i class="fa-solid fa-inbox" style="font-size: 40px; opacity: 0.3"></i>
              <p style="color: var(--p-text-muted-color); margin-top: 10px">{{ t('message.layout.userNews.empty') }}</p>
            </div>
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
    <Dialog v-model:visible="state.dialogVisible" modal :header="t('message.layout.userNews.messageDetail')"
            :style="{ width: 'min(769px, calc(100vw - 32px))' }">
      <p v-html="sanitizeHtml(state.content)"></p>
      <template #footer>
        <Button @click="state.dialogVisible = false">{{ t('message.layout.userNews.confirm') }}</Button>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="layoutBreadcrumbUserNews">
import {reactive} from 'vue';
import router from '../../../router';
import commonFunction from '../../../utils/commonFunction';

import {tenantNoticeSetReadPost} from '../../../api/tenantNotice';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Divider from 'primevue/divider';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import {sanitizeHtml} from '../../../utils/sanitize';

defineProps({
  noticeList: Array as any,
});
const {removeHtmlSub} = commonFunction();
const state = reactive({
  dialogVisible: false,
  content: '',
});
// 前往通知中心点击
const goToNotice = () => {
  router.push('/tenant/dashboard/notice');
};
// 查看消息详情
const viewNoticeDetail = async (notice: any) => {
  state.content = notice.content;
  state.dialogVisible = true;

  // 设置已读
  notice.readStatus = 1;
  await tenantNoticeSetReadPost({id: notice.id});
};
</script>

<style scoped lang="scss">
.user-news-container {
  .notice-box {
    height: 400px;
    padding-right: 10px;
    margin-bottom: 35px;

    &:hover {
      overflow-y: scroll;
    }
  }

  .notice-item {
    cursor: pointer;

    &:focus-visible {
      outline: 2px solid var(--p-primary-color);
      outline-offset: -2px;
    }

    &:hover {
      background-color: var(--p-surface-200);
    }

    // 按压反馈（GFX-661）；surface 阶梯比 hover 深一档，亮暗主题方向一致
    &:active {
      background-color: var(--p-surface-300);
    }

    .notice-content {
      color: var(--p-text-muted-color);
      margin-top: 3px;
      margin-bottom: 3px;
    }

    .notice-time {
      color: var(--p-text-muted-color);
      text-align: right;
    }
  }

  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 0;
  }

  .notice-foot {
    height: 35px;
    width: 100%;
    color: var(--p-primary-color);
    font-size: 14px;
    cursor: pointer;
    position: absolute;
    bottom: 0px;
    background-color: var(--p-content-background);
    display: flex;
    align-items: center;
    justify-content: center;

    &:focus-visible {
      outline: 2px solid var(--p-primary-color);
      outline-offset: -2px;
    }

    // 按压反馈（GFX-661）
    &:active {
      background-color: var(--p-primary-100);
    }
  }
}
</style>
