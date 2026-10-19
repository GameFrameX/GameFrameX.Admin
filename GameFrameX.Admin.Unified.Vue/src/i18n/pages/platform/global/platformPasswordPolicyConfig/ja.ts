export default {
    platformPasswordPolicyConfig: {
        title: 'パスワードポリシー設定',
        saveHint: '有効にすると、パスワード変更時に下記の正規表現で新しいパスワードの強度を検証します。無効の場合は検証を行いません。このページではパスワード強度ポリシーのみを管理し、暗号鍵は取り扱いません。',
        strongPassword: 'パスワード強度検証を有効化',
        passwordStrengthValidation: 'パスワード強度正規表現',
        passwordStrengthValidationHint: '有効時に新しいパスワードの検証へ使用する JavaScript 正規表現',
        passwordStrengthValidationMsg: '検証失敗メッセージ',
        updateTime: '最終更新',
        saveSuccess: 'パスワードポリシーを保存しました',
        validationEmpty: 'パスワード強度正規表現は空にできません',
        validationInvalid: 'パスワード強度正規表現が無効、またはコンパイルできません',
        validationMessageInvalid: '検証失敗メッセージは空にできません',
        loadFailed: 'パスワードポリシーの読み込みに失敗しました',
    },
};
