<template>
  <!-- 安全验证滑块：无障碍友好的标准形态（role="slider" + 键盘档位步进），替代原旋转拖拽弹窗。
       棘轮档位：0-4 共 5 档，松手保留进度（不清零）；到 4 档即通过并盖「已验证」章。 -->
  <div class="slide-verify" :class="{'sv-passed': state.passed, 'sv-error': invalid}">
    <div
        ref="trackRef"
        class="sv-track"
        role="slider"
        :tabindex="state.passed ? -1 : 0"
        :aria-label="t('message.login.slideVerify.label')"
        :aria-describedby="describedBy"
        :aria-valuemin="0"
        :aria-valuemax="DETENTS"
        :aria-valuenow="state.pos"
        :aria-valuetext="ariaText"
        :aria-disabled="state.passed"
        @keydown="onKeydown"
        @pointerdown="onPointerDown"
        @pointermove="onPointerMove"
        @pointerup="onPointerUp"
        @pointercancel="onPointerUp"
    >
      <div class="sv-fill" :style="{width: fillWidth}"></div>
      <span v-if="!state.passed" class="sv-hint">{{ t('message.login.slideVerify.hint') }}</span>
      <span v-else class="sv-stamp">
        <i class="pi pi-check" aria-hidden="true"></i>
        {{ t('message.login.slideVerify.passed') }}
      </span>
      <div class="sv-thumb" :style="{left: thumbLeft}" aria-hidden="true">
        <i :class="state.passed ? 'pi pi-check' : 'pi pi-chevron-right'"></i>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts" name="loginSlideVerify">
import {computed, reactive, ref} from 'vue';
import {t} from '../../../i18n/index';

const DETENTS = 4;

// aria 关联必须落在 role="slider" 的滑轨上（透传会落到根包裹 div，读屏关联失效）
defineProps<{
  invalid?: boolean;
  describedBy?: string;
}>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void; (e: 'passed'): void }>();

const trackRef = ref<HTMLElement | null>(null);
const state = reactive({
  pos: 0,
  passed: false,
  dragging: false,
});

const fillWidth = computed(() => `calc(22px + (100% - 44px) * ${state.pos / DETENTS})`);
const thumbLeft = computed(() => `calc((100% - 44px) * ${state.pos / DETENTS})`);
const ariaText = computed(() => {
  if (state.passed) {
    return t('message.login.slideVerify.passed');
  }
  if (state.pos === 0) {
    return t('message.login.slideVerify.label');
  }
  return `${state.pos}/${DETENTS}`;
});

const setPos = (pos: number) => {
  const clamped = Math.max(0, Math.min(DETENTS, Math.round(pos)));
  if (clamped === state.pos) {
    return;
  }
  state.pos = clamped;
  if (state.pos === DETENTS) {
    pass();
  }
};

const pass = () => {
  if (state.passed) {
    return;
  }
  state.passed = true;
  emit('update:modelValue', true);
  emit('passed');
};

const posFromClientX = (clientX: number) => {
  const track = trackRef.value;
  if (!track) {
    return 0;
  }
  const rect = track.getBoundingClientRect();
  const raw = (clientX - rect.left - 22) / Math.max(1, rect.width - 44);
  return raw * DETENTS;
};

const onPointerDown = (event: PointerEvent) => {
  if (state.passed) {
    return;
  }
  state.dragging = true;
  trackRef.value?.setPointerCapture(event.pointerId);
  setPos(posFromClientX(event.clientX));
};

const onPointerMove = (event: PointerEvent) => {
  if (!state.dragging || state.passed) {
    return;
  }
  setPos(posFromClientX(event.clientX));
};

const onPointerUp = (event: PointerEvent) => {
  if (!state.dragging) {
    return;
  }
  state.dragging = false;
  if (trackRef.value?.hasPointerCapture(event.pointerId)) {
    trackRef.value.releasePointerCapture(event.pointerId);
  }
  // 棘轮档位：松手停在最近档位，进度保留，不清零
};

const onKeydown = (event: KeyboardEvent) => {
  if (state.passed) {
    return;
  }
  switch (event.key) {
    case 'ArrowRight':
    case 'ArrowUp':
      event.preventDefault();
      setPos(state.pos + 1);
      break;
    case 'ArrowLeft':
    case 'ArrowDown':
      event.preventDefault();
      setPos(state.pos - 1);
      break;
    case 'Home':
      event.preventDefault();
      setPos(0);
      break;
    case 'End':
      event.preventDefault();
      setPos(DETENTS);
      break;
    default:
      break;
  }
};

// pointermove/up 经 setPointerCapture 后仍派发到 track 元素，模板事件即可收到

const reset = () => {
  state.pos = 0;
  state.passed = false;
  emit('update:modelValue', false);
};

const focus = () => {
  trackRef.value?.focus();
};

defineExpose({reset, focus});
</script>

<style scoped lang="scss">
.slide-verify {
  .sv-track {
    position: relative;
    height: 46px;
    border: 1px solid var(--p-inputtext-border-color, var(--p-content-border-color));
    border-radius: 8px;
    background: var(--p-surface-0);
    overflow: hidden;
    cursor: pointer;
    user-select: none;
    touch-action: none;

    &:focus-visible {
      outline: 2px solid var(--p-primary-300);
      outline-offset: 2px;
    }
  }

  .sv-fill {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    background: var(--p-primary-50);
    // 棘轮档位：填充条瞬时贴位，不做宽度过渡（拖拽跟手；全页动效只留盖章时刻）
  }

  .sv-hint {
    position: absolute;
    left: 60px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 13px;
    color: var(--p-text-muted-color);
    pointer-events: none;
    white-space: nowrap;
  }

  .sv-thumb {
    position: absolute;
    top: 2px;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--p-surface-0);
    border: 1px solid var(--p-primary-300);
    border-radius: 6px;
    color: var(--p-primary-color);
    transition: left 0.12s ease-out;

    i {
      font-size: 14px;
    }
  }

  // 「已验证」章：本页唯一的动效时刻——压印落定
  .sv-stamp {
    position: absolute;
    left: 50%;
    top: 50%;
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 3px 12px;
    border: 1.5px solid var(--p-primary-color);
    border-radius: 4px;
    color: var(--p-primary-color);
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 0.08em;
    background: var(--p-surface-0);
    transform: translate(-50%, -50%) rotate(-4deg);
    animation: sv-stamp-in 0.22s cubic-bezier(0.2, 0.9, 0.3, 1);
    pointer-events: none;
  }

  .sv-passed {
    .sv-fill {
      background: var(--p-primary-100);
    }

    .sv-thumb {
      border-color: var(--p-primary-color);
      background: var(--p-primary-color);
      color: var(--p-surface-0);
      cursor: default;
    }
  }

  .sv-error {
    .sv-track {
      border-color: var(--p-red-400, #f87171);
    }
  }

  @keyframes sv-stamp-in {
    from {
      opacity: 0;
      transform: translate(-50%, -50%) rotate(-4deg) scale(1.45);
    }

    to {
      opacity: 1;
      transform: translate(-50%, -50%) rotate(-4deg) scale(1);
    }
  }
}
</style>
