// PLATFORM-驾驶舱（/dashboard/home，超管专属）数据编排：平台跨租户视角。
// 租户总数与套餐总数为真实接口（分页 total）；应用总数与到期预警为 mock，
// 待后端平台聚合接口（TODO(HOME-AGG GameFrameX.Admin.Unified.Api#209)）。
import {onMounted, reactive} from 'vue';

// ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
// systemTenant / globalPackage 两接口已删：租户数与套餐数恒为 0，不发请求、不伪造数据。

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
    // ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
    state.tenants = 0;
    state.tenantsLoaded = true;
  };

  const loadPackages = async () => {
    // ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
    state.packages = 0;
    state.packagesLoaded = true;
  };

  onMounted(() => {
    loadTenants();
    loadPackages();
  });

  return {state};
}
