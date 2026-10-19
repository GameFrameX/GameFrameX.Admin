// APP-驾驶舱（/dashboard/appHome）数据编排。
// KPI 与趋势走 #211（GFX-730）真实聚合接口：overview 为租户级聚合
// （应用维度拆分待 Unified.Api#209 后续）；onlineTrend 为最近 24 个整点桶，
// 无采样桶 OnlineCount 为 null（趋势图断点）。公告走 tenantNotice。
import {onMounted, reactive} from 'vue';
import {tenantGameHomeEntityOverviewPost, tenantGameHomeEntityOnlineTrendPost} from '/@/api/tenantGameHomeEntity';
import {tenantNoticePageReceivedGet} from '/@/api/tenantNotice';
import type {HomeNoticeItem} from '/@/views/tenant/home/composables/usePlatformHomeData';

// 通知列表行的 systemNotice 嵌套投影（生成类型未透出该嵌套，按接口实际返回声明）
interface NoticeRow {
  id: string;
  systemNotice?: {
    title?: string | null;
    publicTime?: number | null;
  };
}

export function useAppHomeData() {
  const state = reactive({
    overviewLoaded: false,
    overviewFailed: false,
    onlineCount: 0,
    todayOrders: 0,
    pendingShipments: 0,
    refunding: 0,
    playerTotal: 0,
    playerTodayNew: 0,
    areaTotal: 0,
    areaRunning: 0,
    areaMaintenance: 0,
    // 趋势：最近 24 个整点桶；labels 为对应整点文案，与数据同长
    trend: [] as Array<number | null>,
    trendLabels: [] as string[],
    notices: [] as HomeNoticeItem[],
  });

  const loadOverview = async () => {
    try {
      const res = await tenantGameHomeEntityOverviewPost();
      const d = res.data.data;
      if (d) {
        state.onlineCount = d.onlineCount ?? 0;
        state.todayOrders = d.todayOrderCount ?? 0;
        state.pendingShipments = d.pendingShipmentCount ?? 0;
        state.refunding = d.refundingCount ?? 0;
        state.playerTotal = d.playerTotalCount ?? 0;
        state.playerTodayNew = d.playerTodayNewCount ?? 0;
        state.areaTotal = d.areaTotalCount ?? 0;
        state.areaRunning = d.areaRunningCount ?? 0;
        state.areaMaintenance = d.areaMaintenanceCount ?? 0;
      }
    } catch {
      // 聚合接口失败静默降级为 —（部分上下文无租户业务数据）
      state.overviewFailed = true;
    } finally {
      state.overviewLoaded = true;
    }
  };

  const loadTrend = async () => {
    try {
      const res = await tenantGameHomeEntityOnlineTrendPost();
      const points = res.data.data?.points ?? [];
      state.trend = points.map((p) => (p.onlineCount == null ? null : Number(p.onlineCount)));
      state.trendLabels = points.map((p) => {
        const date = new Date(Number(p.bucketTime) * 1000);
        return `${String(date.getHours()).padStart(2, '0')}:00`;
      });
    } catch {
      // 趋势失败保持空数组（图表空态）
    }
  };

  const loadNotices = async () => {
    try {
      const res = await tenantNoticePageReceivedGet({page: 1, pageSize: 4});
      const rows = (res.data.data?.items ?? []) as unknown as NoticeRow[];
      state.notices = rows
          .filter((row) => row.systemNotice?.title)
          .map((row) => ({
            id: row.id,
            title: row.systemNotice?.title ?? '',
            publicTime: row.systemNotice?.publicTime ?? null,
          }));
    } catch {
      // 公告拉取失败保持空态，不阻断驾驶舱
    }
  };

  onMounted(() => {
    loadOverview();
    loadTrend();
    loadNotices();
  });

  return {state};
}
