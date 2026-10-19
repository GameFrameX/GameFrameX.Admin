export default {
  gameBusiness: {
    openAccess: {
      // 列表标题 / 菜单
      listTitle: '应用开放访问凭证',
      entityId: '凭证ID',

      // 字段标签
      appId: '应用',
      bindUser: '绑定用户',
      bindUserId: '绑定用户',
      scenario: '责任场景',
      purpose: '使用目的',
      subjectName: '责任主体',
      scopes: '接口范围',
      accessKey: '访问 Key',
      isEnabled: '状态',
      expiresTime: '过期时间',
      revokedTime: '撤销时间',
      revokeReason: '撤销原因',
      createdTime: '创建时间',
      noExpiry: '不过期',

      // 状态
      enabled: '已启用',
      disabled: '已禁用',
      revoked: '已撤销',

      // 签发弹窗
      issueTitle: '签发凭证',
      editTitle: '编辑凭证',
      issueHint: '签发成功后 AccessSecret 仅展示一次，请立即保存。',
      pleaseSelectBindUser: '请选择绑定用户',
      pleaseEnterScenario: '请输入责任场景编码',
      pleaseEnterPurpose: '请输入使用目的',
      pleaseEnterSubjectName: '请输入责任主体',
      pleaseSelectScopes: '请选择接口范围',
      pleaseSelectExpiresTime: '选择过期时间（留空表示不过期）',

      // 校验
      bindUserRequired: '请选择绑定用户',
      scenarioRequired: '责任场景编码不能为空',
      purposeRequired: '使用目的不能为空',
      subjectNameRequired: '责任主体不能为空',
      scopesRequired: '请至少选择一个接口范围',

      // Scopes 选项
      scopePackageReport: '分包上报 (package-report)',

      // 操作按钮
      issue: '签发',
      edit: '编辑',
      revoke: '吊销',
      enable: '启用',
      disable: '禁用',
      refresh: '轮换密钥',
      delete: '删除',

      // 操作弹窗
      revokeTitle: '吊销凭证',
      enableTitle: '启用凭证',
      disableTitle: '禁用凭证',
      refreshTitle: '轮换密钥',
      reasonLabel: '操作原因',
      reasonPlaceholder: '请输入操作原因，将记录到操作审计',
      reasonRequired: '吊销凭证必须填写操作原因',
      reasonTooLong: '操作原因不能超过 256 字',
      revokeWarning: '吊销后凭证将立即失效且不可恢复，确定继续？',
      refreshWarning: '轮换后原 AccessSecret 立即失效，新 Secret 仅展示一次。',
      actionRowLabel: '操作凭证',

      // 二次确认
      deleteConfirm: '确定删除该凭证吗',
      enableConfirm: '确定启用该凭证吗',
      disableConfirm: '确定禁用该凭证吗',

      // Secret 一次性展示弹窗
      secretTitle: '凭证密钥（仅此一次展示）',
      secretWarning: 'AccessSecret 仅此一次展示，关闭后不可再查，请立即复制保存！',
      copyKey: '复制 Key',
      copySecret: '复制 Secret',
      copySuccess: '已复制到剪贴板',
      copyFailed: '复制失败，请手动选择复制',
      iHaveSaved: '我已保存，关闭',

      // 操作结果
      issueSuccess: '签发成功',
      updateSuccess: '更新成功',
      deleteSuccess: '删除成功',
      revokeSuccess: '吊销成功',
      enableSuccess: '启用成功',
      disableSuccess: '禁用成功',
      refreshSuccess: '轮换成功',

      // 搜索占位
      pleaseEnterScenarioSearch: '请输入责任场景',
      pleaseEnterSubjectNameSearch: '请输入责任主体',
      pleaseEnterAccessKeySearch: '请输入访问 Key',
      pleaseSelectStatus: '请选择状态',
    },
  },
}
