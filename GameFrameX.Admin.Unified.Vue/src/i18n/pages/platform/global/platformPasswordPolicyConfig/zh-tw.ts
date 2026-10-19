export default {
    platformPasswordPolicyConfig: {
        title: '密碼策略配置',
        saveHint: '開啟後，使用者修改密碼時將依下方正則校驗新密碼強度；關閉時不強制校驗。本頁僅維護密碼強度策略，不涉及任何加解密金鑰。',
        strongPassword: '啟用密碼強度校驗',
        passwordStrengthValidation: '密碼強度正則',
        passwordStrengthValidationHint: 'JavaScript 正規表示式，啟用後用於校驗新密碼',
        passwordStrengthValidationMsg: '校驗失敗提示',
        updateTime: '最近更新',
        saveSuccess: '密碼策略配置已儲存',
        validationEmpty: '密碼強度正則不能為空',
        validationInvalid: '密碼強度正則無效或無法編譯',
        validationMessageInvalid: '校驗失敗提示不能為空',
        loadFailed: '載入密碼策略配置失敗',
    },
};
