export default {
    platformPasswordPolicyConfig: {
        title: 'Password Policy',
        saveHint: 'When enabled, new passwords are validated against the regex below; when disabled, no strength check is enforced. This page only manages password strength policy and does not expose any encryption keys.',
        strongPassword: 'Enable password strength check',
        passwordStrengthValidation: 'Password strength regex',
        passwordStrengthValidationHint: 'A JavaScript regular expression used to validate new passwords when enabled',
        passwordStrengthValidationMsg: 'Validation failure message',
        updateTime: 'Last updated',
        saveSuccess: 'Password policy saved',
        validationEmpty: 'Password strength regex cannot be empty',
        validationInvalid: 'Password strength regex is invalid or cannot be compiled',
        validationMessageInvalid: 'Validation failure message cannot be empty',
        loadFailed: 'Failed to load password policy',
    },
};
