import InputNumber from 'primevue/inputnumber';

/**
 * PrimeVue 4.5 InputNumber 的 onInputFocus 会读取 <c>$refs.input.$el.value</c>：
 * 输入引用尚未挂载的竞态窗口内该引用为 null，直接抛空引用异常并中断后续组件渲染
 * （且 $el 求值先于 highlightOnFocus 短路，无法用配置规避）。
 * 这里在应用启动时对组件方法做同一时序兜底：输入引用未就绪时跳过高亮选择，仅保留 focus 状态与事件转发。
 */
type InputNumberFocusContext = {
  focused: boolean;
  disabled?: boolean;
  readonly?: boolean;
  highlightOnFocus?: boolean;
  $refs: { input?: { $el?: HTMLInputElement } };
  $emit: (event: string, payload: unknown) => void;
};

const componentOptions = InputNumber as unknown as {
  methods?: Record<string, unknown>;
};

const originalOnInputFocus = componentOptions.methods?.onInputFocus as ((this: InputNumberFocusContext, event: FocusEvent) => void) | undefined;
if (typeof originalOnInputFocus === 'function') {
  componentOptions.methods!.onInputFocus = function (this: InputNumberFocusContext, event: FocusEvent) {
    this.focused = true;
    const inputElement = this.$refs?.input?.$el;
    if (
      inputElement &&
      !this.disabled &&
      !this.readonly &&
      this.highlightOnFocus &&
      inputElement.value !== (document.getSelection()?.toString() ?? '')
    ) {
      (event.target as HTMLInputElement).select();
    }

    this.$emit('focus', event);
  };
}
