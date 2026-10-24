import { describe, expect, it, vi } from 'vitest';
import InputNumber from 'primevue/inputnumber';

import './primevue-inputnumber-focus-guard';

type FocusContext = {
  focused: boolean;
  disabled?: boolean;
  readonly?: boolean;
  highlightOnFocus?: boolean;
  $refs: { input?: { $el?: HTMLInputElement } };
  $emit: (event: string, payload: unknown) => void;
};

// PrimeVue 组件以 options 对象导出，methods 上取回兜底后的处理器（结构性断言，取自官方组件导出）
const componentOptions = InputNumber as unknown as { methods: Record<string, unknown> };
const onInputFocus = componentOptions.methods.onInputFocus as (this: FocusContext, event: FocusEvent) => void;

function callOnInputFocus(context: FocusContext) {
  const inputElement = document.createElement('input');
  const event = { target: inputElement } as unknown as FocusEvent;
  onInputFocus.call(context, event);
}

describe('primevue-inputnumber-focus-guard', () => {
  it('输入引用未就绪时不抛空引用，仍置焦点态并转发 focus 事件', () => {
    const context: FocusContext = {
      focused: false,
      highlightOnFocus: true,
      $refs: {},
      $emit: vi.fn(),
    };

    expect(() => callOnInputFocus(context)).not.toThrow();
    expect(context.focused).toBe(true);
    expect(context.$emit).toHaveBeenCalledWith('focus', expect.anything());
  });

  it('输入引用就绪且启用聚焦高亮时调用 select', () => {
    const inputElement = document.createElement('input');
    inputElement.value = '3';
    const selectSpy = vi.spyOn(inputElement, 'select');
    const context: FocusContext = {
      focused: false,
      highlightOnFocus: true,
      $refs: { input: { $el: inputElement } },
      $emit: vi.fn(),
    };

    onInputFocus.call(context, { target: inputElement } as unknown as FocusEvent);
    expect(selectSpy).toHaveBeenCalled();
  });
});
