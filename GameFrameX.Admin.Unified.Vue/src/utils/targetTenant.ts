import {local} from './storage';

export const TARGET_TENANT_ID_HEADER = 'GameFrameX-Target-Tenant-Id';
export const TARGET_TENANT_ID_KEY = 'target-tenant-id';

export function getTargetTenantId(): string {
    const value = local.get(TARGET_TENANT_ID_KEY);
    return typeof value === 'string' ? value : '';
}

export function setTargetTenantId(value: string): void {
    local.set(TARGET_TENANT_ID_KEY, value);
}

export function clearTargetTenantId(): void {
    local.remove(TARGET_TENANT_ID_KEY);
}

export function normalizeTargetTenantId(value: string): string {
    const tenantId = value.trim();
    if (!/^\d+$/.test(tenantId)) {
        return '';
    }

    const parsed = Number(tenantId);
    return Number.isSafeInteger(parsed) && parsed > 0 ? tenantId : '';
}
