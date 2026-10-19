// PLATFORM-驾驶舱（/dashboard/home，超管专属）数据编排：平台跨租户视角。
// 租户总数与套餐总数为真实接口（分页 total）；应用总数与到期预警为 mock，
// 待后端平台聚合接口（TODO(HOME-AGG GameFrameX.Admin.Unified.Api#209)）。
import {onMounted, reactive} from 'vue';
import {systemTenantPagePost} from '/@/api/systemTenant';
import {globalPackagePagePost} from '/@/api/globalPackage';

export interface HomeNoticeItem {
  id: string;
  title: string;
  publicTime: number | null;
}

// TODO(HOME-AGG GameFrameX.Admin.Unified.Api#209): 平台级应用总数与租户到期预警暂为 mock
const MOCK_PLATFORM_KPIS = {
  apps: 36,
  expiringTenants: 2,
};

export function usePlatformHomeData() {
  const state = reactive({
    tenants: 0,
    tenantsLoaded: false,
    packages: 0,
    packagesLoaded: false,
    // 以下为 mock（见文件头 TODO）
    apps: MOCK_PLATFORM_KPIS.apps,
    expiringTenants: MOCK_PLATFORM_KPIS.expiringTenants,
  });

  const loadTenants = async () => {
    try {
      const res = await systemTenantPagePost({page: 1, pageSize: 1});
      state.tenants = res.data.data?.total ?? 0;
    } catch {
      // 平台接口失败保持 0，显示 '—' 由 onlineFailed 类似逻辑处理（这里直接显示 0 亦可）
    } finally {
      state.tenantsLoaded = true;
    }
  };

  const loadPackages = async () => {
    try {
      const res = await globalPackagePagePost({page: 1, pageSize: 1});
      state.packages = res.data.data?.total ?? 0;
    } catch {
      // 同上，静默
    } finally {
      state.packagesLoaded = true;
    }
  };

  onMounted(() => {
    loadTenants();
    loadPackages();
  });

  return {state};
}
