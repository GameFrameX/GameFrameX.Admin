import type { MenuItem } from 'primevue/menuitem';

export interface NavMenuItem extends MenuItem {
    key: string;
    route?: string;
}

export function convertRoutesToMenuItems(
    routes: RouteItems,
    t: (key: string) => string,
): NavMenuItem[] {
    return routes
        .filter((route) => !route.meta?.isHide)
        .map((route) => {
            const label = t(route.meta?.titleCode || route.meta?.title || '');
            const icon = route.meta?.icon;
            const key = route.path;

            if (route.children && route.children.length > 0) {
                return {
                    key,
                    label,
                    icon,
                    items: convertRoutesToMenuItems(route.children, t),
                } as NavMenuItem;
            }

            const isExternalLink = route.meta?.isLink && !route.meta?.isIframe;

            if (isExternalLink) {
                return {
                    key,
                    label,
                    icon,
                    url: route.meta!.isLink,
                    target: '_blank',
                } as NavMenuItem;
            }

            return {
                key,
                label,
                icon,
                route: route.path,
            } as NavMenuItem;
        });
}

export function computeExpandedKeys(items: NavMenuItem[], targetPath: string): Record<string, boolean> {
    const keys: Record<string, boolean> = {};

    function walk(list: NavMenuItem[]): boolean {
        for (const item of list) {
            if (item.items?.length) {
                if (walk(item.items as NavMenuItem[])) {
                    keys[item.key] = true;
                    return true;
                }
            }
            if (item.route === targetPath || item.key === targetPath) {
                return true;
            }
        }
        return false;
    }

    walk(items);
    return keys;
}
