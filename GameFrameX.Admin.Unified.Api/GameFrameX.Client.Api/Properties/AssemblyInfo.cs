using System.Runtime.CompilerServices;

// 暴露 internal 配置解析辅助方法（如 ClientApiTenantOptionsProvider.ParseAdMobKeys）给单测程序集
[assembly: InternalsVisibleTo("GameFrameX.Core.Tests")]