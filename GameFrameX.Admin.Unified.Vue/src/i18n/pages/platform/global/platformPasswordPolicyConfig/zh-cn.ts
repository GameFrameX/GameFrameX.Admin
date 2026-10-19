export default {
    platformPasswordPolicyConfig: {
        title: '密码策略配置',
        saveHint: '开启后，用户修改密码时将按下方正则校验新密码强度；关闭时不强制校验。本页仅维护密码强度策略，不涉及任何加解密密钥。',
        strongPassword: '启用密码强度校验',
        passwordStrengthValidation: '密码强度正则',
        passwordStrengthValidationHint: 'JavaScript 正则表达式，启用后用于校验新密码',
        passwordStrengthValidationMsg: '校验失败提示',
        updateTime: '最近更新',
        saveSuccess: '密码策略配置已保存',
        validationEmpty: '密码强度正则不能为空',
        validationInvalid: '密码强度正则无效或无法编译',
        validationMessageInvalid: '校验失败提示不能为空',
        loadFailed: '加载密码策略配置失败',
    },
};
