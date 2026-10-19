import type {Directive} from 'vue';

const STYLE_ID = 'v-block-loading-styles';

function injectStyles() {
  if (document.getElementById(STYLE_ID)) return;
  const style = document.createElement('style');
  style.id = STYLE_ID;
  style.textContent = `
    @keyframes v-block-loading-spin { to { transform: rotate(360deg); } }
    .v-block-loading-overlay {
      display: none;
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: color-mix(in srgb, var(--p-content-background) 70%, transparent);
      z-index: var(--z-index-block);
      justify-content: center;
      align-items: center;
      pointer-events: all;
    }
    .v-block-loading-spinner {
      width: 32px; height: 32px;
      border: 3px solid var(--p-surface-300);
      border-top-color: var(--p-primary-color);
      border-radius: 50%;
      animation: v-block-loading-spin 0.8s linear infinite;
    }
    .dark .v-block-loading-overlay { background: color-mix(in srgb, var(--p-content-background) 65%, transparent); }
    .dark .v-block-loading-spinner { border-color: var(--p-surface-700); border-top-color: var(--p-primary-color); }
  `;
  document.head.appendChild(style);
}

export const blockLoading: Directive = {
  mounted(el: HTMLElement, binding) {
    injectStyles();
    const overlay = document.createElement('div');
    overlay.className = 'v-block-loading-overlay';
    overlay.innerHTML = '<div class="v-block-loading-spinner"></div>';
    el.style.position = el.style.position || 'relative';
    el.appendChild(overlay);
    if (binding.value) {
      (overlay as HTMLElement).style.display = 'flex';
    }
  },
  updated(el: HTMLElement, binding) {
    const overlay = el.querySelector('.v-block-loading-overlay') as HTMLElement | null;
    if (overlay) {
      overlay.style.display = binding.value ? 'flex' : 'none';
    }
  },
  unmounted(el: HTMLElement) {
    const overlay = el.querySelector('.v-block-loading-overlay');
    if (overlay) overlay.remove();
  },
};
