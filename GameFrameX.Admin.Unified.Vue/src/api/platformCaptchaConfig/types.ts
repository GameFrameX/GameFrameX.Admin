/* eslint-disable */
/* tslint:disable */

/** 平台图形验证码配置输出。 */
export interface PlatformCaptchaConfigResponse {
  /** 是否启用登录图形验证码。 */
  isEnabled?: boolean;
  /**
   * 配置版本。
   * @format int64
   */
  version?: number;
  /**
   * 验证码类型。
   * @format int32
   */
  captchaType?: number;
  /**
   * 验证码长度。
   * @format int32
   */
  codeLength?: number;
  /**
   * 过期秒数。
   * @format int32
   */
  expirySeconds?: number;
  /** 是否忽略大小写。 */
  ignoreCase?: boolean;
  /** 平台图形验证码图像配置输入。 */
  imageOption?: PlatformCaptchaImageRequest;
  /**
   * 最近更新时间。
   * @format int64
   */
  updateTime?: number | null;
}

/** 平台图形验证码配置更新输入。 */
export interface PlatformCaptchaConfigUpdateRequest {
  /** 是否启用登录图形验证码。 */
  isEnabled?: boolean;
  /**
   * 验证码类型。
   * @format int32
   * @min 0
   * @max 11
   */
  captchaType?: number;
  /**
   * 验证码长度。
   * @format int32
   * @min 1
   * @max 8
   */
  codeLength?: number;
  /**
   * 过期秒数。
   * @format int32
   * @min 10
   * @max 600
   */
  expirySeconds?: number;
  /** 是否忽略大小写。 */
  ignoreCase?: boolean;
  /** 平台图形验证码图像配置输入。 */
  imageOption: PlatformCaptchaImageRequest;
}

/** 平台图形验证码图像配置输入。 */
export interface PlatformCaptchaImageRequest {
  /** 是否启用 GIF 动画。 */
  animation?: boolean;
  /**
   * 字号。
   * @format float
   * @min 12
   * @max 96
   */
  fontSize?: number;
  /**
   * 宽度。
   * @format int32
   * @min 80
   * @max 400
   */
  width?: number;
  /**
   * 高度。
   * @format int32
   * @min 30
   * @max 180
   */
  height?: number;
  /**
   * 气泡最小半径。
   * @format int32
   * @min 0
   * @max 40
   */
  bubbleMinRadius?: number;
  /**
   * 气泡最大半径。
   * @format int32
   * @min 0
   * @max 60
   */
  bubbleMaxRadius?: number;
  /**
   * 气泡数量。
   * @format int32
   * @min 0
   * @max 30
   */
  bubbleCount?: number;
  /**
   * 气泡线条厚度。
   * @format float
   * @min 0
   * @max 10
   */
  bubbleThickness?: number;
  /**
   * 干扰线数量。
   * @format int32
   * @min 0
   * @max 30
   */
  interferenceLineCount?: number;
  /**
   * 字体名称。
   * @maxLength 256
   */
  fontFamily?: string | null;
  /**
   * 动画帧延迟。
   * @format int32
   * @min 50
   * @max 2000
   */
  frameDelay?: number;
  /**
   * 背景颜色。
   * @maxLength 256
   */
  backgroundColor?: string | null;
  /**
   * 前景颜色，多个颜色用逗号分隔。
   * @maxLength 256
   */
  foregroundColors?: string | null;
  /**
   * 图片质量。
   * @format int32
   * @min 1
   * @max 100
   */
  quality?: number;
  /** 是否粗体。 */
  textBold?: boolean;
}

export interface PlatformCaptchaConfigDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台图形验证码配置输出。 */
  data: PlatformCaptchaConfigResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}

export type PlatformCaptchaConfigUpdatePostData = any;
